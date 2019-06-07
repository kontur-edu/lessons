using System.Collections.Generic;
using Database.Models;

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
		public bool CanViewAuthorSubmissions;
		public bool CanViewAuthorProfile;
		public bool IsCommentForInstructorsOnly;

		public ApplicationUser CurrentUser;

		public bool ShowContextInformation = false;
		public string ContextSlideTitle;
		public string ContextParentComment;
	}
}