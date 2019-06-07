using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Database;
using Database.DataContexts;
using Graphite;
using log4net;
using log4net.Config;
using Metrics;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Configuration;
using XQueue;
using XQueue.Models;

namespace XQueueWatcher
{
	public class Program
	{
		private static readonly TimeSpan pauseBetweenRequests = TimeSpan.FromSeconds(1);

		private static readonly ILog log = LogManager.GetLogger(typeof(Program));
		private static readonly CourseManager courseManager = WebCourseManager.Instance;

		private static readonly ServiceKeepAliver keepAliver = new ServiceKeepAliver(ApplicationConfiguration.Read<UlearnConfiguration>().GraphiteServiceName);
		
		private static readonly Dictionary<int, XQueueClient> clientsCache = new Dictionary<int, XQueueClient>();

		static void Main(string[] args)
		{
			new Program().StartXQueueWatchers(new CancellationToken());
		}

		public void StartXQueueWatchers(CancellationToken cancellationToken)
		{
			XmlConfigurator.Configure();
			StaticMetricsPipeProvider.Instance.Start();
			
			if (!int.TryParse(ConfigurationManager.AppSettings["ulearn.xqueuewatcher.keepAlive.interval"], out var keepAliveIntervalSeconds))
				keepAliveIntervalSeconds = 30;
			var keepAliveInterval = TimeSpan.FromSeconds(keepAliveIntervalSeconds);

			while (true)
			{
				var xQueueRepo = GetNewXQueueRepo();
				var dbWatchers = xQueueRepo.GetXQueueWatchers();

				var tasks = dbWatchers.Select(SafeGetAndProcessSubmissionFromXQueue);
				
				Task.WaitAll(tasks.ToArray(), cancellationToken);
				if (cancellationToken.IsCancellationRequested)
					break;
					
				Task.Delay(pauseBetweenRequests, cancellationToken).Wait(cancellationToken);
				keepAliver.Ping(keepAliveInterval);
			}
		}

		private async Task SafeGetAndProcessSubmissionFromXQueue(Database.Models.XQueueWatcher watcher)
		{
			try
			{
				await GetAndProcessSubmissionFromXQueue(watcher);
			}
			catch (Exception e)
			{
				log.Error(e);
			}
		}

		private async Task GetAndProcessSubmissionFromXQueue(Database.Models.XQueueWatcher watcher)
		{
			if (!clientsCache.TryGetValue(watcher.Id, out var client))
			{
				client = new XQueueClient(watcher.BaseUrl, watcher.UserName, watcher.Password);

				if (!await client.Login())
				{
					log.Error($"Can\'t login to xqueue {watcher.QueueName} ({watcher.BaseUrl}, user {watcher.UserName})");
					return;
				}

				clientsCache[watcher.Id] = client;
			}

			var submission = await client.GetSubmission(watcher.QueueName);
			if (submission == null)
				return;

			log.Info($"Got new submission in xqueue {watcher.QueueName}: {submission.JsonSerialize()}");

			try
			{
				await ProcessSubmissionFromXQueue(watcher, submission);
			}
			catch (Exception e)
			{
				log.Error($"Error occurred while processing submission from xqueue: {e.Message}", e);
				await client.PutResult(new XQueueResult
				{
					header = submission.header,
					Body = new XQueueResultBody
					{
						Score = 0,
						IsCorrect = false,
						Message = "Sorry, we can't check your code now. Please retry or contact us at support@ulearn.me",
					}
				});
			}
		}

		private async Task ProcessSubmissionFromXQueue(Database.Models.XQueueWatcher watcher, XQueueSubmission submission)
		{
			var xQueueRepo = GetNewXQueueRepo();
			GraderPayload graderPayload;
			try
			{
				graderPayload = submission.Body.GraderPayload.DeserializeJson<GraderPayload>();
			}
			catch (Exception e)
			{
				log.Error($"Can't parse grader payload: {e.Message}");
				log.Info($"Payload is: {submission.Body.GraderPayload}");
				return;
			}

			log.Info($"Add new xqueue submission for course {graderPayload.CourseId}, slide {graderPayload.SlideId}");
			await xQueueRepo.AddXQueueSubmission(
				watcher,
				submission.Header.JsonSerialize(),
				graderPayload.CourseId,
				graderPayload.SlideId,
				submission.Body.StudentResponse
			);
		}
		
		/* We need to create new database connection each time for disabling EF's caching */
		private static XQueueRepo GetNewXQueueRepo()
		{
			var db = new ULearnDb();
			return new XQueueRepo(db, courseManager);
		}
	}

	[DataContract]
	public class GraderPayload
	{
		[DataMember(Name = "course_id")]
		public string CourseId;

		[DataMember(Name = "slide_id")]
		public Guid SlideId;
	}
}