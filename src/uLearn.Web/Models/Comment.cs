using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uLearn.Web.Models
{
	public class Comment
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(64)]
		public string CourseId { get; set; }

		[Required]
		[StringLength(64)]
		[Index("IDX_Comment_CommentBySlide")]
		public string SlideId { get; set; }

		[Required]
		[StringLength(64)]
		[Index("IDX_Comment_ByAuthorAndPublishTime", 1)]
		public string AuthorId { get; set; }

		public virtual ApplicationUser Author { get; set; }

		[Required]
		[Index("IDX_Comment_ByAuthorAndPublishTime", 2)]
		public DateTime PublishTime { get; set; }

		[Required(AllowEmptyStrings=false)]
		public string Text { get; set; }

		[Required]
		public bool IsApproved { get; set; }

		[Required]
		public bool IsDeleted { get; set; }

		[Required]
		public bool IsCorrectAnswer { get; set; }

		[Required]
		public bool IsPinnedToTop { get; set; }

		// For top-level comments ParentCommentId = -1 
		[Required]
		public int ParentCommentId { get; set; }

		public virtual ICollection<CommentLike> Likes { get; set; }

		public bool IsTopLevel()
		{
			return ParentCommentId == -1;
		}
	}
}