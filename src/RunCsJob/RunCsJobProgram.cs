using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using log4net;
using log4net.Config;
using Metrics;
using RunCsJob.Api;
using Ulearn.Core;

namespace RunCsJob
{
	public class RunCsJobProgram
	{
		private readonly string address;
		private readonly string token;
		private readonly TimeSpan sleep;
		private readonly string agentName; 

		private readonly ManualResetEvent shutdownEvent = new ManualResetEvent(false);
		private readonly List<Thread> threads = new List<Thread>();
		
		private static readonly ILog log = LogManager.GetLogger(typeof(RunCsJobProgram));
		public readonly SandboxRunnerSettings Settings;

		public RunCsJobProgram(ManualResetEvent externalShutdownEvent = null)
		{
			if (externalShutdownEvent != null)
				shutdownEvent = externalShutdownEvent;

			try
			{
				address = ConfigurationManager.AppSettings["submissionsUrl"];
				token = ConfigurationManager.AppSettings["runnerToken"];
				sleep = TimeSpan.FromSeconds(int.Parse(ConfigurationManager.AppSettings["sleepSeconds"] ?? "1"));
				var deleteSubmissions = bool.Parse(ConfigurationManager.AppSettings["ulearn.runcsjob.deleteSubmissions"] ?? "true");
				Settings = new SandboxRunnerSettings
				{
					DeleteSubmissionsAfterFinish = deleteSubmissions,
				};
				var workingDirectory = ConfigurationManager.AppSettings["ulearn.runcsjob.submissionsWorkingDirectory"];
				if (!string.IsNullOrWhiteSpace(workingDirectory))
					Settings.WorkingDirectory = new DirectoryInfo(workingDirectory);

				agentName = ConfigurationManager.AppSettings["ulearn.runcsjob.agentName"];
				if (string.IsNullOrEmpty(agentName))
				{
					agentName = Environment.MachineName;
					log.Info($"Автоопределённое имя клиента: {agentName}. Его можно переопределить в настройках (appSettings/ulearn.runcsjob.agentName)");					
				}
			}
			catch (Exception e)
			{
				log.Error(e);
				throw;
			}
		}

		public static void Main(string[] args)
		{
			XmlConfigurator.Configure();

			var program = new RunCsJobProgram();
			if (args.Any(x => x.StartsWith("-p:")))
			{
				var path = args.FirstOrDefault(x => x.StartsWith("-p:"))?.Substring(3);
				if (path != null)
					program.Settings.MsBuildSettings.CompilerDirectory = new DirectoryInfo(path);
			}
			if (args.Contains("--selfcheck"))
				program.SelfCheck();
			else
				program.Run();
		}

		public void Run(bool joinAllThreads=true)
		{
			if (!Settings.MsBuildSettings.CompilerDirectory.Exists)
			{
				log.Error($"Не найдена папка с компиляторами: {Settings.MsBuildSettings.CompilerDirectory}");
				Environment.Exit(1);
			}
			log.Info($"Путь до компиляторов: {Settings.MsBuildSettings.CompilerDirectory}");

			AppDomain.MonitoringIsEnabled = true;
			log.Info($"Отправляю запросы на {address} для получения новых решений");
			
			var threadsCount = int.Parse(ConfigurationManager.AppSettings["ulearn.runcsjob.threadsCount"] ?? "1");
			if (threadsCount < 1)
			{
				log.Error($"Не могу определить количество потоков для запуска из конфигурации: ${threadsCount}. Количество потоков должно быть положительно");
				throw new ArgumentOutOfRangeException(nameof(threadsCount), "Number of threads (appSettings/ulearn.runcsjob.threadsCount) should be positive");
			}
			
			log.Info($"Запускаю {threadsCount} потока(ов)");
			for (var i = 0; i < threadsCount; i++)
			{
				threads.Add(new Thread(WorkerThread)
				{
					Name = $"RunCsJob Worker #{i}",
					IsBackground = true
				});
			}
			threads.ForEach(t => t.Start());
			
			if (joinAllThreads)
				threads.ForEach(t => t.Join());
		}
		
		private void WorkerThread()
		{
			log.Info($"Поток {Thread.CurrentThread.Name} запускается");
			RunOneThread();
		}

		public void Stop()
		{
			shutdownEvent.Set();
			log.Info("Получен сигнал остановки");

			foreach (var thread in threads)
			{
				log.Info($"Пробую остановить поток {thread.Name}");
				if (!thread.Join(10000))
				{
					log.Info($"Вызываю Abort() для потока {thread.Name}");
					thread.Abort();
				}
			}
		}

		private void RunOneThread()
		{
			var fullAgentName = $"{agentName}:Process={Process.GetCurrentProcess().Id}:ThreadId={Thread.CurrentThread.ManagedThreadId}:Thread={Thread.CurrentThread.Name}";
			Client client;
			try
			{
				client = new Client(address, token, fullAgentName);
			}
			catch (Exception e)
			{
				log.Error("Не могу создать HTTP-клиента для отправки запроса на ulearn", e);
				throw;
			}
			MainLoop(client);
		}

		private void MainLoop(Client client)
		{
			var serviceKeepAliver = new ServiceKeepAliver("runcsjob");
			if (!int.TryParse(ConfigurationManager.AppSettings["ulearn.runcsjob.keepAlive.interval"], out var keepAliveIntervalSeconds))
				keepAliveIntervalSeconds = 30;
			var keepAliveInterval = TimeSpan.FromSeconds(keepAliveIntervalSeconds);
			while (!shutdownEvent.WaitOne(0))
			{
				List<RunnerSubmission> newUnhandled;
				try
				{
					newUnhandled = client.TryGetSubmission().Result;
				}
				catch (Exception e)
				{
					log.Error($"Не могу получить решения из ulearn. Следующая попытка через {sleep.TotalSeconds} секунд", e);
					Thread.Sleep(sleep);
					continue;
				}

				log.Info($"Получил {newUnhandled.Count} решение(й) со следующими ID: [{string.Join(", ", newUnhandled.Select(s => s.Id))}]");

				if (newUnhandled.Any())
				{
					var results = newUnhandled.Select(unhandled => SandboxRunner.Run(unhandled, Settings)).ToList();
					log.Info($"Результаты проверки: [{string.Join(", ", results.Select(r => r.Verdict))}]");
					try
					{
						client.SendResults(results);
					}
					catch (Exception e)
					{
						log.Error("Не могу отправить результаты проверки на ulearn", e);
					}
				}
				serviceKeepAliver.Ping(keepAliveInterval);
				Thread.Sleep(sleep);
			}
		}

		private void SelfCheck()
		{
			var res = SandboxRunner.Run(new FileRunnerSubmission
			{
				Id = Utils.NewNormalizedGuid(),
				NeedRun = true,
				Code = "class C { static void Main(){ System.Console.WriteLine(\"Привет мир!\");}}"
			}, Settings);
			log.Info(res);
		}
	}
}