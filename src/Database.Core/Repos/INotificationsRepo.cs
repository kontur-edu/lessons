using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;
using Ulearn.Common;

namespace Database.Repos
{
	public interface INotificationsRepo
	{
		Task AddNotificationTransport(NotificationTransport transport);
		Task<NotificationTransport> FindNotificationTransport(int transportId);
		Task EnableNotificationTransport(int transportId, bool isEnabled = true);
		Task<List<NotificationTransport>> GetUsersNotificationTransports(string userId, bool includeDisabled = false);
		[ItemCanBeNull]
		Task<T> FindUsersNotificationTransport<T>(string userId, bool includeDisabled = false) where T : NotificationTransport;
		Task SetNotificationTransportSettings(string courseId, int transportId, NotificationType type, bool isEnabled);
		Task<DefaultDictionary<int, NotificationTransportSettings>> GetNotificationTransportsSettings(string courseId, NotificationType type, List<int> transportIds);
		Task<DefaultDictionary<Tuple<int, NotificationType>, NotificationTransportSettings>> GetNotificationTransportsSettings(string courseId, List<int> transportIds);
		Task<DefaultDictionary<Tuple<int, NotificationType>, NotificationTransportSettings>> GetNotificationTransportsSettings(string courseId);
		Task AddNotification(string courseId, Notification notification, string initiatedUserId);
		Task<List<NotificationDelivery>> GetDeliveriesForSendingNow();
		Task MarkDeliveriesAsSent(List<int> deliveriesIds);
		Task MarkDeliveryAsRead(int deliveryId);
		Task MarkDeliveryAsWontSend(int deliveryId);
		Task MarkDeliveryAsSent(int deliveryId);
		Task CreateDeliveries();
		Task MarkDeliveriesAsFailed(IEnumerable<NotificationDelivery> deliveries);
		Task MarkDeliveryAsFailed(NotificationDelivery delivery);
		string GetSecretHashForTelegramTransport(long chatId, string chatTitle, string key);
		Task<List<NotificationType>> GetNotificationTypes(string userId, string courseId);
		Task<List<T>> FindNotifications<T>(Expression<Func<T, bool>> func, Expression<Func<T, object>> includePath = null) where T : Notification;
		IQueryable<NotificationDelivery> GetTransportDeliveriesQueryable(NotificationTransport notificationTransport, DateTime from);
		IQueryable<NotificationDelivery> GetTransportsDeliveriesQueryable(IList<int> notificationTransportsIds, DateTime from);
		Task<DateTime?> GetLastDeliveryTimestampAsync(int notificationTransportId);
	}
}