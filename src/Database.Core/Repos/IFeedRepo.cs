using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos
{
	public interface IFeedRepo
	{
		Task<DateTime?> GetFeedViewTimestampAsync(string userId, int transportId);
		Task UpdateFeedViewTimestampAsync(string userId, int transportId, DateTime timestamp);
		Task AddFeedNotificationTransportIfNeededAsync(string userId);
		Task<FeedNotificationTransport> GetUsersFeedNotificationTransportAsync(string userId);
		Task<FeedNotificationTransport> GetCommentsFeedNotificationTransportAsync();
		FeedNotificationTransport GetCommentsFeedNotificationTransport();
		Task<DateTime?> GetLastDeliveryTimestampAsync(FeedNotificationTransport notificationTransport);
		Task<int> GetNotificationsCountAsync(string userId, DateTime from, params FeedNotificationTransport[] transports);
		Task<List<NotificationDelivery>> GetFeedNotificationDeliveriesAsync<TProperty>(string userId, Expression<Func<NotificationDelivery, TProperty>> includePath, params FeedNotificationTransport[] transports);
		Task<List<NotificationDelivery>> GetFeedNotificationDeliveriesAsync(string userId, params FeedNotificationTransport[] transports);
	}
}