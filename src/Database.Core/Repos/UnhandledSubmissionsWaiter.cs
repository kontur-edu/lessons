using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Vostok.Logging.Abstractions;

namespace Database.Repos
{
	public static class UnhandledSubmissionsWaiter
	{
		public static volatile ConcurrentDictionary<int, DateTime> UnhandledSubmissions = new ConcurrentDictionary<int, DateTime>();
		public static volatile ConcurrentDictionary<int, DateTime> HandledSubmissions = new ConcurrentDictionary<int, DateTime>();
		private static readonly TimeSpan handleTimeout = TimeSpan.FromMinutes(3);

		private static ILog log => LogProvider.Get().ForContext(typeof(UnhandledSubmissionsWaiter));

		public static async Task WaitUntilSubmissionHandled(TimeSpan timeout, int submissionId)
		{
			log.Info($"Вхожу в цикл ожидания результатов проверки решения {submissionId}. Жду {timeout.TotalSeconds} секунд");
			var sw = Stopwatch.StartNew();
			while (sw.Elapsed < timeout)
			{
				if (HandledSubmissions.ContainsKey(submissionId))
				{
					DateTime value;
					HandledSubmissions.TryRemove(submissionId, out value);
					return;
				}

				await Task.Delay(TimeSpan.FromMilliseconds(100));
			}
		}

		public static async Task WaitAnyUnhandledSubmissions(TimeSpan timeout)
		{
			var sw = Stopwatch.StartNew();
			while (sw.Elapsed < timeout)
			{
				if (UnhandledSubmissions.Count > 0)
				{
					log.Info($"Список невзятых пока на проверку решений: [{string.Join(", ", UnhandledSubmissions.Keys)}]");
					ClearHandleDictionaries();
					return;
				}

				await Task.Delay(TimeSpan.FromMilliseconds(100));
			}
		}

		private static void ClearHandleDictionary(ConcurrentDictionary<int, DateTime> dictionary, DateTime timeout)
		{
			foreach (var key in dictionary.Keys)
			{
				if (dictionary.TryGetValue(key, out var value) && value < timeout)
					dictionary.TryRemove(key, out value);
			}
		}

		private static void ClearHandleDictionaries()
		{
			var timeout = DateTime.Now.Subtract(handleTimeout);
			ClearHandleDictionary(HandledSubmissions, timeout);
			ClearHandleDictionary(UnhandledSubmissions, timeout);
		}
	}
}