using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Database.Models;
using Database.Models.Comments;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Vostok.Logging.Abstractions;

namespace Database.Repos
{
	public class FeedRepo : IFeedRepo
	{
		private readonly UlearnDb db;
		private readonly INotificationsRepo notificationsRepo;
		private readonly IVisitsRepo visitsRepo;
		private static ILog log => LogProvider.Get().ForContext(typeof(FeedRepo));

		public FeedRepo(UlearnDb db, INotificationsRepo notificationsRepo, IVisitsRepo visitsRepo)
		{
			this.db = db;
			this.notificationsRepo = notificationsRepo;
			this.visitsRepo = visitsRepo;
		}

		public async Task<DateTime?> GetFeedViewTimestamp(string userId, int transportId)
		{
			var updateTimestamp = await db.FeedViewTimestamps
				.Where(t => t.UserId == userId && (t.TransportId == null || t.TransportId == transportId))
				.OrderByDescending(t => t.Timestamp)
				.FirstOrDefaultAsync()
				;
			return updateTimestamp?.Timestamp;
		}

		public async Task UpdateFeedViewTimestamp(string userId, int transportId, DateTime timestamp)
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

		public async Task AddFeedNotificationTransportIfNeeded(string userId)
		{
			if (await notificationsRepo.FindUsersNotificationTransport<FeedNotificationTransport>(userId, includeDisabled: true) != null)
				return;

			log.Info($"Create feed notification transport for user {userId} because there is no actual one");

			await notificationsRepo.AddNotificationTransport(new FeedNotificationTransport
			{
				UserId = userId,
				IsEnabled = true,
			});
		}

		public async Task<FeedNotificationTransport> GetUsersFeedNotificationTransport(string userId)
		{
			return await notificationsRepo.FindUsersNotificationTransport<FeedNotificationTransport>(userId);
		}

		public async Task<int?> GetUsersFeedNotificationTransportId(string userId)
		{
			return (await GetUsersFeedNotificationTransport(userId))?.Id;
		}

		public async Task<int?> GetCommentsFeedNotificationTransportId()
		{
			var transport = await notificationsRepo.FindUsersNotificationTransport<FeedNotificationTransport>(null);
			if (transport == null)
			{
				log.Error("Can't find common (comments) feed notification transport. You should create FeedNotificationTransport with userId = NULL");
				throw new Exception("Can't find common (comments) feed notification transport");
			}

			return transport.Id;
		}

		public async Task<DateTime?> GetLastDeliveryTimestamp(int notificationTransportId)
		{
			return await notificationsRepo.GetLastDeliveryTimestampAsync(notificationTransportId);
		}

		public async Task<int> GetNotificationsCount(string userId, DateTime from, params int[] transportIds)
		{
			var nextSecond = from.AddSeconds(1);
			var userCourses = await visitsRepo.GetUserCourses(userId);
			var deliveriesQueryable = db.NotificationDeliveries
				.Select(d => new {d.NotificationTransportId, d.Notification.CourseId, d.Notification.InitiatedById, d.CreateTime})
				.Where(d => transportIds.Contains(d.NotificationTransportId))
				.Where(d => userCourses.Contains(d.CourseId))
				.Where(d => d.InitiatedById != userId)
				.Where(d => d.CreateTime >= nextSecond);

			var totalCount = await deliveriesQueryable.CountAsync();
			return totalCount;
		}

		public async Task<List<Notification>> GetNotificationForFeedNotificationDeliveries<TProperty>(string userId, Expression<Func<Notification, TProperty>> includePath, params int[] transportsIds)
		{
			var userCourses = await visitsRepo.GetUserCourses(userId);
			const int count = 99;
			var notifications = await notificationsRepo.GetTransportsDeliveriesQueryable(transportsIds, DateTime.MinValue)
				.Where(d => userCourses.Contains(d.Notification.CourseId))
				.Where(d => d.Notification.InitiatedById != userId)
				.OrderByDescending(d => d.CreateTime)
				.Select(d => d.Notification)
				.Take(count)
				.ToListAsync();

			var abstractCommentNotifications
				= await GetNotifications<AbstractCommentNotification, Comment, TProperty>(notifications, n => n.Comment, includePath);
			var deliveriesWithCourseExportedToStepikNotification
				= await GetNotifications<CourseExportedToStepikNotification, StepikExportProcess, TProperty>(notifications, n => n.Process, includePath);
			var deliveriesWithReceivedCommentToCodeReviewNotification
				= await GetNotifications<ReceivedCommentToCodeReviewNotification, ExerciseCodeReviewComment, TProperty>(notifications, n => n.Comment, includePath);
			var deliveriesWithPassedManualExerciseCheckingNotification
				= await GetNotifications<PassedManualExerciseCheckingNotification, ManualExerciseChecking, TProperty>(notifications, n => n.Checking, includePath);
			var deliveriesWithUploadedPackageNotification
				= await GetNotifications<UploadedPackageNotification, CourseVersion, TProperty>(notifications, n => n.CourseVersion, includePath);
			var deliveriesWithPublishedPackageNotification
				= await GetNotifications<PublishedPackageNotification, CourseVersion, TProperty>(notifications, n => n.CourseVersion, includePath);
			var deliveriesWithCreatedGroupNotification
				= await GetNotifications<CreatedGroupNotification, Group, TProperty>(notifications, n => n.Group, includePath);

			var notificationsWithSpecialInclude = abstractCommentNotifications.Cast<Notification>()
				.Concat(deliveriesWithCourseExportedToStepikNotification)
				.Concat(deliveriesWithReceivedCommentToCodeReviewNotification)
				.Concat(deliveriesWithPassedManualExerciseCheckingNotification)
				.Concat(deliveriesWithUploadedPackageNotification)
				.Concat(deliveriesWithPublishedPackageNotification)
				.Concat(deliveriesWithCreatedGroupNotification)
				.ToDictionary(n => n.Id, n => n);

			var otherNotificationsIds = notifications
				.Select(n => n.Id)
				.Where(id => !notificationsWithSpecialInclude.ContainsKey(id))
				.ToList();
			var otherNotifications = await GetNotifications(otherNotificationsIds, includePath);

			return notificationsWithSpecialInclude.Values
				.Concat(otherNotifications)
				.OrderByDescending(d => d.CreateTime)
				.ToList();
		}

		private async Task<List<TNotification>> GetNotifications<TNotification, TInclude, TProperty>(
			List<Notification> notifications,
			Expression<Func<TNotification, TInclude>> navigationPropertyPath,
			Expression<Func<Notification, TProperty>> includePath)
			where TNotification: Notification
		{
			var notificationIds = notifications.OfType<TNotification>().Select(n => n.Id).ToList();
			if (!notificationIds.Any())
				return new List<TNotification>();
			var notificationQuery = db.Notifications
				.Where(n => notificationIds.Contains(n.Id));
			if (includePath != null)
				notificationQuery = notificationQuery.Include(includePath);
			var tnotificationQuery = notificationQuery
				.OfType<TNotification>()
				.Include(navigationPropertyPath)
				.AsQueryable();
			return await tnotificationQuery.ToListAsync();
		}
		
		private async Task<List<Notification>> GetNotifications<TProperty>(
			List<int> notificationIds,
			Expression<Func<Notification, TProperty>> includePath)
		{
			if (!notificationIds.Any())
				return new List<Notification>();
			var query = db.Notifications
				.Where(n => notificationIds.Contains(n.Id));
			if (includePath != null)
				query = query.Include(includePath);
			return await query.ToListAsync();
		}
	}
}