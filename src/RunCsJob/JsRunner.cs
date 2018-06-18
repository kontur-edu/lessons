using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
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

			var shellCommand = BuildDockerCommand(settings, dir);

			return RunDocker(settings, dir, shellCommand).Result;
		}

		private static async Task<RunningResults> RunDocker(SandboxRunnerSettings settings, DirectoryInfo dir, string shellCommand)
		{
			using (var cli = new Cli("cmd.exe"))
			using (var cts = new CancellationTokenSource())
			{
				cts.CancelAfter(settings.IdleTimeLimit);
				var token = cts.Token;
				var output = await cli.ExecuteAsync(shellCommand, token);

				if (token.IsCancellationRequested)
				{
					log.Info($"Таймаут в папке {dir.FullName}");
					return new RunningResults(Verdict.TimeLimit);
				}

				if (output.ExitCode != 0)
				{
					log.Info($"Упал docker в папке {dir.FullName}");
					return new RunningResults(Verdict.SandboxError, error: output.StandardError);
				}
			}

			var result = LoadResult(dir);

			return MakeVerdict(result);
		}

		private static RunningResults MakeVerdict(JsTestResult result)
		{
			if (result is null)
				return new RunningResults(Verdict.SandboxError);

			if (result.ui.stats.failures == 0 && result.unit.stats.failures == 0)
				return new RunningResults(Verdict.Ok);

			var firstFailedTest = result.ui.failures.FirstOrDefault() ?? result.unit.failures.First();
			return new RunningResults(Verdict.RuntimeError, error: firstFailedTest.err.message ?? "");
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

		private static string BuildDockerCommand(SandboxRunnerSettings settings, DirectoryInfo dir)
		{
			var parts = new List<string>
			{
				"docker run",
				LinkDirectory(dir, "src"),
				LinkDirectory(dir, "ui-test"),
				LinkDirectory(dir, "unit-test"),
				LinkDirectory(dir, "output"),
				"--privileged", // TODO: remove if sandbox is fixed
				"--netowrk none",
				"--restart no",
				"-it",
				"-rm",
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