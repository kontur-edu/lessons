using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos;

namespace Ulearn.Web.Api.Controllers.Notifications
{
	public class NotificationDataPreloader
	{
		private readonly CommentsRepo commentsRepo;

		public NotificationDataPreloader(CommentsRepo commentsRepo)
		{
			this.commentsRepo = commentsRepo;
		}

		public async Task<NotificationDataStorage> LoadAsync(List<Notification> notifications)
		{
			return new NotificationDataStorage
			{
				CommentsByIds = await LoadCommentsAsync(notifications)
			};
		}
		
		private async Task<Dictionary<int, Comment>> LoadCommentsAsync(IEnumerable<Notification> notifications)
		{
			/* Preload comments */
			var commentNotifications = notifications.OfType<AbstractCommentNotification>();
			var commentIds = commentNotifications.Select(n => n.CommentId);
			return (await commentsRepo.GetCommentsByIdsAsync(commentIds)).ToDictionary(c => c.Id);
		}
	}
}