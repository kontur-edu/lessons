using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Database.Models
{
	public class ExerciseCodeReview
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public int? ExerciseCheckingId { get; set; }

		public virtual ManualExerciseChecking ExerciseChecking { get; set; }
		
		/* This field is used only for reviews not attached to specific ManualExerciseChecking */
		public int? SubmissionId { get; set; }
		
		public virtual UserExerciseSubmission Submission { get; set; }

		[Required]
		public int StartLine { get; set; }

		[Required]
		public int StartPosition { get; set; }

		[Required]
		public int FinishLine { get; set; }

		[Required]
		public int FinishPosition { get; set; }

		[Required]
		public string Comment { get; set; }

		[Required]
		[StringLength(64)]
		public string AuthorId { get; set; }

		public virtual ApplicationUser Author { get; set; }

		[Required]
		public bool IsDeleted { get; set; }

		[Required]
		public bool HiddenFromTopComments { get; set; }
		
		public DateTime AddingTime { get; set; }
		
		public virtual IList<ExerciseCodeReviewComment> Comments { get; set; }
		
		[NotMapped]
		public List<ExerciseCodeReviewComment> NotDeletedComments => Comments.Where(r => !r.IsDeleted).ToList();
		
		[NotMapped]
		public static DateTime NullAddingTime = new DateTime(1900, 1, 1);

		[NotMapped]
		public bool HasAddingTime => AddingTime.Year >= 2000;		
	}
}