using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Database.Models;
using log4net;
using Microsoft.EntityFrameworkCore;

namespace Database.Repos
{
	public class FeedRepo
	{
		private readonly ILog log = LogManager.GetLogger(typeof(FeedRepo));

		private readonly UlearnDb db;
		private readonly NotificationsRepo notificationsRepo;
		private readonly VisitsRepo visitsRepo;

		public FeedRepo(UlearnDb db, NotificationsRepo notificationsRepo, VisitsRepo visitsRepo)
		{
			this.db = db ?? throw new ArgumentNullException(nameof(db));
			this.notificationsRepo = notificationsRepo ?? throw new ArgumentNullException(nameof(notificationsRepo));
			this.visitsRepo = visitsRepo ?? throw new ArgumentNullException(nameof(visitsRepo));
		}

		public async Task<DateTime?> GetFeedViewTimestampAsync(string userId, int transportId)
		{
			var updateTimestamp = await db.FeedViewTimestamps
				.Where(t => t.UserId == userId && (t.TransportId == null || t.TransportId == transportId))
				.OrderByDescending(t => t.Timestamp)
				.FirstOrDefaultAsync();
			return updateTimestamp?.Timestamp;
		}

		public async Task UpdateFeedViewTimestampAsync(string userId, int transportId, DateTime timestamp)
		{
			var currentTimestamp = await db.FeedViewTimestamps.FirstOrDefaultAsync(t => t.UserId == userId && t.TransportId == transportId);
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

			await db.SaveChangesAsync();
		}

		public async Task AddFeedNotificationTransportIfNeededAsync(string userId)
		{
			if (await notificationsRepo.FindUsersNotificationTransportAsync<FeedNotificationTransport>(userId, includeDisabled: true) != null)
				return;

			await notificationsRepo.AddNotificationTransportAsync(new FeedNotificationTransport
			{
				UserId = userId,
				IsEnabled = true,
			});
		}

		public Task<FeedNotificationTransport> GetUsersFeedNotificationTransportAsync(string userId)
		{
			return notificationsRepo.FindUsersNotificationTransportAsync<FeedNotificationTransport>(userId);
		}

		public async Task<FeedNotificationTransport> GetCommentsFeedNotificationTransportAsync()
		{
			var transport = await notificationsRepo.FindUsersNotificationTransportAsync<FeedNotificationTransport>(null);
			if (transport == null)
			{
				log.Error("Can't find common (comments) feed notification transport. You should create FeedNotificationTransport with userId = NULL");
				throw new Exception("Can't find common (comments) feed notification transport");
			}

			return transport;
		}
		
		public FeedNotificationTransport GetCommentsFeedNotificationTransport()
		{
			var transport = notificationsRepo.FindUsersNotificationTransport<FeedNotificationTransport>(null);
			if (transport == null)
			{
				log.Error("Can't find common (comments) feed notification transport. You should create FeedNotificationTransport with userId = NULL");
				throw new Exception("Can't find common (comments) feed notification transport");
			}

			return transport;
		}

		public async Task<DateTime?> GetLastDeliveryTimestampAsync(FeedNotificationTransport notificationTransport)
		{
			return await notificationsRepo.GetLastDeliveryTimestampAsync(notificationTransport);
		}

		public int GetNotificationsCount(string userId, DateTime from, params FeedNotificationTransport[] transports)
		{
			var nextSecond = from.AddSeconds(1);
			var deliveriesQueryable = GetFeedNotificationDeliveriesQueryable(userId, transports);

			var totalCount = deliveriesQueryable.Count(d => d.CreateTime >= nextSecond);
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
				.Include(d => (d.Notification as ReceivedCommentToCodeReviewNotification).Comment)
				.Include(d => (d.Notification as AbstractPackageNotification).CourseVersion);
		}
	}
}