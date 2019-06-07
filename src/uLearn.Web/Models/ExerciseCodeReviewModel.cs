using Database.Models;

namespace uLearn.Web.Models
{
	public class ExerciseCodeReviewModel
	{
		public ExerciseCodeReview Review { get; set; }
		public ManualExerciseChecking ManualChecking { get; set; }
		public ApplicationUser CurrentUser { get; set; }
		public bool CanReply { get; set; }
		public bool ShowOnlyAutomaticalReviews { get; set; }
	}
}