using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading;
using log4net;
using Metrics;
using Newtonsoft.Json;
using RunCsJob.Api;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Configuration;

namespace RunCsJob
{
	public class SandboxRunnerSettings
	{
		public SandboxRunnerSettings()
		{
			var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			WorkingDirectory = new DirectoryInfo(Path.Combine(baseDirectory, "submissions"));
		}

		public TimeSpan CompilationTimeLimit = TimeSpan.FromSeconds(10);
		private const int timeLimitInSeconds = 10;		
		public TimeSpan TimeLimit = TimeSpan.FromSeconds(timeLimitInSeconds);
		public TimeSpan IdleTimeLimit = TimeSpan.FromSeconds(2 * timeLimitInSeconds);
		public int MemoryLimit = 64 * 1024 * 1024;
		public int OutputLimit = 10 * 1024 * 1024;
		public TimeSpan WaitSandboxAfterKilling = TimeSpan.FromSeconds(5);
		public MsBuildSettings MsBuildSettings = new MsBuildSettings();
		public DirectoryInfo WorkingDirectory;
		public bool DeleteSubmissionsAfterFinish;
	}

	public class SandboxRunner
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(SandboxRunner));
		private readonly MetricSender metricSender = new MetricSender("runcsjob");

		private readonly RunnerSubmission submission;
		private readonly SandboxRunnerSettings settings;

		private bool hasTimeLimit;
		private bool hasMemoryLimit;
		private bool hasOutputLimit;

		public static RunningResults Run(RunnerSubmission submission, SandboxRunnerSettings settings = null)
		{
			settings = settings ?? new SandboxRunnerSettings();
			var workingDirectory = settings.WorkingDirectory;
			if (!workingDirectory.Exists)
			{
				try
				{
					workingDirectory.Create();
				}
				catch (Exception e)
				{
					log.Error($"Не могу создать директорию для компиляции решений: {workingDirectory}", e);
					return new RunningResults(submission.Id, Verdict.SandboxError, error: e.ToString());
				}
			}

			var randomSuffix = Guid.NewGuid().ToString("D");
			randomSuffix = randomSuffix.Substring(randomSuffix.Length - 8);
			var submissionCompilationDirectory = workingDirectory.GetSubdirectory($"{submission.Id}-{randomSuffix}");
			try
			{
				submissionCompilationDirectory.Create();
			}
			catch (Exception e)
			{
				log.Error($"Не могу создать директорию для компиляции решения: {submissionCompilationDirectory.FullName}", e);
				return new RunningResults(submission.Id, Verdict.SandboxError, error: e.ToString());
			}

			try
			{
				RunningResults result;
				var instance = new SandboxRunner(submission, settings);
				if (submission is ProjRunnerSubmission)
					result = instance.RunMsBuild(submissionCompilationDirectory.FullName);
				else
					result = instance.RunCsc(submissionCompilationDirectory.FullName);
				result.Id = submission.Id;
				return result;
			}
			catch (Exception ex)
			{
				log.Error(ex.Message, ex);
				return new RunningResults(submission.Id, Verdict.SandboxError, error: ex.ToString());
			}
			finally
			{
				if (settings.DeleteSubmissionsAfterFinish)
				{
					log.Info($"Удаляю папку с решением: {submissionCompilationDirectory}");
					SafeRemoveDirectory(submissionCompilationDirectory.FullName);
				}
			}
		}

		public SandboxRunner(RunnerSubmission submission, SandboxRunnerSettings settings = null)
		{
			this.submission = submission;
			this.settings = settings ?? new SandboxRunnerSettings();
		}

		private RunningResults RunMsBuild(string submissionCompilationDirectory)
		{
			var projSubmission = (ProjRunnerSubmission)submission;
			log.Info($"Запускаю проверку C#-решения {projSubmission.Id}, компилирую с помощью MsBuild");
			var dir = new DirectoryInfo(submissionCompilationDirectory);

			try
			{
				Utils.UnpackZip(projSubmission.ZipFileData, dir.FullName);
			}
			catch (Exception ex)
			{
				log.Error("Не могу распаковать решение", ex);
				return new RunningResults(submission.Id, Verdict.SandboxError, error: ex.ToString());
			}

			log.Info($"Компилирую решение {submission.Id}: {projSubmission.ProjectFileName} в папке {dir.FullName}");

			var builderResult = MsBuildRunner.BuildProject(settings.MsBuildSettings, projSubmission.ProjectFileName, dir);

			if (!builderResult.Success)
			{
				log.Info($"Решение {submission.Id} не скомпилировалось: {builderResult.ToString().Replace("\n", @"\n")}");
				return new RunningResults(Verdict.CompilationError, compilationOutput: builderResult.ToString());
			}

			return RunSandbox($"\"{builderResult.PathToExe}\" {submission.Id}");
		}

		public RunningResults RunCsc(string submissionCompilationDirectory)
		{
			log.Info($"Запускаю проверку C#-решения {submission.Id}, компилирую с помощью Roslyn");

			var res = AssemblyCreator.CreateAssemblyWithRoslyn((FileRunnerSubmission)submission, submissionCompilationDirectory, settings.CompilationTimeLimit);

			try
			{
				metricSender.SendTiming("exercise.compilation.csc.elapsed", (int) res.Elapsed.TotalMilliseconds);
				var diagnostics = res.EmitResult.Diagnostics;
				var compilationOutput = diagnostics.DumpCompilationOutput();

				if (diagnostics.HasErrors())
				{
					log.Error($"Ошибка компиляции:\n{compilationOutput}");
					return new RunningResults(Verdict.CompilationError, compilationOutput: compilationOutput);
				}

				if (!submission.NeedRun)
					return new RunningResults(Verdict.Ok);

				return RunSandbox($"\"{Path.GetFullPath(res.PathToAssembly)}\" {submission.Id}");
			}
			finally
			{
				SafeRemoveFile(res.PathToAssembly);
			}
		}

		private static void SafeCopyFileIfNotExists(string sourceFilePath, string destFilePath)
		{
			try
			{
				if (!File.Exists(destFilePath))
					File.Copy(sourceFilePath, destFilePath);
			}
			catch (Exception e)
			{
				log.Warn($"Произошла ошибка при копировании файла {sourceFilePath} в {destFilePath}, но я её проигнорирую", e);
			}
		}

		private static void SafeRemoveDirectory(string path)
		{
			try
			{
				/* Sometimes we can't remove directory after Time Limit Exceeded, because process is alive yet. Just wait some seconds before directory removing */ 
				FuncUtils.TrySeveralTimes(() =>
				{
					Directory.Delete(path, true);
					return true;
				}, 3, () => Thread.Sleep(TimeSpan.FromSeconds(1)));
			}
			catch (Exception e)
			{
				log.Warn($"Произошла ошибка при удалении директории {path}, но я её проигнорирую", e);
			}
		}

		private static void SafeRemoveFile(string path)
		{
			try
			{
				File.Delete(path);
			}
			catch (Exception e)
			{
				log.Warn($"Произошла ошибка при удалении файла {path}, но я её проигнорирую", e);
			}
		}

		private RunningResults RunSandbox(string sandboxArguments)
		{
			var sandbox = StartSandboxProcess(sandboxArguments);
			string sandboxOutput;
			string sandboxError;
			try
			{
				WaitUntilSandboxIsReady(sandbox);
				log.Info("Песочница ответила Ready");
				RunSolutionAndWaitUntilSandboxExit(sandbox, out sandboxOutput, out sandboxError);
			}
			finally
			{
				if (!sandbox.HasExited)
				{
					try
					{
						sandbox.Kill();
					}
					catch (Win32Exception)
					{
						/* Sometimes we can catch Access Denied error because the process is already terminating. It's ok, we don't need to rethrow exception */	
					}
				}
			}

			if (hasOutputLimit)
			{
				log.Warn("Программа вывела слишком много");
				return new RunningResults(Verdict.OutputLimit);
			}

			if (hasTimeLimit)
			{
				log.Warn("Программа превысила ограничение по времени");
				return new RunningResults(Verdict.TimeLimit);
			}

			if (hasMemoryLimit)
			{
				log.Warn("Программа превысила ограничение по памяти");
				return new RunningResults(Verdict.MemoryLimit);
			}

			sandbox.WaitForExit((int)settings.WaitSandboxAfterKilling.TotalMilliseconds);
			if (!sandbox.HasExited)
				return new RunningResults(Verdict.SandboxError, error: "Can't kill sandbox");
			if (sandbox.ExitCode != 0)
				return GetResultForNonZeroExitCode(sandboxError, sandbox.ExitCode);

			return new RunningResults(Verdict.Ok, output: sandboxOutput, error: sandboxError);
		}

		private void RunSolutionAndWaitUntilSandboxExit(Process sandbox, out string sandboxOutput, out string sandboxError)
		{
			sandbox.Refresh();
			var startUsedMemory = sandbox.WorkingSet64;
			var startUsedTime = sandbox.TotalProcessorTime;
			var startTime = DateTime.Now;

			sandbox.StandardInput.WriteLine("Run");
			sandbox.StandardInput.Flush();
			sandbox.StandardInput.WriteLineAsync(submission.Input);

			var stderrReader = new AsyncReader(sandbox.StandardError, settings.OutputLimit + 1);
			var stdoutReader = new AsyncReader(sandbox.StandardOutput, settings.OutputLimit + 1);
			while (!sandbox.HasExited
					&& !CheckIsTimeLimitExceeded(sandbox, startTime, startUsedTime)
					&& !CheckIsMemoryLimitExceeded(sandbox, startUsedMemory)
					&& !CheckIsOutputLimit(stdoutReader)
					&& !CheckIsOutputLimit(stderrReader))
			{
			}

			if (!hasTimeLimit && !hasMemoryLimit && !hasOutputLimit)
			{
				/* Read all data to the end of streams */
				sandboxError = stderrReader.GetData();
				sandboxOutput = stdoutReader.GetData();
				hasOutputLimit = CheckIsOutputLimit(stdoutReader);
				hasOutputLimit = CheckIsOutputLimit(stderrReader);
			}
			else
			{
				sandboxError = "";
				sandboxOutput = "";
			}
		}

		private void WaitUntilSandboxIsReady(Process sandbox)
		{
			var readyLineReadTask = sandbox.StandardOutput.ReadLineAsync();
			if (readyLineReadTask.Wait(settings.TimeLimit) && readyLineReadTask.Result == "Ready")
				return;

			if (!sandbox.HasExited)
				throw new SandboxErrorException($"Песочница не ответила «Ready» через {settings.TimeLimit.TotalSeconds} секунд после запуска, убиваю её");

			if (sandbox.ExitCode != 0)
			{
				log.Warn($"Песочница не ответила «Ready», а вышла с кодом {sandbox.ExitCode}");
				var stderrReader = new AsyncReader(sandbox.StandardError, settings.OutputLimit + 1);
				var stderrData = stderrReader.GetData();
				log.Warn($"Вывод песочницы в stderr:\n{stderrData}");
				var result = GetResultForNonZeroExitCode(stderrData, sandbox.ExitCode);
				throw new SandboxErrorException(result.Error);
			}

			throw new SandboxErrorException("Sandbox unexpectedly exited before respond");
		}

		private Process StartSandboxProcess(string sandboxArguments)
		{
			var workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
			log.Info($"Запускаю C#-песочницу с аргументами: {sandboxArguments}\nРабочая директория: {workingDirectory}");

			var startInfo = new ProcessStartInfo(Path.Combine(workingDirectory, "CsSandboxer.exe"), sandboxArguments)
			{
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
				StandardOutputEncoding = Encoding.UTF8,
				StandardErrorEncoding = Encoding.UTF8
			};
			startInfo.EnvironmentVariables.Add("InsideSandbox", "true");

			Process sandbox;
			try
			{
				sandbox = Process.Start(startInfo);
			}
			catch (Exception e)
			{
				throw new SandboxErrorException("Не смог запустить C#-песочницу", e);
			}

			if (sandbox == null)
				throw new SandboxErrorException("Не смог запустить C#-песочницу. Process.Start() вернул NULL");

			return sandbox;
		}

		private RunningResults GetResultForNonZeroExitCode(string error, int exitCode)
		{
			var obj = ParseSerializedException(error);
			if (obj != null)
				return GetResultFromException(obj);

			var exception = ParseNotSerializedException(error);
			if (exception != null)
				return new RunningResults(Verdict.RuntimeError, error: error);

			log.Warn($"Не вытащил информацию об исключении из строчки \"{error}\", проверяю код выхода: {exitCode}");
			return GetResultFromNtStatus(exitCode, error);
		}

		private RunningResults GetResultFromNtStatus(int exitCode, string error)
		{
			if ((uint)exitCode == 0xC00000FD)
				return new RunningResults(Verdict.RuntimeError, error: "Stack overflow exception");

			var errorMessage = string.IsNullOrWhiteSpace(error) ? "Non-zero exit code" : error;
			errorMessage += $"\nExit code: 0x{exitCode:X8}";
			return new RunningResults(Verdict.SandboxError, error: errorMessage);
		}

		private bool CheckIsOutputLimit(AsyncReader reader)
		{
			return hasOutputLimit = hasOutputLimit || reader.ReadedLength > settings.OutputLimit;
		}

		private bool CheckIsMemoryLimitExceeded(Process sandbox, long startUsedMemory)
		{
			sandbox.Refresh();
			long mem;
			try
			{
				mem = sandbox.PeakWorkingSet64;
			}
			catch
			{
				return hasMemoryLimit;
			}

			return hasMemoryLimit = hasMemoryLimit || startUsedMemory + settings.MemoryLimit < mem;
		}

		private bool CheckIsTimeLimitExceeded(Process sandbox, DateTime startTime, TimeSpan startUsedTime)
		{
			return hasTimeLimit = hasTimeLimit
								|| settings.TimeLimit.Add(startUsedTime).CompareTo(sandbox.TotalProcessorTime) < 0
								|| startTime.Add(settings.IdleTimeLimit).CompareTo(DateTime.Now) < 0;
		}

		private static Exception ParseSerializedException(string stderr)
		{
			if (!stderr.EndsWith("}"))
				return null;

			var pos = stderr.LastIndexOf(Environment.NewLine, StringComparison.Ordinal);

			if (pos == -1)
				return null;

			var jsonSettings = new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			};

			try
			{
				var obj = JsonConvert.DeserializeObject(stderr.Substring(pos), jsonSettings);
				return obj as Exception;
			}
			catch
			{
				return null;
			}
		}

		/* Sometimes we've received something like this:
		   Unhandled Exception: System.ArgumentException: Value does not fall within the expected range.
			   at Memory.API.MagicAPI.Free(Int32 id)
			   at Memory.API.APIObject.Finalize()
			
		   It's possible, i.e., for exceptions thrown from destructors. Let's try to parse them! */
		private static Exception ParseNotSerializedException(string stderr)
		{
			stderr = stderr.Trim();
			
			const string unhandledException = "Unhandled Exception: ";
			if (!stderr.StartsWith(unhandledException))
				return null;
			
			log.Info($"Try to parse not-serialized and not-standard exception from following message: {stderr}");

			var lines = stderr.SplitToLines();
			var exceptionLine = lines[0].Substring(unhandledException.Length);
			var exceptionLineColonIndex = exceptionLine.IndexOf(':');
			if (exceptionLineColonIndex >= 0)
			{
				var exceptionTypeName = exceptionLine.Substring(0, exceptionLineColonIndex);
				var exceptionMessage = exceptionLine.Length > exceptionLineColonIndex + 2 ? exceptionLine.Substring(exceptionLineColonIndex + 2) : "";
				
				var exceptionType = typeof(Exception).Assembly.GetTypes().FirstOrDefault(t => t.FullName == exceptionTypeName);
				if (exceptionType == null)
					return new Exception(exceptionMessage);

				log.Info($"Defined exception type: {exceptionType.FullName}");

				var exception = TryToCreateExceptionByTypeAndMessage(exceptionType, exceptionMessage);
				if (exception == null)
					return new Exception(exceptionMessage);

				return exception;
			}

			return null;
		}

		private static Exception TryToCreateExceptionByTypeAndMessage(Type exceptionType, string exceptionMessage)
		{
			Exception exceptionObject = null;
			try
			{
				exceptionObject = Activator.CreateInstance(exceptionType, exceptionMessage) as Exception;
			}
			catch (Exception e)
			{
				log.Warn($"Can't create exception object of type {exceptionType}: {e.Message}");
			}

			if (exceptionObject == null)
			{
				try
				{
					exceptionObject = Activator.CreateInstance(exceptionType) as Exception;
				}
				catch (Exception e)
				{
					log.Warn($"Can't create exception object of type {exceptionType}: {e.Message}");
					return null;
				}

				if (exceptionObject == null)
				{
					log.Warn($"Can't create exception object of type {exceptionType}, returned null");
					return null;
				}
			}

			return exceptionObject;
		}

		private static RunningResults GetResultFromException(Exception ex)
		{
			if (ex is TargetInvocationException)
				return HandleTargetInvocationException((TargetInvocationException)ex);

			return new RunningResults(Verdict.SandboxError, output: ex.ToString());
		}

		private static RunningResults HandleTargetInvocationException(TargetInvocationException ex)
		{
			return HandleInnerException((dynamic)ex.InnerException);
		}

		private static RunningResults HandleInnerException(SecurityException ex)
		{
			return new RunningResults(Verdict.SecurityException, output: ex.ToString());
		}

		private static RunningResults HandleInnerException(MemberAccessException ex)
		{
			return new RunningResults(Verdict.SecurityException, output: ex.ToString());
		}

		private static RunningResults HandleInnerException(TypeInitializationException ex)
		{
			return new RunningResults(Verdict.SecurityException, output: ex.ToString());
		}

		private static RunningResults HandleInnerException(Exception ex)
		{
			return new RunningResults(Verdict.RuntimeError, output: ex.ToString());
		}
	}

	internal class SandboxErrorException : Exception
	{
		public SandboxErrorException()
		{
		}

		public SandboxErrorException(string message)
			: base(message)
		{
		}

		public SandboxErrorException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected SandboxErrorException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}