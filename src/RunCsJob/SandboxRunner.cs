using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using FluentAssertions.Common;
using log4net;
using Newtonsoft.Json;
using RunCsJob.Api;
using uLearn;

namespace RunCsJob
{
	public class SandboxRunnerSettings
	{
		public SandboxRunnerSettings()
		{
			var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			WorkingDirectory = new DirectoryInfo(Path.Combine(baseDirectory, "submissions"));
		}

		private const int timeLimitInSeconds = 10;
		public TimeSpan TimeLimit = TimeSpan.FromSeconds(timeLimitInSeconds);
		public TimeSpan IdleTimeLimit = TimeSpan.FromSeconds(2 * timeLimitInSeconds);
		public int MemoryLimit = 64 * 1024 * 1024;
		public int OutputLimit = 10 * 1024 * 1024;
		public MsBuildSettings MsBuildSettings = new MsBuildSettings();
		public DirectoryInfo WorkingDirectory;
		public bool DeleteSubmissionsAfterFinish;
	}

	public class SandboxRunner
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(SandboxRunner));

		private readonly RunnerSubmission submission;
		private readonly SandboxRunnerSettings settings;

		private bool hasTimeLimit;
		private bool hasMemoryLimit;
		private bool hasOutputLimit;

		private readonly RunningResults result = new RunningResults();

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
				}
			}

			var randomSuffix = Guid.NewGuid().ToString("D");
			randomSuffix = randomSuffix.Substring(randomSuffix.Length - 8);
			var submissionCompilationDirectory = workingDirectory.GetSubdir($"{submission.Id}-{randomSuffix}");
			try
			{
				submissionCompilationDirectory.Create();
			}
			catch (Exception e)
			{
				log.Error($"Не могу создать директорию для компиляции решения: {submissionCompilationDirectory.FullName}", e);
				return new RunningResults
				{
					Id = submission.Id,
					Verdict = Verdict.SandboxError,
					Error = e.ToString()
				};
			}

			try
			{
				try
				{
					if (submission is ProjRunnerSubmission)
						return new SandboxRunner(submission, settings).RunMsBuild(submissionCompilationDirectory.FullName);
					return new SandboxRunner(submission, settings).RunCsc60(submissionCompilationDirectory.FullName);
				}
				catch (Exception ex)
				{
					log.Error(ex);
					return new RunningResults
					{
						Id = submission.Id,
						Verdict = Verdict.SandboxError,
						Error = ex.ToString()
					};
				}
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
			result.Id = submission.Id;
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
				return new RunningResults
				{
					Id = submission.Id,
					Verdict = Verdict.SandboxError,
					Error = ex.ToString()
				};
			}

			log.Info($"Компилирую решение {submission.Id}: {projSubmission.ProjectFileName} в папке {dir.FullName}");

			var builderResult = MsBuildRunner.BuildProject(settings.MsBuildSettings, projSubmission.ProjectFileName, dir);
			result.Verdict = Verdict.Ok;

			if (!builderResult.Success)
			{
				log.Info($"Решение {submission.Id} не скомпилировалось: {builderResult.ToString().RemoveNewLines()}");
				result.Verdict = Verdict.CompilationError;
				result.CompilationOutput = builderResult.ToString();
				return result;
			}
			RunSandboxer($"\"{builderResult.PathToExe}\" {submission.Id}");
			return result;
		}

		public RunningResults RunCsc60(string submissionCompilationDirectory)
		{
			log.Info($"Запускаю проверку C#-решения {submission.Id}, компилирую с помощью Roslyn");
			var res = AssemblyCreator.CreateAssemblyWithRoslyn((FileRunnerSubmission)submission, submissionCompilationDirectory);

			result.Verdict = Verdict.Ok;
			result.AddCompilationInfo(res.EmitResult.Diagnostics);

			if (result.IsCompilationError())
			{
				log.Error($"Ошибка компиляции:\n{result.CompilationOutput}");
				SafeRemoveFile(res.PathToAssembly);
				return result;
			}

			if (!submission.NeedRun)
			{
				SafeRemoveFile(res.PathToAssembly);
				return result;
			}

			var valueTupleDllPath = Path.Combine(submissionCompilationDirectory, "System.ValueTuple.dll");
			SafeCopyFileIfNotExists(typeof(ValueTuple).Assembly.Location, valueTupleDllPath);
			RunSandboxer($"\"{Path.GetFullPath(res.PathToAssembly)}\" {submission.Id}");

			SafeRemoveFile(res.PathToAssembly);
			SafeRemoveFile(valueTupleDllPath);

			return result;
		}

		private static void SafeCopyFileIfNotExists(string sourceFilePath, string destFilePath)
		{
			try
			{
				if(!File.Exists(destFilePath))
					File.Copy(sourceFilePath, destFilePath);
			}
			catch
			{
			}
		}

		private static void SafeRemoveDirectory(string path)
		{
			try
			{
				Directory.Delete(path, true);
			}
			catch
			{
			}
		}

		private static void SafeRemoveFile(string path)
		{
			try
			{
				File.Delete(path);
			}
			catch
			{
			}
		}

		private void RunSandboxer(string args)
		{
			var workingDirectory = AppDomain.CurrentDomain.BaseDirectory;
			log.Info($"Запускаю C#-песочницу с аргументами: {args}\nРабочая директория: {workingDirectory}");

			var startInfo = new ProcessStartInfo(Path.Combine(workingDirectory, "CsSandboxer.exe"), args)
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

			Process sandboxer;
			try
			{
				sandboxer = Process.Start(startInfo);
			}
			catch (Exception e)
			{
				log.Error("Не могу запустить C#-песочницу", e);
				result.Verdict = Verdict.SandboxError;
				result.Error = "Can't start process";
				return;
			}

			if (sandboxer == null)
			{
				log.Error("Не могу запустить C#-песочницу. Process.Start() вернул NULL");
				result.Verdict = Verdict.SandboxError;
				result.Error = "Can't start process";
				return;
			}

			var stderrReader = new AsyncReader(sandboxer.StandardError, settings.OutputLimit + 1);

			var readyState = sandboxer.StandardOutput.ReadLineAsync();
			if (!readyState.Wait(settings.TimeLimit) || readyState.Result != "Ready")
			{
				if (!sandboxer.HasExited)
				{
					log.Error($"Песочница не завершилась через {settings.TimeLimit.TotalSeconds} секунд, убиваю её");
					sandboxer.Kill();
					result.Verdict = Verdict.SandboxError;
					result.Error = "Sandbox does not respond";
					return;
				}
				if (sandboxer.ExitCode != 0)
				{
					HandleNonZeroExitCode(stderrReader.GetData(), sandboxer.ExitCode);
					return;
				}
				result.Verdict = Verdict.SandboxError;
				result.Error = "Sandbox exit before respond";
				return;
			}

			sandboxer.Refresh();
			var startUsedMemory = sandboxer.WorkingSet64;
			var startUsedTime = sandboxer.TotalProcessorTime;
			var startTime = DateTime.Now;

			sandboxer.StandardInput.WriteLine("Run");
			sandboxer.StandardInput.WriteLineAsync(submission.Input);

			var stdoutReader = new AsyncReader(sandboxer.StandardOutput, settings.OutputLimit + 1);
			while (!sandboxer.HasExited
					&& !IsTimeLimitExpected(sandboxer, startTime, startUsedTime)
					&& !IsMemoryLimitExpected(sandboxer, startUsedMemory)
					&& !IsOutputLimit(stdoutReader)
					&& !IsOutputLimit(stderrReader))
			{
			}

			if (!sandboxer.HasExited)
				sandboxer.Kill();

			if (hasOutputLimit)
			{
				result.Verdict = Verdict.OutputLimit;
				return;
			}

			if (hasTimeLimit)
			{
				log.Error("Программа превысила ограничение по времени");
				result.Verdict = Verdict.TimeLimit;
				return;
			}

			if (hasMemoryLimit)
			{
				log.Error("Программа превысила ограничение по памяти");
				result.Verdict = Verdict.MemoryLimit;
				return;
			}

			sandboxer.WaitForExit();
			if (sandboxer.ExitCode != 0)
			{
				HandleNonZeroExitCode(stderrReader.GetData(), sandboxer.ExitCode);
				return;
			}

			result.Output = stdoutReader.GetData();
			result.Error = stderrReader.GetData();
		}

		private void HandleNonZeroExitCode(string error, int exitCode)
		{
			var obj = FindSerializedException(error);

			if (obj != null)
				result.HandleException(obj);
			else
				HandleNtStatus(exitCode, error);
		}

		private void HandleNtStatus(int exitCode, string error)
		{
			switch ((uint)exitCode)
			{
				case 0xC00000FD:
					result.Verdict = Verdict.RuntimeError;
					result.Error = "Stack overflow exception.";
					break;
				default:
					result.Verdict = Verdict.SandboxError;
					result.Error = string.IsNullOrWhiteSpace(error) ? "Non-zero exit code" : error;
					result.Error += $"\nExit code: 0x{exitCode:X8}";
					break;
			}
		}

		private bool IsOutputLimit(AsyncReader reader)
		{
			return hasOutputLimit = hasOutputLimit
									|| (reader.ReadedLength > settings.OutputLimit);
		}

		private bool IsMemoryLimitExpected(Process sandboxer, long startUsedMemory)
		{
			sandboxer.Refresh();
			long mem;
			try
			{
				mem = sandboxer.PeakWorkingSet64;
			}
			catch
			{
				return hasMemoryLimit;
			}

			return hasMemoryLimit = hasMemoryLimit
									|| startUsedMemory + settings.MemoryLimit < mem;
		}

		private bool IsTimeLimitExpected(Process sandboxer, DateTime startTime, TimeSpan startUsedTime)
		{
			return hasTimeLimit = hasTimeLimit
								|| settings.TimeLimit.Add(startUsedTime).CompareTo(sandboxer.TotalProcessorTime) < 0
								|| startTime.Add(settings.IdleTimeLimit).CompareTo(DateTime.Now) < 0;
		}

		private static Exception FindSerializedException(string str)
		{
			if (!str.EndsWith("}"))
				return null;

			var pos = str.LastIndexOf(Environment.NewLine, StringComparison.Ordinal);

			if (pos == -1)
				return null;

			var jsonSettings = new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			};

			try
			{
				var obj = JsonConvert.DeserializeObject(str.Substring(pos), jsonSettings);
				return obj as Exception;
			}
			catch
			{
				return null;
			}
		}
	}
}