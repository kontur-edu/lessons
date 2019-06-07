using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Database;
using Database.DataContexts;
using Database.Extensions;
using Database.Models;
using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using uLearn.Web.Extensions;
using uLearn.Web.FilterAttributes;
using uLearn.Web.Helpers;
using uLearn.Web.Models;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Units;

namespace uLearn.Web.Controllers
{
	[ULearnAuthorize(MinAccessLevel = CourseRole.Student)]
	public class AnalyticsController : JsonDataContractController
	{
		private readonly ULearnDb db;
		private readonly CourseManager courseManager;

		private readonly VisitsRepo visitsRepo;
		private readonly UserSolutionsRepo userSolutionsRepo;
		private readonly GroupsRepo groupsRepo;
		private readonly UsersRepo usersRepo;
		private readonly AdditionalScoresRepo additionalScoresRepo;

		public AnalyticsController()
			: this(new ULearnDb(), WebCourseManager.Instance)
		{
		}

		public AnalyticsController(ULearnDb db, CourseManager courseManager)
		{
			this.db = db;
			this.courseManager = courseManager;

			additionalScoresRepo = new AdditionalScoresRepo(db);
			userSolutionsRepo = new UserSolutionsRepo(db, courseManager);
			groupsRepo = new GroupsRepo(db, courseManager);
			usersRepo = new UsersRepo(db);
			visitsRepo = new VisitsRepo(db);
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		public ActionResult UnitStatistics(UnitStatisticsParams param)
		{
			const int usersLimit = 200;

			var courseId = param.CourseId;
			var unitId = param.UnitId;
			var periodStart = param.PeriodStartDate;
			var periodFinish = param.PeriodFinishDate;
			var groupsIds = Request.GetMultipleValuesFromQueryString("group");

			var realPeriodFinish = periodFinish.Add(TimeSpan.FromDays(1));

			var course = courseManager.GetCourse(courseId);
			if (!unitId.HasValue)
				return View("UnitStatisticsList", new UnitStatisticPageModel
				{
					Course = course,
					Units = course.Units,
				});
			var selectedUnit = course.FindUnitById(unitId.Value);
			if (selectedUnit == null)
				return HttpNotFound();
			
			var slides = selectedUnit.Slides;
			var slidesIds = slides.Select(s => s.Id).ToList();
			var quizzes = slides.OfType<QuizSlide>();
			var exersices = slides.OfType<ExerciseSlide>();

			var groups = groupsRepo.GetAvailableForUserGroups(courseId, User);
			var filterOptions = ControllerUtils.GetFilterOptionsByGroup<VisitsFilterOptions>(groupsRepo, User, courseId, groupsIds);
			filterOptions.SlidesIds = slidesIds;
			filterOptions.PeriodStart = periodStart;
			filterOptions.PeriodFinish = realPeriodFinish;

			/* Dictionary<SlideId, List<Visit>> */
			var slidesVisits = visitsRepo.GetVisitsInPeriodForEachSlide(filterOptions);

			var usersVisitedAllSlidesBeforePeriodCount = visitsRepo.GetUsersVisitedAllSlides(filterOptions.WithPeriodStart(DateTime.MinValue).WithPeriodFinish(periodStart)).Count();
			var usersVisitedAllSlidesInPeriodCount = visitsRepo.GetUsersVisitedAllSlides(filterOptions).Count();
			var usersVisitedAllSlidesBeforePeriodFinishedCount = visitsRepo.GetUsersVisitedAllSlides(filterOptions.WithPeriodStart(DateTime.MinValue)).Count();

			var quizzesAverageScore = quizzes.ToDictionary(q => q.Id,
				q => (int)slidesVisits.GetOrDefault(q.Id, new List<Visit>())
					.Where(v => v.IsPassed)
					.Select(v => 100 * Math.Min(v.Score, q.MaxScore) / (q.MaxScore != 0 ? q.MaxScore : 1))
					.DefaultIfEmpty(-1)
					.Average()
			);

			/* Dictionary<SlideId, count (distinct by user)> */
			var exercisesSolutionsCount = userSolutionsRepo.GetAllSubmissions(courseId, slidesIds, periodStart, realPeriodFinish)
				.GroupBy(s => s.SlideId)
				.Select(g => new { g.Key, Count = g.Select(s => s.UserId).Distinct().Count() })
				.ToDictionary(g => g.Key, g => g.Count);

			var exercisesAcceptedSolutionsCount = userSolutionsRepo.GetAllAcceptedSubmissions(courseId, slidesIds, periodStart, realPeriodFinish)
				.GroupBy(s => s.SlideId)
				.Select(g => new { g.Key, Count = g.Select(s => s.UserId).Distinct().Count() })
				.ToDictionary(g => g.Key, g => g.Count);

			var usersIds = visitsRepo.GetVisitsInPeriod(filterOptions).Select(v => v.UserId).Distinct().AsEnumerable();
			/* If we filtered out users from one or several groups show them all */
			if (filterOptions.UserIds != null && !filterOptions.IsUserIdsSupplement)
				usersIds = filterOptions.UserIds;

			var visitedUsers = usersRepo.GetUsersByIds(usersIds).Select(u => new UnitStatisticUserInfo(u)).ToList();
			var isMore = visitedUsers.Count > usersLimit;

			var visitedSlidesCountByUser = visitsRepo.GetVisitsInPeriod(filterOptions)
				.GroupBy(v => v.UserId)
				.Select(g => new { g.Key, Count = g.Count() })
				.ToDictionary(g => g.Key, g => g.Count);
			var visitedSlidesCountByUserAllTime = visitsRepo.GetVisitsInPeriod(filterOptions.WithPeriodStart(DateTime.MinValue).WithPeriodFinish(DateTime.MaxValue))
				.GroupBy(v => v.UserId)
				.Select(g => new { g.Key, Count = g.Count() })
				.ToDictionary(g => g.Key, g => g.Count);

			/* Get `usersLimit` best by slides count and order them by name */
			visitedUsers = visitedUsers
				.OrderByDescending(u => visitedSlidesCountByUserAllTime.GetOrDefault(u.UserId, 0))
				.Take(usersLimit)
				.OrderBy(u => u.UserLastName)
				.ThenBy(u => u.UserVisibleName)
				.ToList();

			var visitedUsersIds = visitedUsers.Select(v => v.UserId).ToList();
			var additionalScores = additionalScoresRepo
				.GetAdditionalScoresForUsers(courseId, unitId.Value, visitedUsersIds)
				.ToDictionary(kv => kv.Key, kv => kv.Value.Score);
			var usersGroupsIds = groupsRepo.GetUsersGroupsIds(courseId, visitedUsersIds);
			var enabledAdditionalScoringGroupsForGroups = groupsRepo.GetEnabledAdditionalScoringGroups(courseId)
				.GroupBy(e => e.GroupId)
				.ToDictionary(g => g.Key, g => g.Select(e => e.ScoringGroupId).ToList());

			var model = new UnitStatisticPageModel
			{
				Course = course,
				Units = course.Units,
				Unit = selectedUnit,
				SelectedGroupsIds = groupsIds,
				Groups = groups,

				PeriodStart = periodStart,
				PeriodFinish = periodFinish,

				Slides = slides,
				SlidesVisits = slidesVisits,

				UsersVisitedAllSlidesBeforePeriodCount = usersVisitedAllSlidesBeforePeriodCount,
				UsersVisitedAllSlidesInPeriodCount = usersVisitedAllSlidesInPeriodCount,
				UsersVisitedAllSlidesBeforePeriodFinishedCount = usersVisitedAllSlidesBeforePeriodFinishedCount,

				QuizzesAverageScore = quizzesAverageScore,
				ExercisesSolutionsCount = exercisesSolutionsCount,
				ExercisesAcceptedSolutionsCount = exercisesAcceptedSolutionsCount,
				VisitedUsers = visitedUsers,
				VisitedUsersIsMore = isMore,
				VisitedSlidesCountByUser = visitedSlidesCountByUser,
				VisitedSlidesCountByUserAllTime = visitedSlidesCountByUserAllTime,

				AdditionalScores = additionalScores,
				UsersGroupsIds = usersGroupsIds,
				EnabledAdditionalScoringGroupsForGroups = enabledAdditionalScoringGroupsForGroups,
			};
			return View(model);
		}

		private bool CanStudentViewGroupsStatistics(string userId, List<string> groupsIds)
		{
			foreach (var groupId in groupsIds)
			{
				int groupIdInt;
				if (!int.TryParse(groupId, out groupIdInt))
					return false;
				var usersIds = groupsRepo.GetGroupMembersAsUsers(groupIdInt).Select(u => u.Id);
				if (!usersIds.Contains(userId))
					return false;
			}
			return true;
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		public ActionResult ExportCourseStatisticsAsJson(CourseStatisticsParams param)
		{
			if(param.CourseId == null)
				return HttpNotFound();

			var model = GetCourseStatisticsModel(param, 3000);

			var filename = model.Course.Id + ".json";
			Response.AddHeader("Content-Disposition", "attachment;filename=" + filename);

			return Json(new CourseStatisticsModel(model), JsonRequestBehavior.AllowGet);
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		public ActionResult ExportCourseStatisticsAsXml(CourseStatisticsParams param)
		{
			if(param.CourseId == null)
				return HttpNotFound();
			
			var model = GetCourseStatisticsModel(param, 3000);

			var filename = model.Course.Id + ".xml";
			Response.AddHeader("Content-Disposition", "attachment;filename=" + filename);

			return Content(new CourseStatisticsModel(model).XmlSerialize(), "text/xml");
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		public ActionResult ExportCourseStatisticsAsXlsx(CourseStatisticsParams param)
		{
			if(param.CourseId == null)
				return HttpNotFound();
			
			var model = GetCourseStatisticsModel(param, 3000);

			var package = new ExcelPackage();
			FillCourseStatisticsExcelWorksheet(
				package.Workbook.Worksheets.Add(model.Course.Title),
				model
			);
			FillCourseStatisticsExcelWorksheet(
				package.Workbook.Worksheets.Add("Только полные баллы"),
				model,
				onlyFullScores: true
			);

			var filename = model.Course.Id + ".xlsx";
			Response.AddHeader("Content-Disposition", "attachment;filename=" + filename);
			Response.Charset = "";
			Response.Cache.SetCacheability(HttpCacheability.NoCache);
			Response.ContentType = "application/vnd.ms-excel";
			package.SaveAs(Response.OutputStream);
			Response.End();
			return new EmptyResult();
		}

		private void FillCourseStatisticsExcelWorksheet(ExcelWorksheet worksheet, CourseStatisticPageModel model, bool onlyFullScores = false)
		{
			var builder = new ExcelWorksheetBuilder(worksheet);

			builder.AddStyleRule(s => s.Font.Bold = true);

			builder.AddCell("", 3);
			builder.AddCell("За весь курс", model.ScoringGroups.Count);
			builder.AddStyleRule(s => s.Border.Left.Style = ExcelBorderStyle.Thin);
			foreach (var unit in model.Course.Units)
			{
				var colspan = 0;
				foreach (var scoringGroup in model.GetUsingUnitScoringGroups(unit, model.ScoringGroups).Values)
				{
					var shouldBeSolvedSlides = model.ShouldBeSolvedSlidesByUnitScoringGroup[Tuple.Create(unit.Id, scoringGroup.Id)];
					colspan += shouldBeSolvedSlides.Count + 1;
					if (shouldBeSolvedSlides.Count > 0 && scoringGroup.CanBeSetByInstructor)
						colspan++;
				}
				builder.AddCell(unit.Title, colspan);
			}
			builder.PopStyleRule(); // Border.Left
			builder.GoToNewLine();

			builder.AddCell("Фамилия Имя");
			builder.AddCell("Эл. почта");
			builder.AddCell("Группа");
			foreach (var scoringGroup in model.ScoringGroups.Values)
				builder.AddCell(scoringGroup.Abbreviation);
			foreach (var unit in model.Course.Units)
			{
				builder.AddStyleRuleForOneCell(s => s.Border.Left.Style = ExcelBorderStyle.Thin);
				foreach (var scoringGroup in model.GetUsingUnitScoringGroups(unit, model.ScoringGroups).Values)
				{
					var shouldBeSolvedSlides = model.ShouldBeSolvedSlidesByUnitScoringGroup[Tuple.Create(unit.Id, scoringGroup.Id)];
					builder.AddCell(scoringGroup.Abbreviation);

					builder.AddStyleRule(s => s.TextRotation = 90);
					builder.AddStyleRule(s => s.Font.Bold = false);
					foreach (var slide in shouldBeSolvedSlides)
						builder.AddCell($"{scoringGroup.Abbreviation}: {slide.Title}");
					if (shouldBeSolvedSlides.Count > 0 && scoringGroup.CanBeSetByInstructor)
						builder.AddCell("Доп");
					builder.PopStyleRule();
					builder.PopStyleRule();
				}
			}
			builder.GoToNewLine();

			builder.AddStyleRule(s => s.Border.Bottom.Style = ExcelBorderStyle.Thin);
			builder.AddStyleRuleForOneCell(s =>
			{
				s.HorizontalAlignment = ExcelHorizontalAlignment.Right;
				s.Font.Size = 10;
			});
			builder.AddCell("Максимум:", 3);
			foreach (var scoringGroup in model.ScoringGroups.Values)
				builder.AddCell(model.Course.Units.Sum(unit => model.GetMaxScoreForUnitByScoringGroup(unit, scoringGroup)));
			foreach (var unit in model.Course.Units)
			{
				builder.AddStyleRuleForOneCell(s => s.Border.Left.Style = ExcelBorderStyle.Thin);
				foreach (var scoringGroup in model.GetUsingUnitScoringGroups(unit, model.ScoringGroups).Values)
				{
					var shouldBeSolvedSlides = model.ShouldBeSolvedSlidesByUnitScoringGroup[Tuple.Create(unit.Id, scoringGroup.Id)];
					builder.AddCell(model.GetMaxScoreForUnitByScoringGroup(unit, scoringGroup));
					foreach (var slide in shouldBeSolvedSlides)
						builder.AddCell(slide.MaxScore);
					if (shouldBeSolvedSlides.Count > 0 && scoringGroup.CanBeSetByInstructor)
						builder.AddCell(scoringGroup.MaxAdditionalScore);
				}
			}
			builder.PopStyleRule(); // Bottom.Border
			builder.GoToNewLine();

			builder.AddStyleRule(s => s.Font.Bold = false);

			foreach (var user in model.VisitedUsers)
			{
				builder.AddCell(user.UserVisibleName);
				builder.AddCell(user.UserEmail);
				var userGroups = model.Groups.Where(g => model.VisitedUsersGroups[user.UserId].Contains(g.Id)).Select(g => g.Name).ToList();
				builder.AddCell(string.Join(", ", userGroups));
				foreach (var scoringGroup in model.ScoringGroups.Values)
				{
					var scoringGroupScore = model.Course.Units.Sum(unit => model.GetTotalScoreForUserInUnitByScoringGroup(user.UserId, unit, scoringGroup));
					var scoringGroupOnlyFullScore = model.Course.Units.Sum(unit => model.GetTotalOnlyFullScoreForUserInUnitByScoringGroup(user.UserId, unit, scoringGroup));
					builder.AddCell(onlyFullScores ? scoringGroupOnlyFullScore : scoringGroupScore);
				}
				foreach (var unit in model.Course.Units)
				{
					builder.AddStyleRuleForOneCell(s => s.Border.Left.Style = ExcelBorderStyle.Thin);
					foreach (var scoringGroup in model.GetUsingUnitScoringGroups(unit, model.ScoringGroups).Values)
					{
						var shouldBeSolvedSlides = model.ShouldBeSolvedSlidesByUnitScoringGroup[Tuple.Create(unit.Id, scoringGroup.Id)];
						var scoringGroupScore = model.GetTotalScoreForUserInUnitByScoringGroup(user.UserId, unit, scoringGroup);
						var scoringGroupOnlyFullScore = model.GetTotalOnlyFullScoreForUserInUnitByScoringGroup(user.UserId, unit, scoringGroup);
						builder.AddCell(onlyFullScores ? scoringGroupOnlyFullScore : scoringGroupScore);
						foreach (var slide in shouldBeSolvedSlides)
						{
							var slideScore = model.ScoreByUserAndSlide[Tuple.Create(user.UserId, slide.Id)];
							builder.AddCell(onlyFullScores ? model.GetOnlyFullScore(slideScore, slide) : slideScore);
						}
						if (shouldBeSolvedSlides.Count > 0 && scoringGroup.CanBeSetByInstructor)
							builder.AddCell(model.AdditionalScores[Tuple.Create(user.UserId, unit.Id, scoringGroup.Id)]);
					}
				}
				builder.GoToNewLine();
			}

			for (var column = 1; column <= builder.ColumnsCount; column++)
				worksheet.Column(column).AutoFit(0.1);
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.Student)]
		public ActionResult CourseStatistics(CourseStatisticsParams param, int max=200)
		{
			if(param.CourseId == null)
				return HttpNotFound();
			
			var usersLimit = max;
			if (usersLimit > 300)
				usersLimit = 300;
			if (usersLimit < 0)
				usersLimit = 200;

			var model = GetCourseStatisticsModel(param, usersLimit);
			if (model == null)
				return HttpNotFound();
			return View(model);
		}

		/* TODO: extract copy-paste */
		private CourseStatisticPageModel GetCourseStatisticsModel(CourseStatisticsParams param, int usersLimit)
		{
			var courseId = param.CourseId;
			var periodStart = param.PeriodStartDate;
			var periodFinish = param.PeriodFinishDate;
			var groupsIds = Request.GetMultipleValuesFromQueryString("group");
			var isInstructor = User.HasAccessFor(courseId, CourseRole.Instructor);
			var isStudent = !isInstructor;

			var currentUserId = User.Identity.GetUserId();
			if (isStudent && !CanStudentViewGroupsStatistics(currentUserId, groupsIds))
				return null;

			var realPeriodFinish = periodFinish.Add(TimeSpan.FromDays(1));

			var course = courseManager.GetCourse(courseId);
			var slidesIds = course.Slides.Select(s => s.Id).ToList();

			var filterOptions = ControllerUtils.GetFilterOptionsByGroup<VisitsFilterOptions>(groupsRepo, User, courseId, groupsIds, allowSeeGroupForAnyMember: true);
			filterOptions.SlidesIds = slidesIds;
			filterOptions.PeriodStart = periodStart;
			filterOptions.PeriodFinish = realPeriodFinish;

			var usersIds = visitsRepo.GetVisitsInPeriod(filterOptions).Select(v => v.UserId).Distinct().ToList();
			/* If we filtered out users from one or several groups show them all */
			if (filterOptions.UserIds != null && !filterOptions.IsUserIdsSupplement)
				usersIds = filterOptions.UserIds;

			var visitedUsers = usersRepo.GetUsersByIds(usersIds).Select(u => new UnitStatisticUserInfo(u)).ToList();
			var isMore = visitedUsers.Count > usersLimit;

			var unitBySlide = course.Units.SelectMany(u => u.Slides.Select(s => Tuple.Create(u.Id, s.Id))).ToDictionary(p => p.Item2, p => p.Item1);
			var scoringGroups = course.Settings.Scoring.Groups;

			var totalScoreByUserAllTime = visitsRepo.GetVisitsInPeriod(filterOptions.WithPeriodStart(DateTime.MinValue).WithPeriodFinish(DateTime.MaxValue))
				.GroupBy(v => v.UserId)
				.Select(g => new { g.Key, Sum = g.Sum(v => v.Score)})
				.ToDictionary(g => g.Key, g => g.Sum)
				.ToDefaultDictionary();

			/* Get `usersLimit` best by slides count */
			visitedUsers = visitedUsers
				.OrderByDescending(u => totalScoreByUserAllTime[u.UserId])
				.Take(usersLimit)
				.ToList();
			var visitedUsersIds = visitedUsers.Select(v => v.UserId).ToList();

			var visitedUsersGroups = groupsRepo.GetUsersGroupsIds(new List<string> { courseId }, visitedUsersIds, User, 10).ToDefaultDictionary();

			/* From now fetch only filtered users' statistics */
			filterOptions.UserIds = visitedUsersIds;
			filterOptions.IsUserIdsSupplement = false;
			var scoreByUserUnitScoringGroup = ((IEnumerable<Visit>)visitsRepo.GetVisitsInPeriod(filterOptions))
				.GroupBy(v => Tuple.Create(v.UserId, unitBySlide[v.SlideId], course.FindSlideById(v.SlideId)?.ScoringGroup))
				.ToDictionary(g => g.Key, g => g.Sum(v => v.Score))
				.ToDefaultDictionary();

			var shouldBeSolvedSlides = course.Slides.Where(s => s.ShouldBeSolved).ToList();
			var shouldBeSolvedSlidesIds = shouldBeSolvedSlides.Select(s => s.Id).ToList();
			var shouldBeSolvedSlidesByUnitScoringGroup = shouldBeSolvedSlides
				.GroupBy(s => Tuple.Create(unitBySlide[s.Id], s.ScoringGroup))
				.ToDictionary(g => g.Key, g => g.ToList())
				.ToDefaultDictionary();
			var scoreByUserAndSlide = ((IEnumerable<Visit>)visitsRepo.GetVisitsInPeriod(filterOptions.WithSlidesIds(shouldBeSolvedSlidesIds)))
				.GroupBy(v => Tuple.Create(v.UserId, v.SlideId))
				.ToDictionary(g => g.Key, g => g.Sum(v => v.Score))
				.ToDefaultDictionary();

			var additionalScores = additionalScoresRepo
				.GetAdditionalScoresForUsers(courseId, visitedUsersIds)
				.ToDictionary(kv => kv.Key, kv => kv.Value.Score)
				.ToDefaultDictionary();
			var usersGroupsIds = groupsRepo.GetUsersGroupsIds(courseId, visitedUsersIds);
			var enabledAdditionalScoringGroupsForGroups = groupsRepo.GetEnabledAdditionalScoringGroups(courseId)
				.GroupBy(e => e.GroupId)
				.ToDictionary(g => g.Key, g => g.Select(e => e.ScoringGroupId).ToList());

			/* Filter out only scoring groups which are affected in selected groups */
			var additionalScoringGroupsForFilteredGroups = ControllerUtils.GetEnabledAdditionalScoringGroupsForGroups(groupsRepo, course, groupsIds, User);
			scoringGroups = scoringGroups
				.Where(kv => kv.Value.MaxNotAdditionalScore > 0 || additionalScoringGroupsForFilteredGroups.Contains(kv.Key))
				.ToDictionary(kv => kv.Key, kv => kv.Value)
				.ToSortedDictionary();

			var groups = groupsRepo.GetAvailableForUserGroups(courseId, User);
			if (!isInstructor)
				groups = groupsRepo.GetUserGroups(courseId, currentUserId);
			var model = new CourseStatisticPageModel
			{
				IsInstructor = isInstructor,
				Course = course,
				SelectedGroupsIds = groupsIds,
				Groups = groups,
				PeriodStart = periodStart,
				PeriodFinish = periodFinish,
				VisitedUsers = visitedUsers,
				VisitedUsersIsMore = isMore,
				VisitedUsersGroups = visitedUsersGroups,
				ShouldBeSolvedSlidesByUnitScoringGroup = shouldBeSolvedSlidesByUnitScoringGroup,
				ScoringGroups = scoringGroups,
				ScoreByUserUnitScoringGroup = scoreByUserUnitScoringGroup,
				ScoreByUserAndSlide = scoreByUserAndSlide,
				AdditionalScores = additionalScores,
				UsersGroupsIds = usersGroupsIds,
				EnabledAdditionalScoringGroupsForGroups = enabledAdditionalScoringGroupsForGroups,
			};
			return model;
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		public ActionResult UserUnitStatistics(string courseId, Guid unitId, string userId)
		{
			var course = courseManager.GetCourse(courseId);
			var user = usersRepo.FindUserById(userId);
			if (user == null)
				return HttpNotFound();

			var unit = course.FindUnitById(unitId);
			if (unit == null)
				return HttpNotFound();
			
			var slides = unit.Slides;
			var exercises = slides.OfType<ExerciseSlide>().ToList();
			var acceptedSubmissions = userSolutionsRepo
				.GetAllAcceptedSubmissionsByUser(courseId, exercises.Select(s => s.Id), userId)
				.OrderByDescending(s => s.Timestamp)
				.DistinctBy(u => u.SlideId)
				.ToList();
			var reviewedSubmissions = userSolutionsRepo
				.GetAllAcceptedSubmissionsByUser(courseId, exercises.Select(s => s.Id), userId)
				.Where(s => s.ManualCheckings.Any(c => c.IsChecked))
				.OrderByDescending(s => s.Timestamp)
				.DistinctBy(u => u.SlideId)
				.ToList();
			var userScores = visitsRepo.GetScoresForSlides(courseId, userId, slides.Select(s => s.Id));

			var unitIndex = course.Units.FindIndex(u => u.Id == unitId);
			var previousUnit = unitIndex == 0 ? null : course.Units[unitIndex - 1];
			var nextUnit = unitIndex == course.Units.Count - 1 ? null : course.Units[unitIndex + 1];

			var model = new UserUnitStatisticsPageModel
			{
				Course = course,
				Unit = unit,
				User = user,
				Slides = slides.ToDictionary(s => s.Id),
				Submissions = acceptedSubmissions,
				ReviewedSubmissions = reviewedSubmissions,
				Scores = userScores,
				PreviousUnit = previousUnit,
				NextUnit = nextUnit,
			};

			return View(model);
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		public ActionResult SlideRatings(string courseId, Guid unitId)
		{
			var course = courseManager.GetCourse(courseId);
			var unit = course.FindUnitById(unitId);
			if (unit == null)
				return HttpNotFound();
			
			var slides = unit.Slides.ToArray();
			var model = GetSlideRateStats(course, slides);
			return PartialView(model);
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		public ActionResult DailyStatistics(string courseId, Guid? unitId)
		{
			IEnumerable<Slide> slides = null;
			if (courseId != null && unitId.HasValue)
			{
				var course = courseManager.GetCourse(courseId);

				var unit = course.FindUnitById(unitId.Value);
				if (unit == null)
					return HttpNotFound();
				slides = unit.Slides.ToArray();
			}
			var model = GetDailyStatistics(slides);
			return PartialView(model);
		}

		[ULearnAuthorize(ShouldBeSysAdmin = true)]
		public ActionResult SystemStatistics()
		{
			return View();
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		public ActionResult UsersProgress(string courseId, Guid unitId, DateTime periodStart)
		{
			var course = courseManager.GetCourse(courseId);
			var unit = course.FindUnitById(unitId);
			if (unit == null)
				return HttpNotFound();
			var slides = unit.Slides.ToArray();
			var users = GetUserInfos(courseId, slides, periodStart).OrderByDescending(GetRating).ToArray();
			return PartialView(new UserProgressViewModel
			{
				Slides = slides,
				Users = users,
				GroupsNames = groupsRepo.GetUsersGroupsNamesAsStrings(courseId, users.Select(u => u.UserId), User),
				CourseId = courseId
			});
		}

		private DailyStatistics[] GetDailyStatistics(IEnumerable<Slide> slides = null)
		{
			var slideIds = slides?.Select(s => s.Id).ToArray();
			var lastDay = DateTime.Now.Date.Add(new TimeSpan(0, 23, 59, 59, 999));
			var firstDay = lastDay.Date.AddDays(-14);
			var tasks = GetTasksSolvedStats(slideIds, firstDay, lastDay);
			var quizes = GetQuizPassedStats(slideIds, firstDay, lastDay);
			var visits = GetSlidesVisitedStats(slideIds, firstDay, lastDay);
			var result =
				Enumerable.Range(0, 14)
					.Select(diff => lastDay.AddDays(-diff).Date)
					.Select(date => new DailyStatistics
					{
						Day = date,
						TasksSolved = tasks.GetOrDefault(date, 0) + quizes.GetOrDefault(date, 0),
						SlidesVisited = visits.GetOrDefault(date, Tuple.Create(0, 0)).Item1,
						Score = visits.GetOrDefault(date, Tuple.Create(0, 0)).Item2
					})
					.ToArray();
			return result;
		}

		private Dictionary<DateTime, Tuple<int, int>> SumByDays(IQueryable<Visit> actions)
		{
			var q = from s in actions
				group s by DbFunctions.TruncateTime(s.Timestamp)
				into day
				select new { day.Key, sum = day.Sum(v => v.Score), count = day.Select(d => new { d.UserId, d.SlideId }).Distinct().Count() };
			return q.ToDictionary(d => d.Key.Value, d => Tuple.Create(d.count, d.sum));
		}

		private IQueryable<T> FilterBySlides<T>(IQueryable<T> source, IEnumerable<Guid> slideIds) where T : class, ISlideAction
		{
			return slideIds == null ? source : source.Where(s => slideIds.Contains(s.SlideId));
		}

		private IQueryable<T> FilterByTime<T>(IQueryable<T> source, DateTime firstDay, DateTime lastDay) where T : class, ITimedSlideAction
		{
			return source.Where(s => s.Timestamp > firstDay && s.Timestamp <= lastDay);
		}

		private Dictionary<DateTime, int> GroupByDays<T>(IQueryable<T> actions) where T : class, ITimedSlideAction
		{
			var q = from s in actions
				group s by DbFunctions.TruncateTime(s.Timestamp)
				into day
				select new { day.Key, count = day.Select(d => new { d.UserId, d.SlideId }).Distinct().Count() };
			return q.ToDictionary(d => d.Key.Value, d => d.count);
		}

		private Dictionary<DateTime, int> GetTasksSolvedStats(IEnumerable<Guid> slideIds, DateTime firstDay, DateTime lastDay)
		{
			return GroupByDays(FilterByTime(FilterBySlides(db.UserExerciseSubmissions, slideIds), firstDay, lastDay).Where(s => s.AutomaticCheckingIsRightAnswer));
		}

		private Dictionary<DateTime, int> GetQuizPassedStats(IEnumerable<Guid> slideIds, DateTime firstDay, DateTime lastDay)
		{
			return GroupByDays(FilterByTime(FilterBySlides(db.UserQuizSubmissions, slideIds), firstDay, lastDay));
		}

		private Dictionary<DateTime, Tuple<int, int>> GetSlidesVisitedStats(IEnumerable<Guid> slideIds, DateTime firstDay, DateTime lastDay)
		{
			return SumByDays(FilterByTime(FilterBySlides(db.Visits, slideIds), firstDay, lastDay));
		}

		private SlideRateStats[] GetSlideRateStats(Course course, IEnumerable<Slide> slides)
		{
			var courseId = course.Id;
			var rates =
				(from rate in db.SlideRates
					where rate.CourseId == courseId
					group rate by new { rate.SlideId, rate.Rate }
					into slideRate
					select new
					{
						slideRate.Key.SlideId,
						slideRate.Key.Rate,
						count = slideRate.Count()
					})
				.ToLookup(r => r.SlideId);
			return slides.Select(s => new SlideRateStats
			{
				SlideId = s.Id,
				SlideTitle = s.Title,
				NotUnderstand = rates[s.Id].Where(r => r.Rate == SlideRates.NotUnderstand).Sum(r => r.count),
				Good = rates[s.Id].Where(r => r.Rate == SlideRates.Good).Sum(r => r.count),
				Trivial = rates[s.Id].Where(r => r.Rate == SlideRates.Trivial).Sum(r => r.count),
			}).ToArray();
		}

		private double GetRating(UserInfo user)
		{
			return
				user.SlidesSlideInfo.Sum(
					s =>
						(s.IsVisited ? 1 : 0)
						+ (s.IsExerciseSolved ? 1 : 0)
						+ (s.IsQuizPassed ? s.QuizPercentage / 100.0 : 0));
		}

		private IEnumerable<UserInfo> GetUserInfos(string courseId, Slide[] slides, DateTime periodStart, DateTime? periodFinish = null)
		{
			if (!periodFinish.HasValue)
				periodFinish = DateTime.Now;

			var slidesIds = slides.Select(s => s.Id).ToImmutableHashSet();

			var dq = visitsRepo.GetVisitsInPeriod(courseId, slidesIds, periodStart, periodFinish.Value)
				.Select(v => v.UserId)
				.Distinct()
				.Join(db.Visits, s => s, v => v.UserId, (s, visiters) => visiters)
				.Where(v => slidesIds.Contains(v.SlideId))
				.Select(v => new { v.UserId, v.User.UserName, v.SlideId, v.IsPassed, v.Score, v.AttemptsCount })
				.ToList();

			var r = dq.GroupBy(v => new { v.UserId, v.UserName }).Select(u => new UserInfo
			{
				UserId = u.Key.UserId,
				UserName = u.Key.UserName,
				SlidesSlideInfo = GetSlideInfo(slides, u.Select(arg => Tuple.Create(arg.SlideId, arg.IsPassed, arg.Score, arg.AttemptsCount)))
			});

			return r;
		}

		private static UserSlideInfo[] GetSlideInfo(IEnumerable<Slide> slides, IEnumerable<Tuple<Guid, bool, int, int>> slideResults)
		{
			var results = slideResults.GroupBy(tuple => tuple.Item1).ToDictionary(g => g.Key, g => g.First());
			var defaultValue = Tuple.Create(Guid.Empty, false, 0, 0);
			return slides
				.Select(slide => new
				{
					slide,
					result = results.GetOrDefault(slide.Id, defaultValue)
				})
				.Select(r => new UserSlideInfo
				{
					AttemptsCount = r.result.Item4,
					IsExerciseSolved = r.result.Item2,
					IsQuizPassed = r.result.Item2,
					QuizPercentage = r.slide is QuizSlide ? (double)r.result.Item3 / r.slide.MaxScore : 0.0,
					IsVisited = results.ContainsKey(r.slide.Id)
				})
				.ToArray();
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		public ActionResult UserSolutions(string courseId, string userId, Guid slideId, int? version = null)
		{
			var user = db.Users.Find(userId);
			if (user == null || user.IsDeleted)
				return HttpNotFound();
			
			var course = courseManager.GetCourse(courseId);
			var slide = course.FindSlideById(slideId) as ExerciseSlide;
			if (slide == null)
				return RedirectToAction("CourseInfo", "Account", new { userId = userId, courseId });
			
			var model = new UserSolutionsViewModel
			{
				User = user,
				Course = course,
				GroupsNames = groupsRepo.GetUserGroupsNamesAsString(course.Id, userId, User),
				Slide = slide,
				SubmissionId = version
			};
			return View(model);
		}
	}

	public class StatisticsParams
	{
		public string CourseId { get; set; }

		public string PeriodStart { get; set; }
		public string PeriodFinish { get; set; }

		private static readonly string[] dateFormats = { "dd.MM.yyyy" };

		public DateTime PeriodStartDate
		{
			get
			{
				var defaultPeriodStart = GetDefaultPeriodStart();
				if (string.IsNullOrEmpty(PeriodStart))
					return defaultPeriodStart;
				if (!DateTime.TryParseExact(PeriodStart, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
					return defaultPeriodStart;
				return result;
			}
		}

		private static DateTime GetDefaultPeriodStart()
		{
			var now = DateTime.Now;
			return new DateTime(2015, 1, 1);
		}

		public DateTime PeriodFinishDate
		{
			get
			{
				var defaultPeriodFinish = DateTime.Now.Date;
				if (string.IsNullOrEmpty(PeriodFinish))
					return defaultPeriodFinish;
				if (!DateTime.TryParseExact(PeriodFinish, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
					return defaultPeriodFinish;
				return result;
			}
		}
	}

	public class CourseStatisticsParams : StatisticsParams
	{
		/* Course statistics can't be filtered by dates */
		public new DateTime PeriodStartDate => DateTime.MinValue;

		public new DateTime PeriodFinishDate => DateTime.MaxValue.Subtract(TimeSpan.FromDays(2));
	}

	public class UnitStatisticsParams : StatisticsParams
	{
		public Guid? UnitId { get; set; }
	}

	public class UserUnitStatisticsPageModel
	{
		public Course Course { get; set; }
		public Unit Unit { get; set; }
		public ApplicationUser User { get; set; }
		public List<UserExerciseSubmission> Submissions { get; set; }
		public List<UserExerciseSubmission> ReviewedSubmissions { get; set; }
		public Dictionary<Guid, Slide> Slides { get; set; }
		public Dictionary<Guid, int> Scores { get; set; }
		public Unit PreviousUnit { get; set; }
		public Unit NextUnit { get; set; }
	}

	public class UserSolutionsViewModel
	{
		public ExerciseSlide Slide { get; set; }
		public ApplicationUser User { get; set; }
		public Course Course { get; set; }
		public string GroupsNames { get; set; }
		public int? SubmissionId { get; set; }
	}

	public class UserProgressViewModel
	{
		public string CourseId;
		public UserInfo[] Users;
		public Dictionary<string, string> GroupsNames;
		public Slide[] Slides;
	}
}