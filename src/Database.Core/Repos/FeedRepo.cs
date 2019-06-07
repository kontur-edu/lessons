using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Database.Repos
{
	/* TODO (andgein): This repo is not fully migrated to .NET Core and EF Core */
	public class FeedRepo : IFeedRepo
	{
		private readonly UlearnDb db;
		private readonly INotificationsRepo notificationsRepo;
		private readonly IVisitsRepo visitsRepo;
		private readonly ILogger logger;

		public FeedRepo(UlearnDb db, INotificationsRepo notificationsRepo, IVisitsRepo visitsRepo, ILogger logger)
		{
			this.db = db ?? throw new ArgumentNullException(nameof(db));
			this.notificationsRepo = notificationsRepo ?? throw new ArgumentNullException(nameof(notificationsRepo));
			this.visitsRepo = visitsRepo ?? throw new ArgumentNullException(nameof(visitsRepo));
			this.logger = logger;
		}

		public async Task<DateTime?> GetFeedViewTimestampAsync(string userId, int transportId)
		{
			var updateTimestamp = await db.FeedViewTimestamps
				.Where(t => t.UserId == userId && (t.TransportId == null || t.TransportId == transportId))
				.OrderByDescending(t => t.Timestamp)
				.FirstOrDefaultAsync()
				.ConfigureAwait(false);
			return updateTimestamp?.Timestamp;
		}

		public async Task UpdateFeedViewTimestampAsync(string userId, int transportId, DateTime timestamp)
		{
			var currentTimestamp = await db.FeedViewTimestamps.FirstOrDefaultAsync(t => t.UserId == userId && t.TransportId == transportId).ConfigureAwait(false);
			if (currentTimestamp == null)
			{
				currentTimestamp = new FeedViewTimestamp
				{
					UserId = userId,
					TransportId = transportId,
				};
				db.FeedViewTimestamps.Add(currentTimestamp);
			}
			currentTimestamp.Timestamp = timestamp;

			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task AddFeedNotificationTransportIfNeededAsync(string userId)
		{
			if (await notificationsRepo.FindUsersNotificationTransportAsync<FeedNotificationTransport>(userId, includeDisabled: true).ConfigureAwait(false) != null)
				return;
			
			logger.Information($"Create feed notification transport for user {userId} because there is no actual one");

			await notificationsRepo.AddNotificationTransportAsync(new FeedNotificationTransport
			{
				UserId = userId,
				IsEnabled = true,
			}).ConfigureAwait(false);
		}

		public Task<FeedNotificationTransport> GetUsersFeedNotificationTransportAsync(string userId)
		{
			return notificationsRepo.FindUsersNotificationTransportAsync<FeedNotificationTransport>(userId);
		}

		public async Task<FeedNotificationTransport> GetCommentsFeedNotificationTransportAsync()
		{
			var transport = await notificationsRepo.FindUsersNotificationTransportAsync<FeedNotificationTransport>(null).ConfigureAwait(false);
			if (transport == null)
			{
				logger.Error("Can't find common (comments) feed notification transport. You should create FeedNotificationTransport with userId = NULL");
				throw new Exception("Can't find common (comments) feed notification transport");
			}

			return transport;
		}
		
		public FeedNotificationTransport GetCommentsFeedNotificationTransport()
		{
			var transport = notificationsRepo.FindUsersNotificationTransport<FeedNotificationTransport>(null);
			if (transport == null)
			{
				logger.Error("Can't find common (comments) feed notification transport. You should create FeedNotificationTransport with userId = NULL");
				throw new Exception("Can't find common (comments) feed notification transport");
			}

			return transport;
		}

		public async Task<DateTime?> GetLastDeliveryTimestampAsync(FeedNotificationTransport notificationTransport)
		{
			return await notificationsRepo.GetLastDeliveryTimestampAsync(notificationTransport).ConfigureAwait(false);
		}

		public async Task<int> GetNotificationsCountAsync(string userId, DateTime from, params FeedNotificationTransport[] transports)
		{
			var nextSecond = from.AddSeconds(1);
			var deliveriesQueryable = GetFeedNotificationDeliveriesQueryable(userId, transports);

			var totalCount = await deliveriesQueryable.CountAsync(d => d.CreateTime >= nextSecond).ConfigureAwait(false);
			return totalCount;
		}

		public Task<List<NotificationDelivery>> GetFeedNotificationDeliveriesAsync<TProperty>(string userId, Expression<Func<NotificationDelivery, TProperty>> includePath, params FeedNotificationTransport[] transports)
		{
			var queryable = GetFeedNotificationDeliveriesQueryable(userId, transports);
			if (includePath != null)
				queryable = queryable.Include(includePath);
			return queryable
				.OrderByDescending(d => d.CreateTime)
				.Take(99)
				.ToListAsync();
		}
		
		public Task<List<NotificationDelivery>> GetFeedNotificationDeliveriesAsync(string userId, params FeedNotificationTransport[] transports)
		{
			return GetFeedNotificationDeliveriesAsync<object>(userId, null, transports: transports);
		}

		private IQueryable<NotificationDelivery> GetFeedNotificationDeliveriesQueryable(string userId, params FeedNotificationTransport[] transports)
		{
			var transportsIds = new List<FeedNotificationTransport>(transports).Select(t => t.Id).ToList();
			var userCourses = visitsRepo.GetUserCourses(userId);
			return notificationsRepo.GetTransportsDeliveriesQueryable(transportsIds, DateTime.MinValue)
				.Where(d => userCourses.Contains(d.Notification.CourseId))
				.Where(d => d.Notification.InitiatedById != userId)

				/* TODO (andgein): bad code. we need to make these navigation properties loading via Notification' interface */
				.Include(d => (d.Notification as AbstractCommentNotification).Comment)
				.Include(d => (d.Notification as CourseExportedToStepikNotification).Process)
				.Include(d => (d.Notification as ReceivedCommentToCodeReviewNotification).Comment)
				.Include(d => (d.Notification as PassedManualExerciseCheckingNotification).Checking)
				.Include(d => (d.Notification as UploadedPackageNotification).CourseVersion)
				.Include(d => (d.Notification as PublishedPackageNotification).CourseVersion)
				.Include(d => (d.Notification as CreatedGroupNotification).Group)
				;
		}
	}
}