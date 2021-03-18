using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Database;
using Database.Di;
using Database.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ulearn.Common.Api;
using Ulearn.Common.Extensions;
using Ulearn.Core.Metrics;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;
using XQueue;
using XQueue.Models;

namespace XQueueWatcher
{
	public class XQueueWatcherApplication : BaseApplication
	{
		private static readonly TimeSpan pauseBetweenRequests = TimeSpan.FromSeconds(1);
		private static readonly Dictionary<int, XQueueClient> clientsCache = new();
		private static ServiceKeepAliver keepAliver = keepAliver = new ServiceKeepAliver(configuration.GraphiteServiceName);
		private static ILog log => LogProvider.Get().ForContext(typeof(XQueueWatcherApplication));

		public override async Task InitializeAsync(IVostokHostingEnvironment environment)
		{
			await base.InitializeAsync(environment);
		}

		public override async Task RunAsync(IVostokHostingEnvironment environment)
		{
			await StartXQueueWatchers(new CancellationToken());
		}

		protected override void ConfigureServices(IServiceCollection services, IVostokHostingEnvironment hostingEnvironment)
		{
			base.ConfigureServices(services, hostingEnvironment);
			services.AddDbContext<UlearnDb>( // AddDbContextPool: DbContext Pooling does not dispose LazyLoader https://github.com/dotnet/efcore/issues/11308
				options => options
					.UseLazyLoadingProxies()
					.UseNpgsql(configuration.Database, o => o.SetPostgresVersion(13, 2))
			);
		}

		protected override void ConfigureDi(IServiceCollection services)
		{
			base.ConfigureDi(services);
			services.AddDatabaseServices();
		}

		public async Task StartXQueueWatchers(CancellationToken cancellationToken)
		{
			var keepAliveInterval = TimeSpan.FromSeconds(configuration.KeepAliveInterval ?? 30);

			while (true)
			{
				var xQueueRepo = serviceProvider.GetService<IXQueueRepo>();
				var dbWatchers = await xQueueRepo.GetXQueueWatchers();

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
				log.Error(e, "GetAndProcessSubmissionFromXQueue error");
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
				log.Error(e, $"Error occurred while processing submission from xqueue: {e.Message}");
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
			using (var scope = serviceProvider.CreateScope())
			{
				var xQueueRepo = scope.ServiceProvider.GetService<IXQueueRepo>();
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