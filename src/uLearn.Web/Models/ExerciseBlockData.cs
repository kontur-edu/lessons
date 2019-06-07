using System.Collections.Generic;
using System.Web.Mvc;
using Database.Models;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;

namespace uLearn.Web.Models
{
	public class ExerciseBlockData
	{
		public ExerciseBlockData(string courseId, ExerciseSlide slide, bool isSkipped = true, string solution = null)
		{
			CourseId = courseId;
			Slide = slide;
			Solution = solution;
			CanSkip = !isSkipped && Solution == null;
			IsGuest = true;
			ReviewState = ExerciseReviewState.NotReviewed;
			Submissions = new List<UserExerciseSubmission>();
			TopUserReviewComments = new List<string>();
		}

		public string CourseId { get; set; }
		public ExerciseSlide Slide { get; set; }
		public AbstractExerciseBlock Block => Slide.Exercise;
		
		public ApplicationUser CurrentUser { get; set; }

		public bool IsLti { get; set; }
		public bool IsGuest { get; set; }
		public bool DebugView { get; set; }
		public bool IsSkippedOrPassed { get; set; }
		public bool ShowOutputImmediately { get; set; }
		public bool InstructorView { get; set; }
		public bool ShowOnlyAccepted { get; set; }

		public bool ShowControls => !IsGuest;
		public bool CanSkip { get; private set; }
		public string Solution { get; private set; }

		public UrlHelper Url { get; set; }
		private string runSolutionUrl;

		public string RunSolutionUrl
		{
			get
			{
				if (!string.IsNullOrEmpty(runSolutionUrl) || Url == null)
					return runSolutionUrl ?? "";
				return Url.Action("RunSolution", "Exercise", new { courseId = CourseId, slideId = Slide.Id, IsLti });
			}
			set { runSolutionUrl = value; }
		}

		public ExerciseReviewState ReviewState { get; set; }
		public List<ExerciseCodeReview> Reviews { get; set; }
		public UserExerciseSubmission SubmissionSelectedByUser { get; set; }
		public List<UserExerciseSubmission> Submissions { get; set; }
		public ManualExerciseChecking ManualChecking { get; set; }
		public List<string> TopUserReviewComments { get; set; }
		public List<string> TopOtherUsersReviewComments { get; set; }
	}

	public enum ExerciseReviewState
	{
		NotReviewed,
		WaitingForReview,
		Reviewed
	}
}