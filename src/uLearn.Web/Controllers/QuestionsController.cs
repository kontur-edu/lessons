using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using uLearn.Web.DataContexts;
using uLearn.Web.FilterAttributes;
using uLearn.Web.Models;
using System.Collections.Generic;

namespace uLearn.Web.Controllers
{
	public class QuestionViewModel
	{
		public UserQuestion Question { get; set; }
		public Slide Slide { get; set; }
		public string SlideTitle { get { return Slide != null ? Slide.Title : Question.SlideTitle; } }
	}
	
	[ULearnAuthorize]
	public class QuestionsController : Controller
	{
		private readonly CourseManager courseManager;
		private readonly ULearnDb db = new ULearnDb();
		private readonly UserQuestionsRepo userQuestionsRepo = new UserQuestionsRepo();

		public QuestionsController()
			: this(WebCourseManager.Instance)
		{
		}
		
		public QuestionsController(CourseManager courseManager)
		{
			this.courseManager = courseManager;
		}

		[ULearnAuthorize(Roles = LmsRoles.SysAdmin)]
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

		[ULearnAuthorize(MinAccessLevel = CourseRoles.Instructor)]
		public ActionResult Items(string courseId, string unitName = null)
		{
			IQueryable<UserQuestion> questions = db.UserQuestions;
			if (unitName != null)
			{
				var slideIds = courseManager.GetCourse(courseId).Slides.Where(s => s.Info.UnitName == unitName).Select(s => s.Id).ToArray();
				questions = questions.Where(q => q.CourseId == courseId && slideIds.Contains(q.SlideId));
			}
			var result = questions.OrderByDescending(q => q.Time).Take(20).ToList()
				.Select(q => new QuestionViewModel
				{
					Question = q,
					Slide = courseManager.GetCourse(q.CourseId).GetSlideById(q.SlideId)
				})
				.ToList();
			return PartialView(result);
		}

		[ULearnAuthorize(MinAccessLevel = CourseRoles.Instructor)]
		public ActionResult ItemsOfUser(string userId, string courseId = null)
		{
			IQueryable<UserQuestion> questions = db.UserQuestions.Where(q => q.UserId == userId);
			if (courseId != null)
				questions = questions.Where(q => q.CourseId == courseId);
			var result = questions.OrderByDescending(q => q.Time).Take(20).ToList()
				.Select(q => new QuestionViewModel
				{
					Question = q,
					Slide = courseManager.GetCourse(q.CourseId).GetSlideById(q.SlideId)
				})
				.ToList();
			return PartialView("Items", result);
		}

		[HttpPost]
		[ValidateInput(false)]
		public async Task<string> AddQuestion(string courseId, string slideId, string question)
		{
			var user = User.Identity;
			var slide = courseManager.GetCourse(courseId).GetSlideById(slideId);
			if (!string.IsNullOrWhiteSpace(question))
				await userQuestionsRepo.AddUserQuestion(question, courseId, slide, user.GetUserId(), user.Name, DateTime.Now);
			return "Success!";
		}
	}
}