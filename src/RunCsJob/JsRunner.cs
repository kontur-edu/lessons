using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using RunCsJob.Api;
using Ulearn.Common.Extensions;

namespace RunCsJob
{
	public class JsRunner
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(JsRunner));

		public static RunningResults Run(SandboxRunnerSettings settings, DirectoryInfo dir)
		{
			var outputDirectory = dir.GetSubdirectory("output");
			try
			{
				outputDirectory.Create();
			}
			catch (Exception e)
			{
				log.Error($"Не могу создать директорию для результатов решения: {dir.FullName}", e);
				return new RunningResults(Verdict.SandboxError, error: e.ToString());
			}

			return RunDocker(settings, dir).Result;
		}

		private static async Task<RunningResults> RunDocker(SandboxRunnerSettings settings, DirectoryInfo dir)
		{
			{
				var name = Guid.NewGuid();
				var shellCommand = BuildDockerCommand(settings, dir, name);

				var shellProcess = BuildShellProcess(shellCommand);

				shellProcess.Start();
				var isFinished = shellProcess.WaitForExit((int)settings.IdleTimeLimit.TotalMilliseconds);

				if (!isFinished)
				{
					log.Info($"Не хватило времени на работу Docker в папке {dir.FullName}");
					shellProcess.Kill();
					Action clean = () =>
					{
						var cleanup = BuildShellProcess($"docker container rm -f {name}");
						cleanup.Start();
						var isCleanupFinished = cleanup.WaitForExit((int)settings.TimeLimit.TotalMilliseconds);
						if (isCleanupFinished)
							log.Info($"Повисший контейнер {name} очищен");
						else
							log.Error($"Не удалось очистить повисший контейнер {name}");
					};
					clean.BeginInvoke(ar => { }, null);
					
					return new RunningResults(Verdict.TimeLimit);
				}

				log.Info($"Docker закончил работу и написал: {shellProcess.StandardOutput.ReadToEnd()}");

				if (shellProcess.ExitCode != 0)
				{
					log.Info($"Упал docker в папке {dir.FullName}");
					return new RunningResults(Verdict.SandboxError, error: shellProcess.StandardError.ReadToEnd());
				}
			}

			var result = LoadResult(dir);

			return MakeVerdict(result);
		}

		private static Process BuildShellProcess(string shellCommand)
		{
			return new Process
			{
				StartInfo =
				{
					Arguments = $"/C {shellCommand}",
					FileName = "cmd.exe",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true,
					UseShellExecute = false
				}
			};
		}

		private static RunningResults MakeVerdict(JsTestResult result)
		{
			if (result is null)
				return new RunningResults(Verdict.SandboxError);

			var hasUiTests = result.ui.stats != null;
			var hasUnitTests = result.unit.stats != null;

			if (hasUiTests && result.ui.stats.failures != 0)
			{
				return new RunningResults(Verdict.RuntimeError, error: result.ui.failures.First().err.message);
			}

			if (hasUnitTests && result.unit.stats.failures != 0)
			{
				return new RunningResults(Verdict.RuntimeError, error: result.unit.failures.First().err.message);
			}

			return new RunningResults(Verdict.Ok);
		}

		private static JsTestResult LoadResult(DirectoryInfo dir)
		{
			try
			{
				var resultPath = Path.Combine(dir.GetSubdirectory("output").FullName, "result.json");
				using (StreamReader r = new StreamReader(resultPath))
				{
					var json = r.ReadToEnd();
					try
					{
						var result = JsonConvert.DeserializeObject<JsTestResult>(json);
						return result;
					}
					catch (Exception e)
					{
						log.Info($"Не удалось распарсить результат тестов в папке {dir.FullName}");
						throw;
					}
				}
			}
			catch (Exception e)
			{
				log.Info($"Не удалось прочитать результат тестов в папке {dir.FullName}");
				return null;
			}
		}

		private static string BuildDockerCommand(SandboxRunnerSettings settings, DirectoryInfo dir, Guid name)
		{
			var parts = new List<string>
			{
				"docker run",
				LinkDirectory(dir, "src"),
				LinkDirectory(dir, "tests"),
				LinkDirectory(dir, "output"),
				//$"--security-opt seccomp=$(pwd)/chrome.json",
				"--privileged",
				"--network none",
				"--restart no",
				"--rm",
				$"--name {name}",
				"-it",
				$"-m {settings.MemoryLimit}b",
				"js-sandbox",
			};
			return string.Join(" ", parts);
		}

		private static string LinkDirectory(DirectoryInfo rootDirectory, string subdirectory) => $"-v {ConvertToUnixPath(rootDirectory.GetSubdirectory(subdirectory))}:/app/{subdirectory}";

		private static string ConvertToUnixPath(DirectoryInfo dir) => dir.FullName.Replace(@"\", "/");
	}

	public class JsTestResult
	{
		public MochaResult ui;
		public MochaResult unit;
	}

	public class MochaResult
	{
		public List<MochaTest> failures;
		public List<MochaTest> passes;
		public List<MochaTest> pending;
		public MochaStats stats;
		public List<MochaTest> tests;
	}

	public class MochaTest
	{
		public int currentRetry;
		public int duration;
		public JsError err;
		public string fullTitle;
		public string title;
	}

	public class JsError
	{
		public string message;
		public string stack;
	}

	public class MochaStats
	{
		public int duration;
		public DateTime end;
		public int failures;
		public int passes;
		public int pending;
		public DateTime start;
		public int suites;
		public int tests;
	}
}