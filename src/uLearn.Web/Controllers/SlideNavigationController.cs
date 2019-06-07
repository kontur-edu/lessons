using System;
using System.Collections.Immutable;
using System.Linq;
using System.Web.Mvc;
using Database;
using Database.DataContexts;
using Database.Extensions;
using Database.Models;
using Microsoft.AspNet.Identity;
using uLearn.Web.Models;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Quizzes;

namespace uLearn.Web.Controllers
{
	public class SlideNavigationController : Controller
	{
		private readonly CourseManager courseManager = WebCourseManager.Instance;

		private readonly UnitsRepo unitsRepo;
		private readonly UserSolutionsRepo solutionsRepo;
		private readonly VisitsRepo visitsRepo;
		private readonly UserQuizzesRepo userQuizzesRepo;
		private readonly GroupsRepo groupsRepo;
		private readonly AdditionalScoresRepo additionalScoresRepo;

		public SlideNavigationController()
		{
			var db = new ULearnDb();
			unitsRepo = new UnitsRepo(db);
			solutionsRepo = new UserSolutionsRepo(db, courseManager);
			visitsRepo = new VisitsRepo(db);
			userQuizzesRepo = new UserQuizzesRepo(db);
			groupsRepo = new GroupsRepo(db, courseManager);
			additionalScoresRepo = new AdditionalScoresRepo(db);
		}

		public ActionResult TableOfContents(string courseId, Guid? slideId = null)
		{
			var course = courseManager.GetCourse(courseId);
			var userId = User.Identity.GetUserId();
			var model = User.Identity.IsAuthenticated ? CreateTocModel(course, slideId, userId) : CreateGuestTocModel(course, slideId);
			return PartialView(model);
		}

		private TocModel CreateGuestTocModel(Course course, Guid? currentSlideId)
		{
			var visibleUnits = unitsRepo.GetVisibleUnits(course, User);
			var builder = new TocModelBuilder(
				s => Url.RouteUrl("Course.SlideById", new { courseId = course.Id, slideId = s.Url }),
				s => 0,
				s => 0,
				(u, g) => 0,
				course,
				currentSlideId)
			{
				IsInstructor = false,
				IsVisited = s => false,
				IsUnitVisible = visibleUnits.Contains,
				IsSlideHidden = s => s is QuizSlide && ((QuizSlide)s).ManualChecking
			};
			var toc = builder.CreateTocModel();
			toc.NextUnitTime = unitsRepo.GetNextUnitPublishTime(course.Id);
			return toc;
		}

		private TocModel CreateTocModel(Course course, Guid? currentSlideId, string userId)
		{
			var visibleUnits = unitsRepo.GetVisibleUnits(course, User);
			var visited = visitsRepo.GetIdOfVisitedSlides(course.Id, userId);
			var scoresForSlides = visitsRepo.GetScoresForSlides(course.Id, userId);

			var solvedSlidesIds = ControllerUtils.GetSolvedSlides(solutionsRepo, userQuizzesRepo, course, userId);
			var slidesWithUsersManualChecking = visitsRepo.GetSlidesWithUsersManualChecking(course.Id, userId).ToImmutableHashSet();
			var enabledManualCheckingForUser = groupsRepo.IsManualCheckingEnabledForUser(course, userId);
			Func<Slide, int> getSlideMaxScoreFunc = s => ControllerUtils.GetMaxScoreForUsersSlide(s, solvedSlidesIds.Contains(s.Id), slidesWithUsersManualChecking.Contains(s.Id), enabledManualCheckingForUser);

			var userGroupsIds = groupsRepo.GetUserGroupsIds(course.Id, userId);
			var enabledScoringGroupsIds = groupsRepo.GetEnabledAdditionalScoringGroups(course.Id)
				.Where(e => userGroupsIds.Contains(e.GroupId))
				.Select(e => e.ScoringGroupId)
				.ToList();
			var additionalScores = additionalScoresRepo.GetAdditionalScoresForUser(course.Id, userId);

			var builder = new TocModelBuilder(
				s => Url.RouteUrl("Course.SlideById", new { courseId = course.Id, slideId = s.Url }),
				s => scoresForSlides.ContainsKey(s.Id) ? scoresForSlides[s.Id] : 0,
				getSlideMaxScoreFunc,
				(u, g) => additionalScores.GetOrDefault(Tuple.Create(u.Id, g.Id), 0),
				course,
				currentSlideId)
			{
				GetUnitInstructionNotesUrl = unit => Url.Action("InstructorNote", "Course", new { courseId = course.Id, unitId = unit.Id }),
				GetUnitStatisticsUrl = unit => Url.Action("UnitStatistics", "Analytics", new { courseId = course.Id, unitId = unit.Id }),
				IsInstructor = User.HasAccessFor(course.Id, CourseRole.Instructor),
				IsSolved = s => solvedSlidesIds.Contains(s.Id),
				IsVisited = s => visited.Contains(s.Id),
				IsUnitVisible = visibleUnits.Contains,
				IsSlideHidden = s => s is QuizSlide && ((QuizSlide)s).ManualChecking &&
									!enabledManualCheckingForUser && !solvedSlidesIds.Contains(s.Id),
				EnabledScoringGroupsIds = enabledScoringGroupsIds,
			};

			var userGroups = groupsRepo.GetUserGroups(course.Id, User.Identity.GetUserId());
			var tocGroupsForStatistics = userGroups.Where(g => g.CanUsersSeeGroupProgress).Select(g => new TocGroupForStatistics
			{
				GroupName = g.Name,
				StatisticsUrl = Url.Action("CourseStatistics", "Analytics", new { courseId = course.Id, group = g.Id })
			});
			var toc = builder.CreateTocModel(tocGroupsForStatistics.ToList());
			toc.NextUnitTime = unitsRepo.GetNextUnitPublishTime(course.Id);
			return toc;
		}

		public ActionResult PrevNextButtons(string courseId, Guid slideId, bool onSolutionsSlide)
		{
			var course = courseManager.GetCourse(courseId);
			var slide = course.GetSlideById(slideId);
			var userId = User.Identity.GetUserId();
			var nextIsAcceptedSolutions = !onSolutionsSlide && slide is ExerciseSlide && visitsRepo.IsSkippedOrPassed(courseId, slide.Id, userId) && !((ExerciseSlide)slide).Exercise.HideShowSolutionsButton;
			var visibleUnits = unitsRepo.GetVisibleUnits(course, User);
			var nextSlide = course.Slides.FirstOrDefault(s => s.Index > slide.Index && visibleUnits.Contains(s.Info.Unit));
			var prevSlide = course.Slides.LastOrDefault(s => s.Index < slide.Index && visibleUnits.Contains(s.Info.Unit));

			var model = new PrevNextButtonsModel(
				course,
				slide.Id,
				nextIsAcceptedSolutions,
				(slide as ExerciseSlide)?.Exercise.HideShowSolutionsButton ?? false,
				nextSlide,
				prevSlide,
				!User.Identity.IsAuthenticated);
			if (onSolutionsSlide)
				model.SetPrevSlide(slide);
			return PartialView(model);
		}
	}
}