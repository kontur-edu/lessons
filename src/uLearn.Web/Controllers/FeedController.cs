using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web.Mvc;
using Database;
using Database.DataContexts;
using Database.Models;
using log4net;
using Microsoft.AspNet.Identity;
using uLearn.Web.FilterAttributes;
using Ulearn.Core;

namespace uLearn.Web.Controllers
{
	[ULearnAuthorize]
	public class FeedController : JsonDataContractController
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(FeedController));

		private readonly ULearnDb db;
		private readonly CourseManager courseManager;

		private readonly FeedRepo feedRepo;

		private static FeedNotificationTransport commonFeedNotificationTransport;

		public FeedController()
			: this(new ULearnDb(), WebCourseManager.Instance)
		{
		}

		public FeedController(ULearnDb db, CourseManager courseManager)
		{
			this.db = db;
			this.courseManager = courseManager;
			feedRepo = new FeedRepo(db);

			if (commonFeedNotificationTransport == null)
				commonFeedNotificationTransport = feedRepo.GetCommonFeedNotificationTransport();
		}

		protected override void OnActionExecuting(ActionExecutingContext context)
		{
			base.OnActionExecuting(context);

			var userId = User.Identity.GetUserId();
			AddFeedNotificationTransportIfNeeded(userId).Wait(5000);
		}

		private async Task AddFeedNotificationTransportIfNeeded(string userId)
		{
			await feedRepo.AddFeedNotificationTransportIfNeeded(userId).ConfigureAwait(false);
		}

		public async Task<ActionResult> Index()
		{
			var userId = User.Identity.GetUserId();

			var feedNotificationsModel = await GetFeedNotificationsModel();
			await feedRepo.UpdateFeedViewTimestamp(userId, commonFeedNotificationTransport.Id, DateTime.Now);
			return View(feedNotificationsModel);
		}

		public ActionResult UnreadCount(string lastTimestamp)
		{
			var userId = User.Identity.GetUserId();
			DateTime? lastTimestampDateTime = null;
			if (DateTime.TryParse(lastTimestamp, out var dt))
				lastTimestampDateTime = dt;

			var userNotificationTransport = feedRepo.GetUsersFeedNotificationTransport(userId);
			var unreadCountAndLastTimestamp = GetUnreadNotificationsCountAndLastTimestamp(userId, userNotificationTransport, lastTimestampDateTime);

			return Json(new UnreadCountModel
			{
				Status = "ok",
				Count = unreadCountAndLastTimestamp.Item1,
				LastTimestamp = unreadCountAndLastTimestamp.Item2,
			}, JsonRequestBehavior.AllowGet);
		}

		private Tuple<int, DateTime?> GetUnreadNotificationsCountAndLastTimestamp(string userId, FeedNotificationTransport transport, DateTime? from = null)
		{
			var realFrom = from ?? feedRepo.GetFeedViewTimestamp(userId, transport.Id) ?? DateTime.MinValue;
			var unreadCount = feedRepo.GetNotificationsCount(userId, realFrom, transport);
			if (unreadCount > 0)
			{
				from = feedRepo.GetLastDeliveryTimestamp(transport);
			}

			return Tuple.Create(unreadCount, from);
		}

		public async Task<ActionResult> UpdateLastViewTimestamp(int transportId, DateTime timestamp)
		{
			await feedRepo.UpdateFeedViewTimestamp(User.Identity.GetUserId(), transportId, timestamp);
			return new HttpStatusCodeResult(HttpStatusCode.OK);
		}

		public ActionResult NotificationsTopbarPartial(bool isMobile = false)
		{
			var userId = User.Identity.GetUserId();
			var userNotificationTransport = feedRepo.GetUsersFeedNotificationTransport(userId);
			var unreadCountAndLastTimestamp = GetUnreadNotificationsCountAndLastTimestamp(userId, userNotificationTransport);
			return PartialView(new NotificationsTopbarPartialModel
			{
				UnreadCount = unreadCountAndLastTimestamp.Item1,
				LastViewTimestamp = unreadCountAndLastTimestamp.Item2,
				IsMobile = isMobile,
			});
		}

		public async Task<ActionResult> NotificationsPartial()
		{
			var feedNotificationsModel = await GetFeedNotificationsModel();
			return PartialView(feedNotificationsModel);
		}

		private async Task<FeedNotificationsModel> GetFeedNotificationsModel()
		{
			var userId = User.Identity.GetUserId();
			var notificationTransport = feedRepo.GetUsersFeedNotificationTransport(userId);

			var importantNotifications = new List<Notification>();
			var commentsNotifications = new List<Notification>();
			if (notificationTransport != null)
			{
				importantNotifications = feedRepo.GetFeedNotificationDeliveries(userId, notificationTransport).Select(d => d.Notification).ToList();
				commentsNotifications = feedRepo.GetFeedNotificationDeliveries(userId, commonFeedNotificationTransport).Select(d => d.Notification).ToList();
			}

			importantNotifications = RemoveBlockedNotifications(importantNotifications).ToList();
			commentsNotifications = RemoveBlockedNotifications(commentsNotifications, importantNotifications).ToList();

			importantNotifications = RemoveNotActualNotifications(importantNotifications).ToList();
			commentsNotifications = RemoveNotActualNotifications(commentsNotifications).ToList();

			var importantLastViewTimestamp = feedRepo.GetFeedViewTimestamp(userId, notificationTransport?.Id ?? -1);
			var commentsLastViewTimestamp = feedRepo.GetFeedViewTimestamp(userId, commonFeedNotificationTransport.Id);

			if (notificationTransport != null)
				await feedRepo.UpdateFeedViewTimestamp(userId, notificationTransport.Id, DateTime.Now);

			return new FeedNotificationsModel
			{
				ImportantNotifications = importantNotifications,
				ImportantLastViewTimestamp = importantLastViewTimestamp,
				CommentsNotifications = commentsNotifications,
				CommentsLastViewTimestamp = commentsLastViewTimestamp,
				CommentsNotificationsTransportId = commonFeedNotificationTransport.Id,
				CourseManager = courseManager,
			};
		}

		private IEnumerable<Notification> RemoveNotActualNotifications(IEnumerable<Notification> notifications)
		{
			return notifications.Where(notification => notification.IsActual());
		}

		private IEnumerable<Notification> RemoveBlockedNotifications(IReadOnlyCollection<Notification> notifications, IReadOnlyCollection<Notification> searchBlockersAlsoIn=null)
		{
			var notificationsIds = notifications.Select(n => n.Id).ToList();
			var searchBlockersAlsoInIds = searchBlockersAlsoIn?.Select(n => n.Id).ToList();
			foreach (var notification in notifications)
			{
				var blockers = notification.GetBlockerNotifications(db);
				if (blockers.Select(b => b.Id).Intersect(notificationsIds).Any())
					continue;
				if (searchBlockersAlsoInIds != null && blockers.Select(b => b.Id).Intersect(searchBlockersAlsoInIds).Any())
					continue;
				yield return notification;
			}
		}

		[DataContract]
		public class UnreadCountModel
		{
			[DataMember(Name = "status")]
			public string Status;

			[DataMember(Name = "count")]
			public int Count;

			[DataMember(Name = "last_timestamp")]
			public DateTime? LastTimestamp;
		}
	}

	public class FeedNotificationsModel
	{
		public List<Notification> ImportantNotifications { get; set; }
		public List<Notification> CommentsNotifications { get; set; }

		public DateTime? ImportantLastViewTimestamp { get; set; }
		public DateTime? CommentsLastViewTimestamp { get; set; }

		public CourseManager CourseManager { get; set; }

		public int CommentsNotificationsTransportId { get; set; }
	}

	public class NotificationsTopbarPartialModel
	{
		public int UnreadCount { get; set; }

		public DateTime? LastViewTimestamp { get; set; }

		public bool IsMobile { get; set; }
	}
}