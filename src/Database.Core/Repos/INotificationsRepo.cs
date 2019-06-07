using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Threading.Tasks;
using Database.Models;
using Ulearn.Common;

namespace Database.Repos
{
	public interface INotificationsRepo
	{
		Task AddNotificationTransportAsync(NotificationTransport transport);
		Task<NotificationTransport> FindNotificationTransportAsync(int transportId);
		Task EnableNotificationTransport(int transportId, bool isEnabled = true);
		Task<List<NotificationTransport>> GetUsersNotificationTransportsAsync(string userId, bool includeDisabled = false);
		List<NotificationTransport> GetUsersNotificationTransports(string userId, bool includeDisabled = false);
		Task<T> FindUsersNotificationTransportAsync<T>(string userId, bool includeDisabled = false) where T : NotificationTransport;
		T FindUsersNotificationTransport<T>(string userId, bool includeDisabled = false) where T : NotificationTransport;
		Task SetNotificationTransportSettingsAsync(string courseId, int transportId, NotificationType type, bool isEnabled);
		Task<DefaultDictionary<int, NotificationTransportSettings>> GetNotificationTransportsSettingsAsync(string courseId, NotificationType type, List<int> transportIds);
		Task<DefaultDictionary<Tuple<int, NotificationType>, NotificationTransportSettings>> GetNotificationTransportsSettingsAsync(string courseId, List<int> transportIds);
		Task<DefaultDictionary<Tuple<int, NotificationType>, NotificationTransportSettings>> GetNotificationTransportsSettingsAsync(string courseId);
		Task AddNotificationAsync(string courseId, Notification notification, string initiatedUserId);
		Task<List<NotificationDelivery>> GetDeliveriesForSendingNowAsync();
		Task MarkDeliveriesAsSentAsync(List<int> deliveriesIds);
		Task MarkDeliveryAsReadAsync(int deliveryId);
		Task MarkDeliveryAsWontSendAsync(int deliveryId);
		Task MarkDeliveryAsSentAsync(int deliveryId);
		Task CreateDeliveriesAsync();
		Task MarkDeliveriesAsFailedAsync(IEnumerable<NotificationDelivery> deliveries);
		Task MarkDeliveryAsFailedAsync(NotificationDelivery delivery);
		string GetSecretHashForTelegramTransport(long chatId, string chatTitle, string key);
		List<NotificationType> GetNotificationTypes(IPrincipal user, string courseId);
		Task<List<T>> FindNotificationsAsync<T>(Expression<Func<T, bool>> func, Expression<Func<T, object>> includePath=null) where T : Notification;
		List<T> FindNotifications<T>(Expression<Func<T, bool>> func, Expression<Func<T, object>> includePath=null) where T : Notification;
		IQueryable<NotificationDelivery> GetTransportDeliveriesQueryable(NotificationTransport notificationTransport, DateTime from);
		IQueryable<NotificationDelivery> GetTransportsDeliveriesQueryable(List<int> notificationTransportsIds, DateTime from);
		Task<DateTime?> GetLastDeliveryTimestampAsync(NotificationTransport notificationTransport);
	}
}