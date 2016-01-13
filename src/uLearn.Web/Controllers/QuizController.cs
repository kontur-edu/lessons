using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using uLearn.Quizes;
using uLearn.Web.DataContexts;
using uLearn.Web.FilterAttributes;
using uLearn.Web.LTI;
using uLearn.Web.Models;

namespace uLearn.Web.Controllers
{
	[ULearnAuthorize]
	public class QuizController : Controller
	{
		private const int MAX_DROPS_COUNT = 1;
		public const int MAX_FILLINBLOCK_SIZE = 1024;

		private readonly CourseManager courseManager;
		private readonly ULearnDb db = new ULearnDb();
		private readonly UserQuizzesRepo userQuizzesRepo = new UserQuizzesRepo();
		private readonly VisitsRepo visitsRepo = new VisitsRepo();

		public QuizController()
			: this(WebCourseManager.Instance)
		{
		}

		public QuizController(CourseManager courseManager)
		{
			this.courseManager = courseManager;
		}

		internal class QuizAnswer
		{
			public string QuizType;
			public string QuizId;
			public string ItemId;
			public string Text;

			public QuizAnswer(string type, string quizId, string itemId, string text)
			{
				QuizType = type;
				QuizId = quizId;
				ItemId = itemId;
				Text = text;
			}
		}


		internal class QuizInfoForDb
		{
			public string QuizId;
			public string ItemId;
			public string Text;
			public bool IsRightAnswer;
			public Type QuizType;
			public bool IsRightQuizBlock;
		}

		[HttpPost]
		[ULearnAuthorize(MinAccessLevel = CourseRoles.Tester)]
		public async Task<ActionResult> ClearAnswers(string courseId, string slideId, bool isLti)
		{
			var slide = courseManager.GetCourse(courseId).GetSlideById(slideId);
			var userId = User.Identity.GetUserId();
			await userQuizzesRepo.RemoveAnswers(userId, slideId);
			await visitsRepo.RemoveAttempts(slideId, userId);
			var model = new { courseId, slideIndex = slide.Index };
			if (isLti)
			{
				LtiUtils.SubmitScore(slide, userId);
				return RedirectToAction("LtiSlide", "Course", model);
			}
			return RedirectToAction("Slide", "Course", model);
		}

		[HttpPost]
		public async Task<string> SubmitQuiz(string courseId, string slideIndex, string answer, bool isLti)
		{
			var intSlideIndex = int.Parse(slideIndex);
			var course = courseManager.GetCourse(courseId);
			var userId = User.Identity.GetUserId();
			var slide = course.Slides[intSlideIndex];
			var slideId = slide.Id;

			if (visitsRepo.IsPassed(userId, slideId))
				return "already answered";
			var time = DateTime.Now;
			var answers = JsonConvert.DeserializeObject<List<QuizAnswer>>(answer).GroupBy(x => x.QuizId);
			var quizBlockWithTaskCount = course.Slides[intSlideIndex].Blocks.Count(x => x is AbstractQuestionBlock);
			var incorrectQuizzes = new List<string>();
			var fillInBlockType = typeof(FillInBlock);
			var allQuizInfos = new List<QuizInfoForDb>();
			foreach (var ans in answers)
			{
				var quizInfos = CreateQuizInfo(course, intSlideIndex, ans);
				foreach (var quizInfo in quizInfos)
				{
					allQuizInfos.Add(quizInfo);
					if (!quizInfo.IsRightAnswer && quizInfo.QuizType == fillInBlockType)
						incorrectQuizzes.Add(ans.Key);
				}
			}
			var blocksInAnswerCount = allQuizInfos.Select(x => x.QuizId).Distinct().Count();
			if (blocksInAnswerCount != quizBlockWithTaskCount)
				return "has empty blocks";
			var score = allQuizInfos
				.Where(forDb => forDb.IsRightQuizBlock)
				.Select(forDb => forDb.QuizId)
				.Distinct()
				.Count();
			foreach (var quizInfoForDb in allQuizInfos)
				await userQuizzesRepo.AddUserQuiz(courseId, quizInfoForDb.IsRightAnswer, quizInfoForDb.ItemId, quizInfoForDb.QuizId,
					slideId, quizInfoForDb.Text, userId, time, quizInfoForDb.IsRightQuizBlock);

			await visitsRepo.AddAttempt(slideId, userId, score);
			if (isLti)
				LtiUtils.SubmitScore(slide, userId);

			return string.Join("*", incorrectQuizzes.Distinct());
		}

		private IEnumerable<QuizInfoForDb> CreateQuizInfo(Course course, int slideIndex, IGrouping<string, QuizAnswer> answer)
		{
			var slide = (QuizSlide)course.Slides[slideIndex];
			var block = slide.GetBlockById(answer.Key);
			if (block is FillInBlock)
				return CreateQuizInfoForDb(block as FillInBlock, answer.First().Text);
			if (block is ChoiceBlock)
				return CreateQuizInfoForDb(block as ChoiceBlock, answer);
			return CreateQuizInfoForDb(block as IsTrueBlock, answer);
		}

		private IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(IsTrueBlock isTrueBlock, IGrouping<string, QuizAnswer> data)
		{
			var isTrue = isTrueBlock.IsRight(data.First().ItemId);
			return new List<QuizInfoForDb>
			{
				new QuizInfoForDb
				{
					QuizId = isTrueBlock.Id,
					ItemId = null,
					IsRightAnswer = isTrue,
					Text = data.First().ItemId,
					QuizType = typeof(IsTrueBlock),
					IsRightQuizBlock = isTrue
				}
			};
		}

		private IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(ChoiceBlock choiceBlock, IGrouping<string, QuizAnswer> answers)
		{
			if (!choiceBlock.Multiple)
			{
				var answerItemId = answers.First().ItemId;
				var isTrue = choiceBlock.Items.First(x => x.Id == answerItemId).IsCorrect;
				return new List<QuizInfoForDb>
				{
					new QuizInfoForDb
					{
						QuizId = choiceBlock.Id,
						ItemId = answerItemId,
						IsRightAnswer = isTrue,
						Text = null,
						QuizType = typeof (ChoiceBlock),
						IsRightQuizBlock = isTrue
					}
				};
			}
			var ans = answers.Select(x => x.ItemId).ToList()
				.Select(x => new QuizInfoForDb
				{
					QuizId = choiceBlock.Id,
					IsRightAnswer = choiceBlock.Items.Where(y => y.IsCorrect).Any(y => y.Id == x),
					ItemId = x,
					Text = null,
					QuizType = typeof(ChoiceBlock),
					IsRightQuizBlock = false
				}).ToList();
			var isRightQuizBlock = ans.All(x => x.IsRightAnswer) &&
						 choiceBlock.Items.Where(x => x.IsCorrect)
							 .Select(x => x.Id)
							 .All(x => ans.Where(y => y.IsRightAnswer).Select(y => y.ItemId).Contains(x));
			foreach (var info in ans)
				info.IsRightQuizBlock = isRightQuizBlock;
			return ans;
		}

		private IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(FillInBlock fillInBlock, string data)
		{
			if (data.Length > MAX_FILLINBLOCK_SIZE)
				data = data.Substring(0, MAX_FILLINBLOCK_SIZE);
			var isTrue = fillInBlock.Regexes.Any(regex => regex.Regex.IsMatch(data));
			return new List<QuizInfoForDb>
			{
				new QuizInfoForDb
				{
					QuizId = fillInBlock.Id,
					ItemId = null,
					IsRightAnswer = isTrue,
					Text = data,
					QuizType = typeof(FillInBlock),
					IsRightQuizBlock = isTrue
				}
			};
		}

		[HttpGet]
		[ULearnAuthorize(MinAccessLevel = CourseRoles.Instructor)]
		public ActionResult Analytics(string courseId, int slideIndex, DateTime periodStart)
		{
			var course = courseManager.GetCourse(courseId);
			var quizSlide = (QuizSlide)course.Slides[slideIndex];
			var dict = new SortedDictionary<string, List<QuizAnswerInfo>>();
			var groups = new Dictionary<string, string>();
			var passes = db.UserQuizzes
				.Where(q => quizSlide.Id == q.SlideId && !q.isDropped && periodStart <= q.Timestamp)
				.GroupBy(q => q.UserId)
				.Join(db.Users, g => g.Key, user => user.Id, (g, user) => new { user.UserName, user.GroupName, UserQuizzes = g.ToList() })
				.ToList();
			foreach (var pass in passes)
			{
				dict[pass.UserName] = GetUserQuizAnswers(quizSlide, pass.UserQuizzes).ToList();
				groups[pass.UserName] = pass.GroupName;
			}
			var rightAnswersCount = dict.Values
				.SelectMany(list => list
					.Where(info => info.IsRight)
					.Select(info => new { info.Id, info.IsRight }))
				.GroupBy(arg => arg.Id)
				.ToDictionary(grouping => grouping.Key, grouping => grouping.Count());
			return PartialView(new QuizAnalyticsModel
			{
				UserAnswers = dict,
				QuizSlide = quizSlide,
				RightAnswersCount = rightAnswersCount,
				Group = groups
			});
		}

		private static IEnumerable<QuizAnswerInfo> GetUserQuizAnswers(QuizSlide slide, IEnumerable<UserQuiz> userQuizzes)
		{
			var answers = userQuizzes.GroupBy(q => q.QuizId).ToDictionary(g => g.Key, g => g.ToList());
			foreach (var block in slide.Blocks.OfType<AbstractQuestionBlock>())
				if (block is FillInBlock)
					yield return GetFillInBlockAnswerInfo(answers, block.Id, block.QuestionIndex);
				else if (block is ChoiceBlock)
					yield return GetChoiceBlockAnswerInfo(answers, (ChoiceBlock)block, block.QuestionIndex);
				else if (block is IsTrueBlock)
					yield return GetIsTrueBlockAnswerInfo(answers, block.Id, block.QuestionIndex);
		}

		private static QuizAnswerInfo GetFillInBlockAnswerInfo(IReadOnlyDictionary<string, List<UserQuiz>> answers, string quizId, int questionIndex)
		{
			UserQuiz answer = null;
			if (answers.ContainsKey(quizId))
				answer = answers[quizId].FirstOrDefault();
			return new FillInBlockAnswerInfo
			{
				Answer = answer == null ? null : answer.Text,
				IsRight = answer != null && answer.IsRightAnswer,
				Id = questionIndex.ToString()
			};
		}

		private static QuizAnswerInfo GetChoiceBlockAnswerInfo(IReadOnlyDictionary<string, List<UserQuiz>> answers, ChoiceBlock block, int questionIndex)
		{
			IEnumerable<UserQuiz> answer = new List<UserQuiz>();
			if (answers.ContainsKey(block.Id))
				answer = answers[block.Id].Where(q => q.ItemId != null);

			var ans = new SortedDictionary<string, bool>();
			foreach (var item in block.Items)
			{
				ans[item.Id] = false;
			}

			var isRight = false;
			foreach (var quizItem in answer.Where(quizItem => ans.ContainsKey(quizItem.ItemId)))
			{
				isRight = quizItem.IsRightQuizBlock;
				ans[quizItem.ItemId] = true;
			}

			return new ChoiceBlockAnswerInfo
			{
				AnswersId = ans,
				Id = questionIndex.ToString(),
				RealyRightAnswer = new HashSet<string>(block.Items.Where(x => x.IsCorrect).Select(x => x.Id)),
				IsRight = isRight
			};
		}


		private static QuizAnswerInfo GetIsTrueBlockAnswerInfo(IReadOnlyDictionary<string, List<UserQuiz>> answers, string quizId, int questionIndex)
		{
			UserQuiz answer = null;
			if (answers.ContainsKey(quizId))
				answer = answers[quizId].FirstOrDefault();
			return new IsTrueBlockAnswerInfo
			{
				IsAnswered = answer != null,
				Answer = answer != null && answer.Text == "True",
				Id = questionIndex.ToString(),
				IsRight = answer != null && answer.IsRightAnswer
			};
		}

		[HttpPost]
		public async Task<ActionResult> DropQuiz(string courseId, string slideId, bool isLti)
		{
			var slide = courseManager.GetCourse(courseId).GetSlideById(slideId);
			if (slide is QuizSlide)
			{
				var userId = User.Identity.GetUserId();
				if (userQuizzesRepo.GetQuizDropStates(courseId, userId, slideId).Count(b => b) < GetMaxDropCount(slide as QuizSlide) &&
					!userQuizzesRepo.GetQuizBlocksTruth(courseId, userId, slideId).All(b => b.Value))
				{
					await userQuizzesRepo.DropQuiz(userId, slideId);
					await visitsRepo.DropAttempt(slideId, userId);
					if (isLti)
						LtiUtils.SubmitScore(slide, userId);
				}
			}
			var model = new { courseId, slideIndex = slide.Index, isLti };
			if (isLti)
				return RedirectToAction("LtiSlide", "Course", model);
			return RedirectToAction("Slide", "Course", model);
		}

		[AllowAnonymous]
		public ActionResult Quiz(QuizSlide slide, string courseId, string userId, bool isGuest = true, bool isLti = false)
		{
			if (isGuest)
				return PartialView(GuestQuiz(slide, courseId));
			var slideId = slide.Id;
			var maxDropCount = GetMaxDropCount(slide);
			var state = GetQuizState(courseId, userId, slideId, maxDropCount);
			var resultsForQuizes = GetResultForQuizes(courseId, userId, slideId, state.Item1);
			var model = new QuizModel
			{
				CourseId = courseId,
				Slide = slide,
				QuizState = state.Item1,
				TryNumber = state.Item2,
				MaxDropCount = maxDropCount,
				ResultsForQuizes = resultsForQuizes,
				AnswersToQuizes =
					userQuizzesRepo.GetAnswersForShowOnSlide(courseId, slide,
						userId),
				IsLti = isLti
			};

			if (model.QuizState != QuizState.NotPassed && model.RightAnswers == model.QuestionsCount)
				model.QuizState = QuizState.Total;

			return PartialView(model);
		}

		private QuizModel GuestQuiz(QuizSlide slide, string courseId)
		{
			return new QuizModel
			{
				CourseId = courseId,
				Slide = slide,
				IsGuest = true,
				QuizState = QuizState.NotPassed
			};
		}

		private static int GetMaxDropCount(QuizSlide quizSlide)
		{
			if (quizSlide == null)
				return MAX_DROPS_COUNT;
			var maxDropCount = quizSlide.MaxDropCount;
			return maxDropCount == 0 ? MAX_DROPS_COUNT : maxDropCount;
		}

		private Dictionary<string, bool> GetResultForQuizes(string courseId, string userId, string slideId, QuizState state)
		{
			return userQuizzesRepo.GetQuizBlocksTruth(courseId, userId, slideId);
		}

		private Tuple<QuizState, int> GetQuizState(string courseId, string userId, string slideId, int maxDropCount)
		{
			var states = userQuizzesRepo.GetQuizDropStates(courseId, userId, slideId).ToList();
			if (states.Count > maxDropCount)
				return Tuple.Create(QuizState.Total, states.Count);
			if (states.Any(b => !b))
				return Tuple.Create(QuizState.Subtotal, states.Count);
			return Tuple.Create(QuizState.NotPassed, states.Count);
		}
	}
}