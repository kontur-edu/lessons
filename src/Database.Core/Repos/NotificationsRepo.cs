using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Database.Extensions;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace Database.Repos
{
	/* TODO (andgein): This repo is not fully migrated to .NET Core and EF Core */
	public class NotificationsRepo : INotificationsRepo
	{
		private const int maxNotificationsSendingFails = 15;
		public static TimeSpan sendNotificationsDelayAfterCreating = TimeSpan.FromMinutes(1);

		private readonly UlearnDb db;
		private readonly ILogger logger;
		private readonly IServiceProvider serviceProvider;

		public NotificationsRepo(UlearnDb db, ILogger logger, IServiceProvider serviceProvider)
		{
			this.db = db;
			this.logger = logger;
			this.serviceProvider = serviceProvider;
		}

		private static DateTime CalculateNextTryTime(DateTime createTime, int failsCount)
		{
			if (failsCount == 0)
				return createTime;
			return createTime.AddSeconds(Math.Pow(2, failsCount - 1));
		}

		private static List<NotificationType> notificationTypesCache;

		private static void BuildNotificationTypesCache()
		{
			if (notificationTypesCache != null)
				return;

			notificationTypesCache = Enum.GetValues(typeof(NotificationType)).Cast<NotificationType>().ToList();
		}

		private static List<NotificationType> GetAllNotificationTypes()
		{
			BuildNotificationTypesCache();
			return notificationTypesCache;
		}

		private async Task DeleteOldNotificationTransportsAsync<TTransport>(string userId) where TTransport : NotificationTransport
		{
			await DeleteOldNotificationTransportsAsync(typeof(TTransport), userId).ConfigureAwait(false);
		}

		private async Task DeleteOldNotificationTransportsAsync(Type transportType, string userId)
		{
			if (!typeof(NotificationTransport).IsAssignableFrom(transportType))
				throw new ArgumentException("Parameter 'transportType' is not a NotificationTransport", nameof(transportType));

			var transports = await db.NotificationTransports
				.Where(t => t.UserId == userId && !t.IsDeleted)
				.DynamicOfType(transportType)
				.ToListAsync().ConfigureAwait(false);
			foreach (var transport in transports)
				transport.IsDeleted = true;
		}

		public async Task AddNotificationTransportAsync(NotificationTransport transport)
		{
			using (var transaction = db.Database.BeginTransaction())
			{
				await DeleteOldNotificationTransportsAsync(transport.GetType(), transport.UserId).ConfigureAwait(false);
				
				transport.IsDeleted = false;
				db.NotificationTransports.Add(transport);
				
				await db.SaveChangesAsync().ConfigureAwait(false);
				transaction.Commit();
			}
		}

		public Task<NotificationTransport> FindNotificationTransportAsync(int transportId)
		{
			return db.NotificationTransports.FindAsync(transportId);
		}

		public async Task EnableNotificationTransport(int transportId, bool isEnabled = true)
		{
			var transport = await db.NotificationTransports.FindAsync(transportId).ConfigureAwait(false);
			if (transport == null)
				return;

			transport.IsEnabled = isEnabled;
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public Task<List<NotificationTransport>> GetUsersNotificationTransportsAsync(string userId, bool includeDisabled = false)
		{
			var transports = db.NotificationTransports.Where(t => t.UserId == userId && !t.IsDeleted);
			if (!includeDisabled)
				transports = transports.Where(r => r.IsEnabled);
			return transports.ToListAsync();
		}
		
		public List<NotificationTransport> GetUsersNotificationTransports(string userId, bool includeDisabled = false)
		{
			var transports = db.NotificationTransports.Where(t => t.UserId == userId && !t.IsDeleted);
			if (!includeDisabled)
				transports = transports.Where(r => r.IsEnabled);
			return transports.ToList();
		}

		public async Task<T> FindUsersNotificationTransportAsync<T>(string userId, bool includeDisabled = false) where T : NotificationTransport
		{
			return (await GetUsersNotificationTransportsAsync(userId, includeDisabled).ConfigureAwait(false)).OfType<T>().FirstOrDefault();
		}
		
		public T FindUsersNotificationTransport<T>(string userId, bool includeDisabled = false) where T : NotificationTransport
		{
			return GetUsersNotificationTransports(userId, includeDisabled).OfType<T>().FirstOrDefault();
		}

		public async Task SetNotificationTransportSettingsAsync(string courseId, int transportId, NotificationType type, bool isEnabled)
		{
			var settings = db.NotificationTransportSettings.FirstOrDefault(
				s => s.CourseId == courseId && s.NotificationTransportId == transportId && s.NotificationType == type
			);
			if (settings == null)
			{
				settings = new NotificationTransportSettings
				{
					CourseId = courseId,
					NotificationTransportId = transportId,
					NotificationType = type,
					IsEnabled = isEnabled,
				};
			}
			else
				settings.IsEnabled = isEnabled;

			
			db.AddOrUpdate(settings, s => s.Id == settings.Id);
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task<DefaultDictionary<int, NotificationTransportSettings>> GetNotificationTransportsSettingsAsync(string courseId, NotificationType type, List<int> transportIds)
		{
			var settings = await db.NotificationTransportSettings
				.Where(s => s.CourseId == courseId && s.NotificationType == type && transportIds.Contains(s.NotificationTransportId))
				.ToDictionaryAsync(s => s.NotificationTransportId, s => s)
				.ConfigureAwait(false);
			return settings.ToDefaultDictionary();
		}

		// Dictionary<(notificationTransportId, NotificationType), NotificationTransportSettings>
		public async Task<DefaultDictionary<Tuple<int, NotificationType>, NotificationTransportSettings>> GetNotificationTransportsSettingsAsync(string courseId, List<int> transportIds)
		{
			var settings = await db.NotificationTransportSettings
				.Where(s => s.CourseId == courseId && transportIds.Contains(s.NotificationTransportId))
				.ToDictionaryAsync(s => Tuple.Create(s.NotificationTransportId, s.NotificationType), s => s)
				.ConfigureAwait(false);
			return settings.ToDefaultDictionary(() => null);
		}

		// Dictionary<(notificationTransportId, NotificationType), NotificationTransportSettings>
		public async Task<DefaultDictionary<Tuple<int, NotificationType>, NotificationTransportSettings>> GetNotificationTransportsSettingsAsync(string courseId)
		{
			var settings = await db.NotificationTransportSettings
				.Where(s => s.CourseId == courseId)
				.ToDictionaryAsync(s => Tuple.Create(s.NotificationTransportId, s.NotificationType), s => s)
				.ConfigureAwait(false);
			return settings.ToDefaultDictionary(() => null);
		}

		public Task AddNotificationAsync(string courseId, Notification notification, string initiatedUserId)
		{
			notification.CreateTime = DateTime.Now;
			notification.InitiatedById = initiatedUserId;
			notification.CourseId = courseId;
			db.Notifications.Add(notification);

			return db.SaveChangesAsync();
		}

		public Task<List<NotificationDelivery>> GetDeliveriesForSendingNowAsync()
		{
			var now = DateTime.Now;
			return db.NotificationDeliveries.Where(
				d => (d.NextTryTime < now || d.NextTryTime == null) &&
					d.Status == NotificationDeliveryStatus.NotSent &&
					d.FailsCount < maxNotificationsSendingFails
			).ToListAsync();
		}

		public Task MarkDeliveriesAsSentAsync(List<int> deliveriesIds)
		{
			foreach (var d in db.NotificationDeliveries.Where(d => deliveriesIds.Contains(d.Id)))
				d.Status = NotificationDeliveryStatus.Sent;
			return db.SaveChangesAsync();
		}

		private async Task SetDeliveryStatusAsync(int deliveryId, NotificationDeliveryStatus status)
		{
			var delivery = await db.NotificationDeliveries.FindAsync(deliveryId).ConfigureAwait(false);
			if (delivery == null)
				return;

			delivery.Status = status;
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public Task MarkDeliveryAsReadAsync(int deliveryId)
		{
			return SetDeliveryStatusAsync(deliveryId, NotificationDeliveryStatus.Read);
		}

		public Task MarkDeliveryAsWontSendAsync(int deliveryId)
		{
			return SetDeliveryStatusAsync(deliveryId, NotificationDeliveryStatus.WontSend);
		}

		public Task MarkDeliveryAsSentAsync(int deliveryId)
		{
			return SetDeliveryStatusAsync(deliveryId, NotificationDeliveryStatus.Sent);
		}

		public async Task CreateDeliveriesAsync()
		{
			var minuteAgo = DateTime.Now.Subtract(sendNotificationsDelayAfterCreating);
			var notifications = await db.Notifications.Where(n => !n.AreDeliveriesCreated && n.CreateTime < minuteAgo)
				.OrderBy(n => n.Id)
				.ToListAsync()
				.ConfigureAwait(false);
			foreach (var notification in notifications)
			{
				try
				{
					await CreateDeliveriesForNotification(notification).ConfigureAwait(false);
				}
				finally
				{
					notification.AreDeliveriesCreated = true;
				}

				await db.SaveChangesAsync().ConfigureAwait(false);
			}
		}

		private async Task CreateDeliveriesForNotification(Notification notification)
		{
			var notificationType = notification.GetNotificationType();
			logger.Information($"Found new notification {notificationType} #{notification.Id}");

			if (!notification.IsActual())
			{
				logger.Information($"Notification #{notification.Id}: is not actual more");
				return;
			}

			var blockerNotifications = notification.GetBlockerNotifications(serviceProvider);
			if (blockerNotifications.Count > 0)
			{
				logger.Information(
					$"There are blocker notifications (this one will not be send if blocker notifications has been already sent to the same transport): " +
					$"{string.Join(", ", blockerNotifications.Select(n => $"{n.GetNotificationType()} #{n.Id}"))}"
				);
			}

			var recipientsIds = await notification.GetRecipientsIdsAsync(serviceProvider).ConfigureAwait(false);
			logger.Information($"Recipients list for notification {notification.Id}: {recipientsIds.Count} user(s)");

			if (recipientsIds.Count == 0)
				return;

			var transportsSettings = db.NotificationTransportSettings
				.Include(s => s.NotificationTransport)
				.Where(s => s.CourseId == notification.CourseId &&
							s.NotificationType == notificationType &&
							!s.NotificationTransport.IsDeleted &&
							recipientsIds.Contains(s.NotificationTransport.UserId)).ToList();

			var commonTransports = db.NotificationTransports.Where(t => t.UserId == null && t.IsEnabled).ToList();

			var isEnabledByDefault = notificationType.IsEnabledByDefault();

			if (isEnabledByDefault)
			{
				logger.Information($"Notification #{notification.Id}. This notification type is enabled by default, so collecting data for it");

				var recipientsTransports = db.NotificationTransports.Where(
					t => recipientsIds.Contains(t.UserId) &&
						!t.IsDeleted &&
						t.IsEnabled
				).ToList();
				var notFoundTransports = recipientsTransports.Except(transportsSettings.Select(c => c.NotificationTransport), new NotificationTransportIdComparer());

				foreach (var transport in notFoundTransports)
				{
					transportsSettings.Add(new NotificationTransportSettings
					{
						IsEnabled = true,
						NotificationTransport = transport,
						NotificationTransportId = transport.Id,
					});
				}
			}
			else if (notification.IsNotificationForEveryone)
			{
				/* Add notification to all common transports */
				/* It's used for notifications which should be sent to everyone in the course */
				logger.Information($"Notification #{notification.Id}. This notification type is sent to everyone, so add it to all common transports ({commonTransports.Count} transport(s)):");
				foreach (var commonTransport in commonTransports)
				{
					logger.Information($"Common transport {commonTransport}");
					transportsSettings.Add(new NotificationTransportSettings
					{
						IsEnabled = true,
						NotificationTransport = commonTransport,
						NotificationTransportId = commonTransport.Id,
					});
				}
			}

			var blockerNotificationsSentToTransports = new HashSet<int>(
				GetNotificationsDeliveries(blockerNotifications.Select(n => n.Id), transportsSettings.Select(s => s.NotificationTransportId))
					.Select(d => d.NotificationTransportId)
			);
			if (blockerNotificationsSentToTransports.Count > 0)
			{
				logger.Information(
					"Blocked notifications have been already sent to follow transports:" +
					$"{string.Join(", ", blockerNotificationsSentToTransports)}"
				);
			}

			var now = DateTime.Now;

			foreach (var transportSettings in transportsSettings)
			{
				logger.Information($"Notification #{notification.Id}: add delivery to {transportSettings.NotificationTransport}, isEnabled: {transportSettings.IsEnabled}");
				if (!transportSettings.IsEnabled)
					continue;

				if (!transportSettings.NotificationTransport.IsEnabled)
				{
					logger.Information($"Transport {transportSettings.NotificationTransport} is fully disabled, ignore it");
					continue;
				}

				/* Always ignore to send notification to user initiated this notification */
				if (transportSettings.NotificationTransport.UserId == notification.InitiatedById)
				{
					logger.Information($"Don't sent notification to the transport {transportSettings.NotificationTransport} because it has been initiated by this user");
					continue;
				}

				if (blockerNotificationsSentToTransports.Contains(transportSettings.NotificationTransportId))
				{
					logger.Information($"Some blocker notification already has been sent to transport {transportSettings.NotificationTransport}, ignore it");
					continue;
				}

				logger.Information($"Notification #{notification.Id}: add delivery to {transportSettings.NotificationTransport}, sending at {now}");
				db.NotificationDeliveries.Add(new NotificationDelivery
				{
					Notification = notification,
					NotificationTransportId = transportSettings.NotificationTransportId,
					CreateTime = now,
					NextTryTime = now,
					FailsCount = 0,
					Status = NotificationDeliveryStatus.NotSent,
				});
			}
		}

		private IQueryable<NotificationDelivery> GetNotificationsDeliveries(IEnumerable<int> notificationsIds, IEnumerable<int> transportsIds)
		{
			return db.NotificationDeliveries.Where(d => notificationsIds.Contains(d.NotificationId) && transportsIds.Contains(d.NotificationTransportId));
		}

		public Task MarkDeliveriesAsFailedAsync(IEnumerable<NotificationDelivery> deliveries)
		{
			foreach (var delivery in deliveries)
			{
				delivery.FailsCount++;
				delivery.NextTryTime = CalculateNextTryTime(delivery.CreateTime, delivery.FailsCount);
			}
			return db.SaveChangesAsync();
		}

		public Task MarkDeliveryAsFailedAsync(NotificationDelivery delivery)
		{
			return MarkDeliveriesAsFailedAsync(new List<NotificationDelivery> { delivery });
		}

		public string GetSecretHashForTelegramTransport(long chatId, string chatTitle, string key)
		{
			var hasher = MD5.Create();
			var bytes = Encoding.UTF8.GetBytes(key + chatId + "&" + chatTitle + key);
			var hash = hasher.ComputeHash(bytes);

			var sb = new StringBuilder();
			foreach (var b in hash)
				sb.Append(b.ToString("x2"));
			return sb.ToString();
		}

		public List<NotificationType> GetNotificationTypes(IPrincipal user, string courseId)
		{
			var notificationTypes = GetAllNotificationTypes();

			notificationTypes = notificationTypes
				.Where(t => user.HasAccessFor(courseId, t.GetMinCourseRole()) || t.GetMinCourseRole() == CourseRoleType.Student)
				.OrderByDescending(t => t.GetMinCourseRole())
				.ThenBy(t => (int)t)
				.ToList();

			if (!user.IsSystemAdministrator())
				notificationTypes = notificationTypes.Where(t => !t.IsForSysAdminsOnly()).ToList();

			return notificationTypes;
		}

		public static string GetNotificationTransportEnablingSignature(int transportId, long timestamp, string secret)
		{
			return $"{secret}transport={transportId}&timestamp={timestamp}{secret}".CalculateMd5();
		}

		public Task<List<T>> FindNotificationsAsync<T>(Expression<Func<T, bool>> func, Expression<Func<T, object>> includePath=null) where T : Notification
		{
			var query = db.Notifications.OfType<T>();
			if (includePath != null)
				query = query.Include(includePath);
			return query.Where(func).ToListAsync();
		}
		
		public List<T> FindNotifications<T>(Expression<Func<T, bool>> func, Expression<Func<T, object>> includePath=null) where T : Notification
		{
			var query = db.Notifications.OfType<T>();
			if (includePath != null)
				query = query.Include(includePath);
			return query.Where(func).ToList();
		}

		public IQueryable<NotificationDelivery> GetTransportDeliveriesQueryable(NotificationTransport notificationTransport, DateTime from)
		{
			return db.NotificationDeliveries.Where(d => d.NotificationTransportId == notificationTransport.Id && d.CreateTime > from);
		}

		public IQueryable<NotificationDelivery> GetTransportsDeliveriesQueryable(List<int> notificationTransportsIds, DateTime from)
		{
			return db.NotificationDeliveries.Include(d => d.Notification).Where(d => notificationTransportsIds.Contains(d.NotificationTransportId) && d.CreateTime > from);
		}

		public async Task<DateTime?> GetLastDeliveryTimestampAsync(NotificationTransport notificationTransport)
		{
			var lastNotification = await db.NotificationDeliveries.Where(d => d.NotificationTransportId == notificationTransport.Id)
				.OrderByDescending(d => d.CreateTime)
				.FirstOrDefaultAsync()
				.ConfigureAwait(false);
			return lastNotification?.CreateTime;
		}
	}

	class NotificationTransportIdComparer : IEqualityComparer<NotificationTransport>
	{
		public bool Equals(NotificationTransport first, NotificationTransport second)
		{
			return first.Id == second.Id;
		}

		public int GetHashCode(NotificationTransport transport)
		{
			return transport.Id.GetHashCode();
		}
	}
}