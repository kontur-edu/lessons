using System.Collections.Generic;
using System.Security.Principal;
using Ulearn.Core;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;

namespace uLearn.Web.Models
{
	public class AcceptedSolutionsPageModel
	{
		public string CourseId;
		public string CourseTitle;
		public ExerciseSlide Slide;
		public List<AcceptedSolutionInfo> AcceptedSolutions;
		public IPrincipal User;
		public string LikeSolutionUrl;
		public bool IsLti;
		public bool IsPassed;
	}
}