using System;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;

namespace Database.Core.Tests
{
	[TestFixture]
	public class NotificationsTests
	{
		private ServiceProvider serviceProvider;

		[SetUp]
		public void SetUp()
		{
			var log = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.WriteTo.NUnitOutput()
				.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.ffff } {Level}] {Message:lj}{NewLine}{Exception}")
				.CreateLogger();
			
			/* Create database context with logging */
			// var db = new UlearnDbFactory().CreateDbContext(new string[0], new LoggerFactory(new List<ILoggerProvider> { new SerilogLoggerProvider(log) }));
			var db = new UlearnDbFactory().CreateDbContext(new string[0]);
			
			/* Disable changetracker if needed */
			// db.ChangeTracker.AutoDetectChangesEnabled = false;
						
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddSingleton(db);
			serviceCollection.AddSingleton<ILogger>(log);			
			serviceCollection.AddTransient<NotificationsRepo>();
			serviceCollection.AddTransient<FeedRepo>();
			serviceCollection.AddTransient<VisitsRepo>();

			serviceProvider = serviceCollection.BuildServiceProvider();
		}
		
		[TestCase("b189e18e-1866-47ec-8091-5a280a9c7945")]
		[Explicit]
		public async Task SlowNotificationsActualityChecking(string userId)
		{
			var feedRepo = serviceProvider.GetService<FeedRepo>();
			
			var transport = await feedRepo.GetUsersFeedNotificationTransportAsync(userId).ConfigureAwait(false);
			Console.WriteLine($@"Feed notification transport: {transport}");
			
			var deliveries = await feedRepo.GetFeedNotificationDeliveriesAsync(userId, d => d.Notification, transport).ConfigureAwait(false);
			var notifications = deliveries.Select(d => d.Notification).ToList();
			
			foreach (var notification in notifications)
			{
				Console.WriteLine($@"Checking actuality of notification {notification.GetNotificationType().ToString()} #{notification.Id}");
				var start = DateTime.Now;				
				var isActual = notification.IsActual();
				var finish = DateTime.Now;
				Console.WriteLine($@"  Elapsed: {(finish - start).TotalMilliseconds} ms, isActual: {isActual}");
			}
		}
	}
}