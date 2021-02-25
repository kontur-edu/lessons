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
		private const string Course_Slide_User = "Course_Slide_User";
		private const string Course_Slide = "Course_Slide";
		private const string Course_Slide_Time = "Course_Slide_Time";
		private const string Course_IsRightAnswer = "Course_IsRightAnswer";
		private const string Course_Slide_IsRightAnswer = "Course_Slide_IsRightAnswer";

		[Required]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(40)]
		[Index(Course_Slide_User, 3)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(100)]
		[Index(Course_Slide_User, 1)]
		[Index(Course_Slide, 1)]
		[Index(Course_Slide_Time, 1)]
		[Index(Course_IsRightAnswer, 1)]
		[Index(Course_Slide_IsRightAnswer, 1)]
		public string CourseId { get; set; }

		[Required]
		[Index(Course_Slide_User, 2)]
		[Index(Course_Slide, 2)]
		[Index(Course_Slide_Time, 2)]
		[Index(Course_Slide_IsRightAnswer, 2)]
		public Guid SlideId { get; set; }

		[Required]
		[Index(Course_Slide_Time, 3)]
		[Index("Time")]
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

		[Index(Course_IsRightAnswer, 2)]
		[Index(Course_Slide_IsRightAnswer, 3)]
		[Index("IsRightAnswer")]
		public bool AutomaticCheckingIsRightAnswer { get; set; }

		[Index("Language")]
		public Language Language { get; set; }

		[StringLength(40)]
		[Index("Sandbox")]
		public string Sandbox { get; set; } // null if csharp sandbox

		public virtual IList<ManualExerciseChecking> ManualCheckings { get; set; }

		[Obsolete] // YT: ULEARN-217; Используй AntiPlagiarism.Web.Database.Models.Submission.ClientSubmissionId
		[Index("AntiPlagiarismSubmission")]
		public int? AntiPlagiarismSubmissionId { get; set; }

		public virtual IList<ExerciseCodeReview> Reviews { get; set; }

		[NotMapped]
		public List<ExerciseCodeReview> NotDeletedReviews => Reviews.Where(r => !r.IsDeleted).ToList();

		public bool IsWebSubmission => string.Equals(CourseId, "web", StringComparison.OrdinalIgnoreCase) && SlideId == Guid.Empty;

		public List<ExerciseCodeReview> GetAllReviews()
		{
			var manualCheckingReviews = ManualCheckings.SelectMany(c => c.NotDeletedReviews);
			return manualCheckingReviews.Concat(NotDeletedReviews).ToList();
		}
	}
}