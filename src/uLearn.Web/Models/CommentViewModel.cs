namespace uLearn.Web.Models
{
	public class CommentViewModel
	{
		public Comment Comment;
		public int LikesCount;
		public bool IsLikedByUser { get; set; }
	}
}