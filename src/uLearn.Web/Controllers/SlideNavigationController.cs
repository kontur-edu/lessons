using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using uLearn.Web.DataContexts;
using uLearn.Web.Models;

namespace uLearn.Web.Controllers
{
	public class SlideNavigationController : Controller
	{
		private readonly CourseManager courseManager = WebCourseManager.Instance;
		private readonly UnitsRepo unitsRepo = new UnitsRepo();
		private readonly UserSolutionsRepo solutionsRepo = new UserSolutionsRepo();
		private readonly VisitsRepo visitsRepo = new VisitsRepo();
		private readonly UserQuizzesRepo userQuizzesRepo = new UserQuizzesRepo();
		
		public ActionResult TableOfContents(string courseId, int slideIndex = -1)
		{
			var course = courseManager.GetCourse(courseId);
			var userId = User.Identity.GetUserId();
			var model = User.Identity.IsAuthenticated ? CreateTocModel(course, slideIndex, userId) : CreateGuestTocModel(course, slideIndex);
			return PartialView(model);
		}

		private TocModel CreateGuestTocModel(Course course, int slideIndex)
		{
			var visibleUnits = unitsRepo.GetVisibleUnits(course.Id, User);
			var builder = new TocModelBuilder(
				s => Url.Action("Slide", "Course", new { courseId = course.Id, slideIndex = s.Index }),
				s => 0,
				course,
				slideIndex);
			builder.IsInstructor = false;
			builder.IsVisited = s => false;
			builder.IsUnitVisible = visibleUnits.Contains;
			var toc = builder.CreateTocModel();
			toc.NextUnitTime = unitsRepo.GetNextUnitPublishTime(course.Id);
			return toc;
		}

		private HashSet<string> GetSolvedSlides(Course course, string userId)
		{
			var solvedSlides = solutionsRepo.GetIdOfPassedSlides(course.Id, userId);
			solvedSlides.UnionWith(userQuizzesRepo.GetIdOfQuizPassedSlides(course.Id, userId));
			return solvedSlides;
		}

		private TocModel CreateTocModel(Course course, int slideIndex, string userId)
		{
			var visibleUnits = unitsRepo.GetVisibleUnits(course.Id, User);
			var solved = GetSolvedSlides(course, userId);
			var visited = visitsRepo.GetIdOfVisitedSlides(course.Id, userId);
			var scoresForSlides = visitsRepo.GetScoresForSlides(course.Id, userId);
			var builder = new TocModelBuilder(
				s => Url.Action("Slide", "Course", new { courseId = course.Id, slideIndex = s.Index }),
				s => scoresForSlides.ContainsKey(s.Id) ? scoresForSlides[s.Id] : 0,
				course,
				slideIndex);
			builder.GetUnitInstructionNotesUrl = unitName => Url.Action("InstructorNote", "Course", new { courseId = course.Id, unitName });
			builder.GetUnitStatisticsUrl = unitName => Url.Action("UnitStatistics", "Analytics", new { courseId = course.Id, unitName });
			builder.IsInstructor = User.HasAccessFor(course.Id, CourseRoles.Instructor);
			builder.IsSolved = s => solved.Contains(s.Id);
			builder.IsVisited = s => visited.Contains(s.Id);
			builder.IsUnitVisible = visibleUnits.Contains;
			var toc = builder.CreateTocModel();
			toc.NextUnitTime = unitsRepo.GetNextUnitPublishTime(course.Id);
			return toc;
		}

		public ActionResult PrevNextButtons(string courseId, int slideIndex, bool onSolutionsSlide)
		{
			var course = courseManager.GetCourse(courseId);
			var slide = course.Slides[slideIndex];
			var userId = User.Identity.GetUserId();
			var nextIsAcceptedSolutions = !onSolutionsSlide && slide is ExerciseSlide && visitsRepo.IsSkippedOrPassed(slide.Id, userId);
			var visibleUnits = unitsRepo.GetVisibleUnits(courseId, User);
			var nextSlide = course.Slides.FirstOrDefault(s => s.Index > slideIndex && visibleUnits.Contains(s.Info.UnitName));
			var prevSlide = course.Slides.LastOrDefault(s => s.Index < slideIndex && visibleUnits.Contains(s.Info.UnitName));
			
			var model = new PrevNextButtonsModel(
				course, 
				slideIndex, 
				nextIsAcceptedSolutions, 
				nextSlide == null ? -1 : nextSlide.Index, 
				prevSlide == null ? -1 : prevSlide.Index, 
				!User.Identity.IsAuthenticated);
			if (onSolutionsSlide) model.PrevSlideIndex = model.SlideIndex;
			return PartialView(model);
		}
	}
}