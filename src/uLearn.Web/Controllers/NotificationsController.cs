using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using Database;
using Database.DataContexts;
using Database.Extensions;
using Database.Models;
using Microsoft.AspNet.Identity;
using uLearn.Web.Extensions;
using uLearn.Web.FilterAttributes;
using Ulearn.Core;

namespace uLearn.Web.Controllers
{
	[ULearnAuthorize]
	public class NotificationsController : Controller
	{
		private readonly NotificationsRepo notificationsRepo;
		private readonly UsersRepo usersRepo;
		private readonly CourseManager courseManager;
		private readonly VisitsRepo visitsRepo;

		private readonly ULearnUserManager userManager;

		private readonly string telegramBotName;
		private readonly string secretForHashes;

		private readonly TimeSpan notificationEnablingLinkExpiration = TimeSpan.FromDays(7);

		public NotificationsController(ULearnDb db, CourseManager courseManager)
		{
			notificationsRepo = new NotificationsRepo(db);
			usersRepo = new UsersRepo(db);
			visitsRepo = new VisitsRepo(db);
			userManager = new ULearnUserManager(db);

			this.courseManager = courseManager;
			telegramBotName = WebConfigurationManager.AppSettings["ulearn.telegram.botName"];
			secretForHashes = WebConfigurationManager.AppSettings["ulearn.secretForHashes"] ?? "";
		}

		public NotificationsController()
			: this(new ULearnDb(), WebCourseManager.Instance)
		{
		}

		[AllowAnonymous]
		public ActionResult SuggestMailTransport()
		{
			if (!User.HasAccess(CourseRole.Instructor))
				return new HttpStatusCodeResult(HttpStatusCode.OK);

			var userId = User.Identity.GetUserId();
			var transport = notificationsRepo.FindUsersNotificationTransport<MailNotificationTransport>(userId, includeDisabled: true);
			if (transport != null)
				return new HttpStatusCodeResult(HttpStatusCode.OK);

			var user = usersRepo.FindUserById(userId);
			if (user == null)
				return new HttpNotFoundResult();
			if (string.IsNullOrEmpty(user.Email) || !user.EmailConfirmed)
				return new HttpStatusCodeResult(HttpStatusCode.OK);

			var mailNotificationTransport = new MailNotificationTransport
			{
				UserId = User.Identity.GetUserId(),
				IsEnabled = false,
			};
			AddNotificationTransport(mailNotificationTransport).Wait(5000);

			var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
			var signature = GetNotificationTransportEnablingSignature(mailNotificationTransport.Id, timestamp);

			return PartialView(new SuggestMailTransportViewModel
			{
				Transport = mailNotificationTransport,
				TelegramBotName = telegramBotName,
				LinkTimestamp = timestamp,
				LinkSignature = signature,
			});
		}

		public async Task AddNotificationTransport(MailNotificationTransport transport)
		{
			await notificationsRepo.AddNotificationTransport(transport).ConfigureAwait(false);
		}

		public string GetNotificationTransportEnablingSignature(int transportId, long timestamp)
		{
			return NotificationsRepo.GetNotificationTransportEnablingSignature(transportId, timestamp, secretForHashes);
		}

		private bool ValidateNotificationTransportEnablingSignature(int transportId, long timestamp, string signature)
		{
			var currentTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
			var linkExpirationSeconds = notificationEnablingLinkExpiration.TotalSeconds;
			if (currentTimestamp < timestamp || currentTimestamp > timestamp + linkExpirationSeconds)
				return false;

			var correctSignature = GetNotificationTransportEnablingSignature(transportId, timestamp);
			return signature == correctSignature;
		}

		public async Task<ActionResult> EnableNotificationTransport(int transportId, long timestamp, string signature, bool enable = true, string next = "")
		{
			var transport = notificationsRepo.FindNotificationTransport(transportId);
			if (!User.Identity.IsAuthenticated || transport.UserId != User.Identity.GetUserId())
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

			if (!ValidateNotificationTransportEnablingSignature(transportId, timestamp, signature))
				return RedirectToAction("Manage", "Account");

			await notificationsRepo.EnableNotificationTransport(transportId, enable);

			if (next.IsLocalUrl(Request))
				return Redirect(next);

			return RedirectToAction("Manage", "Account");
		}

		public async Task<ActionResult> CreateMailTransport()
		{
			var mailTransport = new MailNotificationTransport
			{
				UserId = User.Identity.GetUserId(),
				IsEnabled = true,
				IsDeleted = false,
			};
			await notificationsRepo.AddNotificationTransport(mailTransport);

			return RedirectToAction("Manage", "Account");
		}

		public ActionResult Settings()
		{
			var user = userManager.FindByName(User.Identity.Name);

			var mailTransport = notificationsRepo.FindUsersNotificationTransport<MailNotificationTransport>(user.Id, includeDisabled: true);
			var telegramTransport = notificationsRepo.FindUsersNotificationTransport<TelegramNotificationTransport>(user.Id, includeDisabled: true);

			var courseIds = visitsRepo.GetUserCourses(user.Id).Where(c => courseManager.FindCourse(c) != null).ToList();
			var courseTitles = courseIds.ToDictionary(c => c, c => courseManager.GetCourse(c).Title);
			var notificationTypesByCourse = courseIds.ToDictionary(c => c, c => notificationsRepo.GetNotificationTypes(User, c));
			var allNotificationTypes = NotificationsRepo.GetAllNotificationTypes();

			var notificationTransportsSettings = courseIds.SelectMany(
				c => notificationsRepo.GetNotificationTransportsSettings(c, user.Id).Select(
					kvp => Tuple.Create(Tuple.Create(c, kvp.Key.Item1, kvp.Key.Item2), kvp.Value.IsEnabled)
				)
			).ToDictionary(kvp => kvp.Item1, kvp => kvp.Item2);

			var selectedTransportIdStr = Request.QueryString["transportId"] ?? "";
			int.TryParse(selectedTransportIdStr, out int selectedTransportId);

			var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
			var getEnableLinkSignature = new Func<int, string>(transportId => GetNotificationTransportEnablingSignature(transportId, timestamp));

			return PartialView(new NotificationSettingsViewModel
			{
				User = user,
				TelegramBotName = telegramBotName,

				MailTransport = mailTransport,
				TelegramTransport = telegramTransport,
				SelectedTransportId = selectedTransportId,

				CourseTitles = courseTitles,
				AllNotificationTypes = allNotificationTypes,
				NotificationTypesByCourse = notificationTypesByCourse,
				NotificationTransportsSettings = notificationTransportsSettings,

				EnableLinkTimestamp = timestamp,
				GetEnableLinkSignature = getEnableLinkSignature,
			});
		}

		[HttpGet]
		public async Task<ActionResult> SaveSettings(string courseId, int transportId, int notificationType, bool isEnabled, long timestamp, string signature)
		{
			var userId = User.Identity.GetUserId();
			var transport = notificationsRepo.FindNotificationTransport(transportId);
			if (transport == null || transport.UserId != userId)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			}

			// TODO (andgein): Add error message about link expiration
			if (!ValidateNotificationTransportEnablingSignature(transportId, timestamp, signature))
				return RedirectToAction("Manage", "Account");

			NotificationType realNotificationType;
			try
			{
				realNotificationType = (NotificationType)notificationType;
			}
			catch (Exception)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			await notificationsRepo.SetNotificationTransportSettings(courseId, transportId, realNotificationType, isEnabled);

			return RedirectToAction("Manage", "Account");
		}

		[HttpPost]
		public async Task<ActionResult> SaveSettings(string courseId, int transportId, int notificationType, bool isEnabled)
		{
			var userId = User.Identity.GetUserId();
			var transport = notificationsRepo.FindNotificationTransport(transportId);
			if (transport == null || transport.UserId != userId)
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			}

			NotificationType realNotificationType;
			try
			{
				realNotificationType = (NotificationType)notificationType;
			}
			catch (Exception)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			await notificationsRepo.SetNotificationTransportSettings(courseId, transportId, realNotificationType, isEnabled);

			return Json(new { status = "ok", notificationTransportsSettings = GetNotificationTransportsSettings(userId) });
		}

		private IEnumerable<NotificationTransportsSettingsViewModel> GetNotificationTransportsSettings(string userId)
		{
			var notificationTransportsSettings = courseManager.GetCourses().SelectMany(
				c => notificationsRepo.GetNotificationTransportsSettings(c.Id, userId).Select(
					kvp => new NotificationTransportsSettingsViewModel
					{
						courseId = c.Id,
						transportId = kvp.Key.Item1,
						notificationType = (int)kvp.Key.Item2,
						isEnabled = kvp.Value.IsEnabled
					}
				)
			);
			return notificationTransportsSettings;
		}
	}

	public class NotificationSettingsViewModel
	{
		public ApplicationUser User { get; set; }

		public string TelegramBotName { get; set; }

		public MailNotificationTransport MailTransport { get; set; }

		public TelegramNotificationTransport TelegramTransport { get; set; }

		public int SelectedTransportId { get; set; }

		public Dictionary<string, string> CourseTitles { get; set; }

		public List<NotificationType> AllNotificationTypes { get; set; }

		public Dictionary<string, List<NotificationType>> NotificationTypesByCourse { get; set; }

		// Dictionary<(courseId, notificationTrnasportId, notificationType), isEnabled>
		public Dictionary<Tuple<string, int, NotificationType>, bool> NotificationTransportsSettings { get; set; }

		public long EnableLinkTimestamp { get; set; }

		public Func<int, string> GetEnableLinkSignature { get; set; }
	}

	public class NotificationTransportsSettingsViewModel
	{
		public string courseId;
		public int transportId;
		public int notificationType;
		public bool isEnabled;
	}

	public class SuggestMailTransportViewModel
	{
		public MailNotificationTransport Transport { get; set; }
		public string TelegramBotName { get; set; }
		public long LinkTimestamp { get; set; }
		public string LinkSignature { get; set; }
	}
}