using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Ulearn.Common.Extensions;
using Ulearn.Web.Api.Models.Results.Notifications;

namespace Ulearn.Web.Api.Controllers.Notifications
{
	[Route("/notifications")]
	public class NotificationsController : BaseController
	{
		private readonly UlearnDb db;
		private readonly NotificationsRepo notificationsRepo;
		private readonly FeedRepo feedRepo;
		private readonly NotificationDataPreloader notificationDataPreloader;

		private static FeedNotificationTransport commentsFeedNotificationTransport;

		public NotificationsController(ILogger logger, WebCourseManager courseManager, UlearnDb db, NotificationsRepo notificationsRepo, FeedRepo feedRepo, NotificationDataPreloader notificationDataPreloader)
			: base(logger, courseManager, db)
		{
			this.db = db ?? throw new ArgumentNullException(nameof(db));
			this.notificationsRepo = notificationsRepo ?? throw new ArgumentNullException(nameof(notificationsRepo));
			this.feedRepo = feedRepo ?? throw new ArgumentNullException(nameof(feedRepo));
			this.notificationDataPreloader = notificationDataPreloader;

			if (commentsFeedNotificationTransport == null)
				commentsFeedNotificationTransport = feedRepo.GetCommentsFeedNotificationTransport();
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> NotificationList()
		{
			var userId = User.GetUserId();
			var (importantNotificationList, commentsNotificationList) = await GetNotificationListsAsync(userId);
			// await feedRepo.UpdateFeedViewTimestampAsync(userId, commentsFeedNotificationTransport.Id, DateTime.Now);

			return Json(new NotificationListResult
			{
				Important = importantNotificationList,
				Comments = commentsNotificationList,
			});
		}
		
		private async Task<(NotificationList, NotificationList)> GetNotificationListsAsync(string userId)
		{
			var notificationTransport = await feedRepo.GetUsersFeedNotificationTransportAsync(userId);

			var importantNotifications = new List<Notification>();
			if (notificationTransport != null)
			{
				importantNotifications = (await feedRepo.GetFeedNotificationDeliveriesAsync(userId, n => n.Notification.InitiatedBy, transports: notificationTransport))
					.Select(d => d.Notification)
					.ToList();
			}
			var commentsNotifications = (await feedRepo.GetFeedNotificationDeliveriesAsync(userId, n => n.Notification.InitiatedBy, transports: commentsFeedNotificationTransport))
				.Select(d => d.Notification)
				.ToList();
			
			logger.Information($"[GetNotificationList] Step 1 done: found {importantNotifications.Count} important notifications and {commentsNotifications.Count} comment notifications");

			importantNotifications = RemoveBlockedNotifications(importantNotifications).ToList();
			commentsNotifications = RemoveBlockedNotifications(commentsNotifications, importantNotifications).ToList();
			
			logger.Information($"[GetNotificationList] Step 2 done, removed blocked notifications: left {importantNotifications.Count} important notifications and {commentsNotifications.Count} comment notifications");

			importantNotifications = RemoveNotActualNotifications(importantNotifications).ToList();
			commentsNotifications = RemoveNotActualNotifications(commentsNotifications).ToList();
			
			logger.Information($"[GetNotificationList] Step 3 done, removed not actual notifications: left {importantNotifications.Count} important notifications and {commentsNotifications.Count} comment notifications");

			var importantLastViewTimestamp = await feedRepo.GetFeedViewTimestampAsync(userId, notificationTransport?.Id ?? -1);
			var commentsLastViewTimestamp = await feedRepo.GetFeedViewTimestampAsync(userId, commentsFeedNotificationTransport.Id);

			logger.Information("[GetNotificationList] Step 4, building models");

			var allNotifications = importantNotifications.Concat(commentsNotifications).ToList();
			var notificationsData = await notificationDataPreloader.LoadAsync(allNotifications);
			
			var importantNotificationList = new NotificationList
			{
				LastViewTimestamp = importantLastViewTimestamp,
				Notifications = importantNotifications.Select(notification => BuildNotificationInfo(notification, notificationsData)).ToList(),
			};
			var commentsNotificationList = new NotificationList
			{
				LastViewTimestamp = commentsLastViewTimestamp,
				Notifications = commentsNotifications.Select(notification => BuildNotificationInfo(notification, notificationsData)).ToList(),
			};
			
			return (importantNotificationList, commentsNotificationList);
		}

		private IEnumerable<Notification> RemoveBlockedNotifications(IReadOnlyCollection<Notification> notifications, IReadOnlyCollection<Notification> searchBlockersAlsoIn=null)
		{
			var allNotifications = notifications.ToList();
			if (searchBlockersAlsoIn != null)
				allNotifications = allNotifications.Concat(searchBlockersAlsoIn).ToList();
			
			foreach (var notification in notifications)
			{
				if (notification.IsBlockedByAnyNotificationFrom(db, allNotifications))
					continue;
				yield return notification;
			}
		}
		
		private IEnumerable<Notification> RemoveNotActualNotifications(IEnumerable<Notification> notifications)
		{
			return notifications.Where(notification =>
			{
				logger.Information($"Checking actuality of notification #{notification.Id}: {notification} ({notification.GetNotificationType().ToString()})");
				return notification.IsActual();
			});
		}		
		
		private NotificationInfo BuildNotificationInfo(Notification notification, NotificationDataStorage notificationsData)
		{
			return new NotificationInfo
			{
				Id = notification.Id,
				Author = BuildShortUserInfo(notification.InitiatedBy),
				Type = notification.GetNotificationType().ToString(),
				CreateTime = notification.CreateTime,
				CourseId = notification.CourseId,
				Data = BuildNotificationData(notification, notificationsData),
			};
		}

		private NotificationData BuildNotificationData(Notification notification, NotificationDataStorage notificationsData)
		{
			var data = new NotificationData();
			if (notification is AbstractCommentNotification commentNotification)
				data.Comment = BuildCommentInfo(notificationsData.CommentsByIds.GetOrDefault(commentNotification.CommentId));
			return data;
		}
	}
}