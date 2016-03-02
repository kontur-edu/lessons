using System.Collections.Generic;

namespace uLearn.Web.Models
{
	public class CommentViewModel
	{
		public Comment Comment;
		public int LikesCount;
		public bool IsLikedByUser;
		public IEnumerable<CommentViewModel> Replies;
		public bool CanReply;
		public bool CanModerateComment;
		public bool IsCommentVisibleForUser;
		public bool CanEditAndDeleteComment;
		public ApplicationUser CurrentUser;
	}
}