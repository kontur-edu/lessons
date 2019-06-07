using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models.Comments
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
		public Guid SlideId { get; set; }

		[Required]
		[StringLength(64)]
		public string AuthorId { get; set; }

		public virtual ApplicationUser Author { get; set; }

		[Required]
		public DateTime PublishTime { get; set; }

		[Required(AllowEmptyStrings = false)]
		public string Text { get; set; }

		[Required]
		public bool IsApproved { get; set; }

		[Required]
		public bool IsDeleted { get; set; }

		[Required]
		public bool IsCorrectAnswer { get; set; }

		[Required]
		public bool IsPinnedToTop { get; set; }
		
		[Required]
		public bool IsForInstructorsOnly { get; set; }

		// For top-level comments ParentCommentId = -1 
		[Required]
		public int ParentCommentId { get; set; }

		public virtual ICollection<CommentLike> Likes { get; set; }

		public bool IsTopLevel => ParentCommentId == -1;
	}
}