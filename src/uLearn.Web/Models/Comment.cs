using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		public string AuthorId { get; set; }

		public virtual ApplicationUser Author { get; set; }

		[Required]
		public DateTime PublishTime { get; set; }

		[Required]
		public string Text { get; set; }

		[Required]
		public bool IsVisibleForEveryone { get; set; }

		[Required]
		[DefaultValue(false)]
		public bool IsDeleted { get; set; }

		[Required]
		public bool IsPinnedToTop { get; set; }

		/* For top-level comments ParentCommentId = -1 */
		[Required]
		public int ParentCommentId { get; set; }

		public virtual ICollection<CommentLike> Likes { get; set; }

		public bool IsTopLevel()
		{
			return ParentCommentId == -1;
		}
	}
}