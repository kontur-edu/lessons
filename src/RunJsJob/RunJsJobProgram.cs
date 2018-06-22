using System;
using System.Collections.Generic;
using System.Configuration;
using System.Configuration.Internal;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using log4net;
using log4net.Config;
using log4net.Repository;
using Metrics;
using RunCsJob.Api;
using uLearn;
using Ulearn.Common.Extensions;

namespace RunJsJob
{
	public class RunJsJobProgram
	{
		private readonly string address;
		private readonly string token;
		private readonly TimeSpan sleep;
		private readonly int jobsToRequest;
		private readonly string agentName;

		private readonly ManualResetEvent shutdownEvent = new ManualResetEvent(false);
		private readonly List<Thread> threads = new List<Thread>();

		private static readonly ILog log = LogManager.GetLogger(typeof(RunJsJobProgram));
		public readonly SandboxRunnerSettings Settings;

		public RunJsJobProgram(ManualResetEvent externalShutdownEvent = null)
		{
			if (externalShutdownEvent != null)
				shutdownEvent = externalShutdownEvent;

			try
			{
				address = ConfigurationManager.AppSettings["submissionsUrl"];
				token = ConfigurationManager.AppSettings["runnerToken"];
				sleep = TimeSpan.FromSeconds(int.Parse(ConfigurationManager.AppSettings["sleepSeconds"] ?? "1"));
				jobsToRequest = int.Parse(ConfigurationManager.AppSettings["jobsToRequest"] ?? "5");
				var deleteSubmissions = bool.Parse(ConfigurationManager.AppSettings["ulearn.runjsjob.deleteSubmissions"] ?? "true");
				Settings = new SandboxRunnerSettings
				{
					DeleteSubmissionsAfterFinish = deleteSubmissions,
				};
				var workingDirectory = ConfigurationManager.AppSettings["ulearn.runjsjob.submissionsWorkingDirectory"];
				if (!string.IsNullOrWhiteSpace(workingDirectory))
					Settings.WorkingDirectory = new DirectoryInfo(workingDirectory);

				agentName = ConfigurationManager.AppSettings["ulearn.runjsjob.agentName"];
				if (string.IsNullOrEmpty(agentName))
				{
					agentName = Environment.MachineName;
					log.Info($"Автоопределённое имя клиента: {agentName}. Его можно переопределить в настройках (appSettings/ulearn.runjsjob.agentName)");
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
			ConfigureLog4Net();
			ConfigureIonicZip();
			
			var program = new RunJsJobProgram();
			if (args.Contains("--selfcheck"))
				program.SelfCheck();
			else
				program.Run();
		}

		private static void ConfigureIonicZip()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}

		private static void ConfigureLog4Net()
		{
			var environmentName = ConfigurationManager.AppSettings["environmentName"];
			var configName = string.Format("log4net{0}.config",
				!string.IsNullOrEmpty(environmentName) ? '.' + environmentName : string.Empty);
			XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetEntryAssembly()),
				new FileInfo(configName));
		}

		public void Run(bool joinAllThreads = true)
		{
			log.Info($"Отправляю запросы на {address} для получения новых решений");

			var threadsCount = int.Parse(ConfigurationManager.AppSettings["ulearn.runjsjob.threadsCount"] ?? "1");
			if (threadsCount < 1)
			{
				log.Error($"Не могу определить количество потоков для запуска из конфигурации: ${threadsCount}. Количество потоков должно быть положительно");
				throw new ArgumentOutOfRangeException(nameof(threadsCount), "Number of threads (appSettings/ulearn.runjsjob.threadsCount) should be positive");
			}

			log.Info($"Запускаю {threadsCount} потока(ов)");
			for (var i = 0; i < threadsCount; i++)
			{
				threads.Add(new Thread(WorkerThread)
				{
					Name = $"RunJsJob Worker #{i}",
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
			var serviceKeepAliver = new ServiceKeepAliver("runjsjob");
			while (!shutdownEvent.WaitOne(0))
			{
				List<RunnerSubmission> newUnhandled;
				try
				{
					newUnhandled = client.TryGetSubmissions(jobsToRequest).Result;
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

				serviceKeepAliver.Ping(TimeSpan.FromMinutes(1));
				Thread.Sleep(sleep);
			}
		}

		// TODO собрать тестовый архив и реализовать
		private void SelfCheck()
		{
			throw new NotImplementedException();
			// var res = SandboxRunner.Run(new FileRunnerSubmission
			// {
			// 	Id = Utils.NewNormalizedGuid(),
			// 	NeedRun = true,
			// 	Code = "console.log('Привет мир')",
			// }, Settings);
			// log.Info(res);
		}
	}
}