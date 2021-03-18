using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Ulearn.Common;

namespace Database.Models
{
	public class UserExerciseSubmission : ITimedSlideAction
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(64)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }

		[Required]
		public Guid SlideId { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }

		[Required]
		[StringLength(40)]
		public string SolutionCodeHash { get; set; }

		public virtual TextBlob SolutionCode { get; set; }

		[Required]
		public int CodeHash { get; set; }

		public virtual IList<Like> Likes { get; set; }

		public int? AutomaticCheckingId { get; set; }

		public virtual AutomaticExerciseChecking AutomaticChecking { get; set; }

		public bool AutomaticCheckingIsRightAnswer { get; set; }

		public Language Language { get; set; }

		[StringLength(40)]
		public string Sandbox { get; set; }

		public virtual IList<ManualExerciseChecking> ManualCheckings { get; set; }

		[Obsolete] // YT: ULEARN-217; Используй AntiPlagiarism.Web.Database.Models.Submission.ClientSubmissionId
		public int? AntiPlagiarismSubmissionId { get; set; }

		// Здесь ревью бота. Ревью преподавателя лежат в ManualExerciseChecking
		public virtual IList<ExerciseCodeReview> Reviews { get; set; }

		[NotMapped]
		public List<ExerciseCodeReview> NotDeletedReviews => Reviews.Where(r => !r.IsDeleted).ToList();
	}
}