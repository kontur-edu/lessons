using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Configuration;

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
		[Index("CommentBySlide")]
		public string SlideId { get; set; }

		[Required]
		public ApplicationUser Author { get; set; }

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
	}
}