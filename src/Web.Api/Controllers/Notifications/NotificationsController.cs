using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using Ulearn.Common.Extensions;
using Ulearn.Web.Api.Models.Parameters.Notifications;
using Ulearn.Web.Api.Models.Responses.Notifications;

namespace Ulearn.Web.Api.Controllers.Notifications
{
	[Route("/notifications")]
	public class NotificationsController : BaseController
	{
		private readonly INotificationsRepo notificationsRepo;
		private readonly IFeedRepo feedRepo;
		private readonly IServiceProvider serviceProvider;
		private readonly INotificationDataPreloader notificationDataPreloader;

		private static FeedNotificationTransport commentsFeedNotificationTransport;

		public NotificationsController(ILogger logger, WebCourseManager courseManager, UlearnDb db,
			IUsersRepo usersRepo,
			INotificationsRepo notificationsRepo, IFeedRepo feedRepo,
			IServiceProvider serviceProvider,
			INotificationDataPreloader notificationDataPreloader)
			: base(logger, courseManager, db, usersRepo)
		{
			this.notificationsRepo = notificationsRepo ?? throw new ArgumentNullException(nameof(notificationsRepo));
			this.feedRepo = feedRepo ?? throw new ArgumentNullException(nameof(feedRepo));
			this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			this.notificationDataPreloader = notificationDataPreloader ?? throw new ArgumentNullException(nameof(notificationDataPreloader));

			if (commentsFeedNotificationTransport == null)
				commentsFeedNotificationTransport = feedRepo.GetCommentsFeedNotificationTransport();
		}

		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var userId = User.GetUserId();
			await feedRepo.AddFeedNotificationTransportIfNeededAsync(userId).ConfigureAwait(false);

			await next().ConfigureAwait(false);
		}

		/// <summary>
		/// Список уведомлений и комментариев
		/// </summary>
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<NotificationListResponse>> NotificationList()
		{
			var userId = User.GetUserId();
			var (importantNotificationList, commentsNotificationList) = await GetNotificationListsAsync(userId).ConfigureAwait(false);

			return new NotificationListResponse
			{
				Important = importantNotificationList,
				Comments = commentsNotificationList,
			};
		}

		/// <summary>
		/// Число непрочитанных уведомлений
		/// </summary>
		[HttpGet("count")]
		[Authorize]
		public async Task<ActionResult<NotificationsCountResponse>> NotificationsCount([FromQuery] NotificationsCountParameters parameters)
		{
			var userId = User.GetUserId();
			var userNotificationTransport = await feedRepo.GetUsersFeedNotificationTransportAsync(userId).ConfigureAwait(false);
			var unreadCountAndLastTimestamp = await GetUnreadNotificationsCountAndLastTimestampAsync(userId, userNotificationTransport, parameters.LastTimestamp).ConfigureAwait(false);

			return new NotificationsCountResponse
			{
				Count = unreadCountAndLastTimestamp.Item1,
				LastTimestamp = unreadCountAndLastTimestamp.Item2,
			};
		}
		
		private async Task<Tuple<int, DateTime?>> GetUnreadNotificationsCountAndLastTimestampAsync(string userId, FeedNotificationTransport transport, DateTime? from = null)
		{
			var realFrom = from ?? await feedRepo.GetFeedViewTimestampAsync(userId, transport.Id).ConfigureAwait(false) ?? DateTime.MinValue;
			var unreadCount = await feedRepo.GetNotificationsCountAsync(userId, realFrom, transport).ConfigureAwait(false);
			if (unreadCount > 0)
			{
				from = await feedRepo.GetLastDeliveryTimestampAsync(transport).ConfigureAwait(false);
			}

			return Tuple.Create(unreadCount, from);
		}
		
		private async Task<(NotificationList, NotificationList)> GetNotificationListsAsync(string userId)
		{
			var notificationTransport = await feedRepo.GetUsersFeedNotificationTransportAsync(userId).ConfigureAwait(false);

			var importantNotifications = new List<Notification>();
			if (notificationTransport != null)
			{
				importantNotifications = (await feedRepo.GetFeedNotificationDeliveriesAsync(userId, n => n.Notification.InitiatedBy, transports: notificationTransport).ConfigureAwait(false))
					.Select(d => d.Notification)
					.ToList();
			}
			var commentsNotifications = (await feedRepo.GetFeedNotificationDeliveriesAsync(userId, n => n.Notification.InitiatedBy, transports: commentsFeedNotificationTransport).ConfigureAwait(false))
				.Select(d => d.Notification)
				.ToList();
			
			logger.Information($"[GetNotificationList] Step 1 done: found {importantNotifications.Count} important notifications and {commentsNotifications.Count} comment notifications");

			importantNotifications = RemoveBlockedNotifications(importantNotifications).ToList();
			commentsNotifications = RemoveBlockedNotifications(commentsNotifications, importantNotifications).ToList();
			
			logger.Information($"[GetNotificationList] Step 2 done, removed blocked notifications: left {importantNotifications.Count} important notifications and {commentsNotifications.Count} comment notifications");

			importantNotifications = RemoveNotActualNotifications(importantNotifications).ToList();
			commentsNotifications = RemoveNotActualNotifications(commentsNotifications).ToList();
			
			logger.Information($"[GetNotificationList] Step 3 done, removed not actual notifications: left {importantNotifications.Count} important notifications and {commentsNotifications.Count} comment notifications");

			var importantLastViewTimestamp = await feedRepo.GetFeedViewTimestampAsync(userId, notificationTransport?.Id ?? -1).ConfigureAwait(false);
			var commentsLastViewTimestamp = await feedRepo.GetFeedViewTimestampAsync(userId, commentsFeedNotificationTransport.Id).ConfigureAwait(false);

			logger.Information("[GetNotificationList] Step 4, building models");

			var allNotifications = importantNotifications.Concat(commentsNotifications).ToList();
			var notificationsData = await notificationDataPreloader.LoadAsync(allNotifications).ConfigureAwait(false);
			
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
				if (notification.IsBlockedByAnyNotificationFrom(serviceProvider, allNotifications))
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
				data.Comment = BuildNotificationCommentInfo(notificationsData.CommentsByIds.GetOrDefault(commentNotification.CommentId));
			return data;
		}
	}
}