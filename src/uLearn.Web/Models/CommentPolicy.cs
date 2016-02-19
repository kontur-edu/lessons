using System.ComponentModel.DataAnnotations;

namespace uLearn.Web.Models
{
	public enum CommentModerationPolicy
	{
		Premoderation,
		Postmoderation
	}

	public class CommentsPolicy
	{
		[Key]
		[StringLength(64)]
		public string CourseId { get; set; }

		[Required]
		public bool IsCommentsEnabled { get; set; }

		[Required]
		public CommentModerationPolicy ModerationPolicy { get; set; }

		[Required]
		public bool OnlyInstructorsCanReply { get; set; }
	}
}