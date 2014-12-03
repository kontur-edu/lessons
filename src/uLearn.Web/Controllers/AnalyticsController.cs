using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using uLearn.Web.DataContexts;
using uLearn.Web.Models;

namespace uLearn.Web.Controllers
{
	[Authorize(Roles = LmsRoles.Instructor)]
	public class AnalyticsController : Controller
	{
		private readonly ULearnDb db = new ULearnDb();
		private readonly CourseManager courseManager;
		private readonly VisitersRepo visitersRepo = new VisitersRepo();
		private readonly SlideRateRepo slideRateRepo = new SlideRateRepo();
		private readonly UserSolutionsRepo userSolutionsRepo = new UserSolutionsRepo();
		private readonly SlideHintRepo slideHintRepo = new SlideHintRepo();
		private readonly UserQuizzesRepo userQuizzesRepo = new UserQuizzesRepo(); //TODO use in statistics


		public AnalyticsController()
			: this(WebCourseManager.Instance)
		{
		}

		public AnalyticsController(CourseManager courseManager)
		{
			this.courseManager = courseManager;
		}

		public ActionResult TotalStatistics(string courseId)
		{
			var model = CreateTotalStatistics(courseId);
			return View(model);
		}

		private AnalyticsTablePageModel CreateTotalStatistics(string courseId)
		{
			var course = courseManager.GetCourse(courseId);
			var tableInfo = CreateTotalStatisticsInfo(course);
			var model = new AnalyticsTablePageModel
			{
				TableInfo = tableInfo,
				CourseId = courseId
			};
			return model;
		}

		private Dictionary<string, AnalyticsTableInfo> CreateTotalStatisticsInfo(Course course)
		{
			var tableInfo = new Dictionary<string, AnalyticsTableInfo>();
			foreach (var slide in course.Slides)
			{
				var exerciseSlide = (slide as ExerciseSlide);
				var quizSlide = (slide as QuizSlide);
				var isExercise = exerciseSlide != null;
				var isQuiz = quizSlide != null;
				var hintsCountOnSlide = isExercise ? exerciseSlide.HintsMd.Count() : 0;
				var visitersCount = visitersRepo.GetVisitersCount(slide.Id, course.Id);
				tableInfo.Add(slide.Index + ". " + slide.Info.UnitName + ": " + slide.Title, new AnalyticsTableInfo
				{
					Rates = slideRateRepo.GetRates(slide.Id, course.Id),
					VisitersCount = visitersCount,
					IsExercise = isExercise,
					IsQuiz = isQuiz,
					SolversPercent = isExercise
						? (visitersCount == 0 ? 0 : (int)((double)userSolutionsRepo.GetAcceptedSolutionsCount(slide.Id, course.Id) / visitersCount) * 100)
						: isQuiz
							? (visitersCount == 0 ? 0 : (int)((double)userQuizzesRepo.GetSubmitQuizCount(slide.Id, course.Id) / visitersCount) * 100)
							: 0,
					SuccessQuizPercentage = isQuiz ? userQuizzesRepo.GetAverageStatistics(slide.Id, course.Id) : 0,
					TotalHintCount = hintsCountOnSlide,
					HintUsedPercent = isExercise ? slideHintRepo.GetHintUsedPercent(slide.Id, course.Id, hintsCountOnSlide, db.Users.Count()) : 0
				});
			}
			return tableInfo;
		}

		public ActionResult UsersStatistics(string courseId)
		{
			var course = courseManager.GetCourse(courseId);
			var model = CreateUsersStatisticsModel(course);
			return View(model);
		}

		private UsersStatsPageModel CreateUsersStatisticsModel(Course course)
		{
			return new UsersStatsPageModel
			{
				CourseId = course.Id,
				UserStats = CreateUserStats(course),
				UnitNamesInOrdered = course.Slides.GroupBy(x => x.Info.UnitName).Select(x => x.Key).ToList()
			};
		}

		private Dictionary<string, SortedDictionary<string, int>> CreateUserStats(Course course)
		{
			var users = db.Users.ToList();
			var ans = new Dictionary<string, SortedDictionary<string, int>>();
			foreach (var user in users)
				ans[user.UserName] = new SortedDictionary<string, int>();
			var acceptedSolutionsForUsers = new Dictionary<string, HashSet<string>>();
			var unitNames = new Dictionary<string, string>();
			foreach (var slide in course.Slides)
				unitNames[slide.Id] = slide.Info.UnitName;
			var slideCountInUnit = new Dictionary<string, int>();
			FillCapacityOfUnits(course, slideCountInUnit);
			FillTheTable(unitNames, acceptedSolutionsForUsers, ans);
			InitialEmptyUnit(slideCountInUnit, ans);
			CalculatePercent(ans, slideCountInUnit);
			return ans;
		}

		private static void FillCapacityOfUnits(Course course, Dictionary<string, int> slideCountInUnit)
		{
			foreach (var slide in course.Slides.Where(x => x is ExerciseSlide || x is QuizSlide))
			{
				if (!slideCountInUnit.ContainsKey(slide.Info.UnitName))
					slideCountInUnit[slide.Info.UnitName] = 0;
				slideCountInUnit[slide.Info.UnitName]++;
			}
		}

		private static void CalculatePercent(Dictionary<string, SortedDictionary<string, int>> ans, Dictionary<string, int> slideCountInUnit)
		{
			foreach (var userRates in ans.ToList())
				foreach (var rate in userRates.Value.ToList())
					ans[userRates.Key][rate.Key] = (int)(100 * (double)rate.Value / slideCountInUnit[rate.Key]);
		}

		private static void InitialEmptyUnit(Dictionary<string, int> slideCountInUnit, Dictionary<string, SortedDictionary<string, int>> ans)
		{
			foreach (var unit in slideCountInUnit.Keys)
			{
				var unitName = unit;
				foreach (var user in ans.Keys.Where(user => !ans[user].ContainsKey(unitName)))
					ans[user].Add(unit, 0);
			}
		}

		private void FillTheTable(Dictionary<string, string> unitNames, Dictionary<string, HashSet<string>> acceptedSolutionsForUsers, Dictionary<string, SortedDictionary<string, int>> ans)
		{
			foreach (var userSolution in db.UserSolutions.Where(x => x.IsRightAnswer))
			{
				if (!unitNames.ContainsKey(userSolution.SlideId)) //пока в старой базе есть старые записи с неправильными ID
					continue;
				var user = db.Users.Find(userSolution.UserId);
				if (user == null)
					continue;
				var userName = user.UserName;
				if (!acceptedSolutionsForUsers.ContainsKey(userName))
					acceptedSolutionsForUsers[userName] = new HashSet<string>();
				if (acceptedSolutionsForUsers[userName].Contains(userSolution.SlideId))
					continue;
				acceptedSolutionsForUsers[userName].Add(userSolution.SlideId);
				if (!ans[userName].ContainsKey(unitNames[userSolution.SlideId]))
					ans[userName][unitNames[userSolution.SlideId]] = 0;
				ans[userName][unitNames[userSolution.SlideId]]++;
			}
			foreach (var quiz in db.UserQuizzes)
			{
				if (!unitNames.ContainsKey(quiz.SlideId)) //пока в старой базе есть старые записи с неправильными ID
					continue;
				var user = db.Users.Find(quiz.UserId);
				if (user == null)
					continue;
				var userName = user.UserName;
				if (!acceptedSolutionsForUsers.ContainsKey(userName))
					acceptedSolutionsForUsers[userName] = new HashSet<string>();
				if (acceptedSolutionsForUsers[userName].Contains(quiz.SlideId))
					continue;
				acceptedSolutionsForUsers[userName].Add(quiz.SlideId);
				if (!ans[userName].ContainsKey(unitNames[quiz.SlideId]))
					ans[userName][unitNames[quiz.SlideId]] = 0;
				ans[userName][unitNames[quiz.SlideId]]++;
			}
		}

		[Authorize]
		public ActionResult PersonalStatistics(string courseId)
		{
			var course = courseManager.GetCourse(courseId);
			return View(CreatePersonalStaticticsModel(course));
		}

		private PersonalStatisticPageModel CreatePersonalStaticticsModel(Course course)
		{
			var userId = User.Identity.GetUserId();
			return new PersonalStatisticPageModel
			{
				CourseId = course.Id,
				PersonalStatistics =
					course.Slides
						.Select((slide, slideIndex) => new PersonalStatisticsInSlide
						{
							UnitName = slide.Info.UnitName,
							SlideTitle = slide.Title,
							SlideIndex = slideIndex,
							IsExercise = slide is ExerciseSlide,
							IsQuiz = slide is QuizSlide,
							IsSolved = userSolutionsRepo.IsUserPassedTask(course.Id, slide.Id, userId) || userQuizzesRepo.IsQuizSlidePassed(course.Id, userId, slide.Id),
							IsVisited = visitersRepo.IsUserVisit(course.Id, slide.Id, userId),
							UserRate = slideRateRepo.GetUserRate(course.Id, slide.Id, userId),
							HintsCountOnSlide = slide is ExerciseSlide ? (slide as ExerciseSlide).HintsMd.Count() : 0,
							HintUsedPercent = slide is ExerciseSlide ? slideHintRepo.GetHintUsedPercentForUser(course.Id, slide.Id, userId, (slide as ExerciseSlide).HintsMd.Count()) : 0
						})
						.ToArray()
			};
		}

		public ActionResult UnitStatistics(string courseId, string unitName)
		{
			var course = courseManager.GetCourse(courseId);
			var slides = course.Slides
				.Where(s => s.Info.UnitName == unitName).ToArray();
			var model = new UnitStatisticPageModel
			{
				CourseId = courseId,
				UnitName = unitName,
				Slides = slides,
				SlideRateStats = GetSlideRateStats(course, slides),
			};
			return View(model);
		}

		public ActionResult SlideRatings(string courseId, string unitName)
		{
			var course = courseManager.GetCourse(courseId);
			var slides = course.Slides
				.Where(s => s.Info.UnitName == unitName).ToArray();
			var model = GetSlideRateStats(course, slides);
			return PartialView(model);
		}

		public ActionResult DailyStatistics(string courseId, string unitName)
		{
			IEnumerable<Slide> slides = null;
			if (courseId != null)
			{
				var course = courseManager.GetCourse(courseId);
				slides = course.Slides.Where(s => unitName == null || s.Info.UnitName == unitName);
			}
			var model = GetDailyStatistics(slides);
			return PartialView(model);
		}

		public ActionResult SystemStatistics()
		{
			return View();
		}

		public ActionResult UsersProgress(string courseId, string unitName)
		{
			var course = courseManager.GetCourse(courseId);
			var slides = course.Slides
				.Where(s => s.Info.UnitName == unitName).ToArray();
			var users = GetUserInfos(course, slides).OrderByDescending(GetRating).ToArray();
			return PartialView(new UserProgressViewModel { Slides = slides, Users = users, CourseId = courseId });
		}

		private DailyStatistics[] GetDailyStatistics(IEnumerable<Slide> slides = null)
		{
			var slideIds = slides == null ? null : slides.Select(s => s.Id).ToArray();
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
						TasksSolved = tasks.Get(date, 0) + quizes.Get(date, 0),
						SlidesVisited = visits.Get(date, 0)
					})
					.ToArray();
			return result;
		}

		private IQueryable<T> FilterBySlides<T>(IQueryable<T> source, IEnumerable<string> slideIds) where T : class, ISlideAction
		{
			return slideIds == null ? source : source.Where(s => slideIds.Contains(s.SlideId));
		}

		private IQueryable<T> FilterByTime<T>(IQueryable<T> source, DateTime firstDay, DateTime lastDay) where T : class, ISlideAction
		{
			return source.Where(s => s.Timestamp > firstDay && s.Timestamp <= lastDay);
		}

		private Dictionary<DateTime, int> GroupByDays<T>(IQueryable<T> actions) where T : class, ISlideAction
		{
			var q = from s in actions
				group s by DbFunctions.TruncateTime(s.Timestamp)
				into day
				select new { day.Key, count = day.Select(d => new { d.UserId, d.SlideId }).Distinct().Count() };
			return q.ToDictionary(d => d.Key.Value, d => d.count);
		}

		private Dictionary<DateTime, int> GetTasksSolvedStats(IEnumerable<string> slideIds, DateTime firstDay, DateTime lastDay)
		{
			return GroupByDays(FilterByTime(FilterBySlides(db.UserSolutions, slideIds), firstDay, lastDay).Where(s => s.IsRightAnswer));
		}

		private Dictionary<DateTime, int> GetQuizPassedStats(IEnumerable<string> slideIds, DateTime firstDay, DateTime lastDay)
		{
			return GroupByDays(FilterByTime(FilterBySlides(db.UserQuizzes, slideIds), firstDay, lastDay));
		}

		private Dictionary<DateTime, int> GetSlidesVisitedStats(IEnumerable<string> slideIds, DateTime firstDay, DateTime lastDay)
		{
			return GroupByDays(FilterByTime(FilterBySlides(db.Visiters, slideIds), firstDay, lastDay));
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

		private IEnumerable<UserInfo> GetUserInfos(Course course, Slide[] slides)
		{
			var slideIds = slides.Select(s => s.Id).ToArray();
			var courseId = course.Id;
			var dataQuery =
				from v in db.Visiters
				where
					v.CourseId == courseId
					&& slideIds.Contains(v.SlideId)
				join s in db.UserSolutions on new { v.SlideId, v.UserId } equals new { s.SlideId, s.UserId } into sInner
				from ss in sInner.DefaultIfEmpty()
				join q in db.UserQuizzes on new { v.SlideId, v.UserId } equals new { q.SlideId, q.UserId } into qInner
				from qq in qInner.DefaultIfEmpty()
				join u in db.Users on v.UserId equals u.Id into users
				from uu in users
				select new
				{
					v.UserId,
					uu.UserName,
					uu.GroupName,
					v.SlideId,
					SolvedExercise = ss != null && ss.IsRightAnswer,
					IsRightQuiz = qq != null && qq.IsRightQuizBlock
				};
			//			Debug.Write(dataQuery.ToString());

			var res = from v in dataQuery.ToList()
				group v by new { v.UserId, v.UserName, v.GroupName }
				into u
				select FillUserInfo(
					new UserInfo
					{
						UserId = u.Key.UserId,
						UserName = u.Key.UserName,
						UserGroup = u.Key.GroupName
					},
					slides,
					u.Select(d => Tuple.Create(d.SlideId, d.SolvedExercise, d.IsRightQuiz))
					);
			return res;
		}

		private UserInfo FillUserInfo(UserInfo userInfo, IEnumerable<Slide> slides, IEnumerable<Tuple<string, bool, bool>> slideExerciseQuiz)
		{
			var lookup = slideExerciseQuiz.ToLookup(tuple => tuple.Item1);
			userInfo.SlidesSlideInfo = slides
				.Select(
					slide => new
					{
						slide,
						quizAnswers = lookup[slide.Id].Select(t => t.Item3),
						solutions = lookup[slide.Id].Select(t => t.Item2)
					}
				)
				.Select(
					slide => new UserSlideInfo
					{
						AttemptsCount = slide.solutions.Count(),
						IsExerciseSolved = slide.solutions.Any(sol => sol),
						IsQuizPassed = slide.quizAnswers.Any(),
						QuizPercentage = slide.quizAnswers.Any() ? (double)slide.quizAnswers.Count(q => q) / slide.quizAnswers.Count() : 0.0,
						IsVisited = slide.solutions.Any() // Если была запись в visited
					}).ToArray();
			return userInfo;
		}

		[HttpPost]
		public async Task<ActionResult> AddUserGroup(string groupName, string userId)
		{
			db.Users.First(x => x.Id == userId).GroupName = groupName;
			await db.SaveChangesAsync();
			return null;
		}

		public ActionResult ShowSolutions(string courseId, string userId, string slideId)
		{
			var solutions = db.UserSolutions.Where(s => s.CourseId == courseId && s.UserId == userId && s.SlideId == slideId).OrderByDescending(s => s.Timestamp).Take(10).ToList();
			var user = db.Users.Find(userId);
			var course = courseManager.GetCourse(courseId);
			var model = new UserSolutionsViewModel
			{
				User = user,
				Course = course,
				Slide = course.GetSlideById(slideId),
				Solutions = solutions
			};
			return View("UserSolutions", model);
		}
	}

	public class UserSolutionsViewModel
	{
		public ApplicationUser User { get; set; }
		public Course Course { get; set; }
		public List<UserSolution> Solutions { get; set; }
		public Slide Slide { get; set; }
	}

	public class UserProgressViewModel
	{
		public string CourseId;
		public UserInfo[] Users;
		public Slide[] Slides;
	}
}