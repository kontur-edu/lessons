using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class ExerciseCodeReviewComment
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		
		[Index("IDX_ExerciseCodeReviewComment_ByReviewAndIsDeleted", 1)]
		public int ReviewId { get; set;  }
		
		public virtual ExerciseCodeReview Review { get; set; }
		
		[Required]
		public string Text { get; set; }

		[Required]
		[StringLength(64)]
		public string AuthorId { get; set; }

		public virtual ApplicationUser Author { get; set; }

		[Required]
		[Index("IDX_ExerciseCodeReviewComment_ByReviewAndIsDeleted", 2)]
		public bool IsDeleted { get; set; }
		
		[Required]
		[Index("IDX_ExerciseCodeReview_ByAddingTime")]
		public DateTime AddingTime { get; set; }
	}
}