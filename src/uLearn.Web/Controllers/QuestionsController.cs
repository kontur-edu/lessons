using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Database;
using Database.DataContexts;
using Database.Models;
using uLearn.Web.FilterAttributes;
using Ulearn.Core;
using Ulearn.Core.Courses.Slides;

namespace uLearn.Web.Controllers
{
	public class QuestionViewModel
	{
		public UserQuestion Question { get; set; }
		public Slide Slide { get; set; }
		public string SlideTitle => Slide != null ? Slide.Title : Question.SlideTitle;
	}

	[ULearnAuthorize]
	public class QuestionsController : Controller
	{
		private readonly CourseManager courseManager;
		private readonly ULearnDb db = new ULearnDb();

		public QuestionsController()
			: this(WebCourseManager.Instance)
		{
		}

		public QuestionsController(CourseManager courseManager)
		{
			this.courseManager = courseManager;
		}

		[ULearnAuthorize(ShouldBeSysAdmin = true)]
		public ActionResult FixQuestions()
		{
			var fixedQuestions = new List<string>();
			var removedQuestions = new List<string>();
			foreach (var userQuestion in db.UserQuestions)
			{
				var slideTitle = userQuestion.SlideTitle;
				if (userQuestion.CourseId == null)
				{
					var cs =
						from c in courseManager.GetCourses()
						from s in c.Slides
						where s.Title == slideTitle
						select new { c, s };
					var course_slide = cs.FirstOrDefault();
					if (course_slide != null)
					{
						userQuestion.CourseId = course_slide.c.Id;
						userQuestion.SlideId = course_slide.s.Id;
						fixedQuestions.Add(userQuestion.SlideTitle);
					}
					else
					{
						db.UserQuestions.Remove(userQuestion);
						removedQuestions.Add(userQuestion.SlideTitle);
					}
				}
			}
			db.SaveChanges();
			return Content("Fixed:\n" + string.Join("\n", fixedQuestions) + "\n\n" + "Removed:\n" + string.Join("\n", removedQuestions));
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		public ActionResult ItemsOfUser(string userId, string courseId = null)
		{
			IQueryable<UserQuestion> questions = db.UserQuestions.Where(q => q.UserId == userId);
			if (courseId != null)
				questions = questions.Where(q => q.CourseId == courseId);
			var result = questions.OrderByDescending(q => q.Time).Take(20).ToList()
				.Select(q => new QuestionViewModel
				{
					Question = q,
					Slide = courseManager.GetCourse(q.CourseId).FindSlideById(q.SlideId)
				})
				.Where(m => m.Slide != null)
				.ToList();
			return PartialView("Items", result);
		}
	}
}