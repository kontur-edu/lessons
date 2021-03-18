using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;

namespace Database.Repos
{
	public interface IFeedRepo
	{
		Task<DateTime?> GetFeedViewTimestamp(string userId, int transportId);
		Task UpdateFeedViewTimestamp(string userId, int transportId, DateTime timestamp);
		Task AddFeedNotificationTransportIfNeeded(string userId);
		[ItemCanBeNull]
		Task<FeedNotificationTransport> GetUsersFeedNotificationTransport(string userId);
		Task<int?> GetUsersFeedNotificationTransportId(string userId);
		Task<int?> GetCommentsFeedNotificationTransportId();
		Task<DateTime?> GetLastDeliveryTimestamp(int notificationTransportId);
		Task<int> GetNotificationsCount(string userId, DateTime from, params int[] transports);
		Task<List<Notification>> GetNotificationForFeedNotificationDeliveries<TProperty>(string userId, Expression<Func<Notification, TProperty>> includePath, params int[] transportsIds);
	}
}