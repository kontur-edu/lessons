using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Database;
using Database.DataContexts;
using Database.Extensions;
using Database.Models;
using JetBrains.Annotations;
using log4net;
using Metrics;
using uLearn.Web.Extensions;
using uLearn.Web.FilterAttributes;
using uLearn.Web.LTI;
using uLearn.Web.Models;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Slides.Quizzes.Blocks;
using Ulearn.Core.Extensions;

namespace uLearn.Web.Controllers
{
	[ULearnAuthorize]
	public class QuizController : Controller
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(QuizController));

		private const int defaultMaxTriesCount = 2;
		public const int InfinityTriesCount = int.MaxValue - 1;
		public const int MaxFillInBlockSize = 8 * 1024;

		private readonly ULearnDb db = new ULearnDb();
		private readonly CourseManager courseManager = WebCourseManager.Instance;
		private readonly MetricSender metricSender;

		private readonly UserQuizzesRepo userQuizzesRepo;
		private readonly VisitsRepo visitsRepo;
		private readonly GroupsRepo groupsRepo;
		private readonly SlideCheckingsRepo slideCheckingsRepo;
		private readonly NotificationsRepo notificationsRepo;
		private readonly UnitsRepo unitsRepo;

		public QuizController()
		{
			metricSender = new MetricSender(ApplicationConfiguration.Read<UlearnConfiguration>().GraphiteServiceName);

			userQuizzesRepo = new UserQuizzesRepo(db);
			visitsRepo = new VisitsRepo(db);
			groupsRepo = new GroupsRepo(db, courseManager);
			slideCheckingsRepo = new SlideCheckingsRepo(db);
			notificationsRepo = new NotificationsRepo(db);
			unitsRepo = new UnitsRepo(db);
		}

		[UsedImplicitly]
		private class QuizAnswer
		{
			public readonly string BlockType;
			public readonly string BlockId;
			public readonly string ItemId;
			public readonly string Text;

			public QuizAnswer(string type, string blockId, string itemId, string text)
			{
				BlockType = type;
				BlockId = blockId;
				ItemId = itemId;
				Text = text;
			}
		}

		private class QuizInfoForDb
		{
			public Type BlockType;			
			public string BlockId;
			public string ItemId;
			public string Text;
			public bool IsRightAnswer;
			public int QuizBlockScore;
			public int QuizBlockMaxScore;
		}

		public bool CanUserFillQuiz(QuizStatus status)
		{
			return status == QuizStatus.ReadyToSend || status == QuizStatus.WaitsForManualChecking;
		}

		[AllowAnonymous]
		public ActionResult Quiz(QuizSlide slide, string courseId, string userId, bool isGuest, bool isLti = false, ManualQuizChecking manualQuizCheckQueueItem = null, int? send = null, bool attempt = false)
		{
			metricSender.SendCount("quiz.show");
			if (isLti)
				metricSender.SendCount("quiz.show.lti");
			metricSender.SendCount($"quiz.show.{courseId}");
			metricSender.SendCount($"quiz.show.{courseId}.{slide.Id}");

			var course = courseManager.FindCourse(courseId);
			if (course == null)
				return HttpNotFound();
			
			if (isGuest)
			{
				metricSender.SendCount("quiz.show.to.guest");
				return PartialView(GuestQuiz(course, slide));
			}
			var slideId = slide.Id;
			var maxAttemptsCount = GetMaxAttemptsCount(courseId, slide);
			var userScores = GetUserScoresForBlocks(courseId, userId, slideId, manualQuizCheckQueueItem?.Submission);

			var score = userScores?.AsEnumerable().Sum(res => res.Value) ?? 0;
			
			var state = GetQuizState(courseId, userId, slideId, score, slide.MaxScore, manualQuizCheckQueueItem?.Submission);

			log.Info($"Показываю тест для пользователя {userId} в слайде {courseId}:{slide.Id}, isLti = {isLti}");
			
			/* If it's manual checking, change quiz state to IsChecking for correct rendering */
			if (manualQuizCheckQueueItem != null)
				state.Status = QuizStatus.IsCheckingByInstructor;
			
			/* For manually checked quizzes show last attempt's answers until ?attempt=true is defined in query string */
			if (slide.ManualChecking && manualQuizCheckQueueItem == null && state.Status == QuizStatus.ReadyToSend && state.UsedAttemptsCount > 0 && !attempt)
				state.Status = QuizStatus.Sent;
			
			/* We also want to show user's answer if user sent answers just now */
			if (state.Status == QuizStatus.ReadyToSend && send.HasValue)
				state.Status = QuizStatus.Sent;

			var userAnswers = userQuizzesRepo.GetAnswersForShowingOnSlide(courseId, slide, userId, manualQuizCheckQueueItem?.Submission);
			var canUserFillQuiz = CanUserFillQuiz(state.Status);

			var questionAnswersFrequency = new DefaultDictionary<string, DefaultDictionary<string, int>>();
			if (User.HasAccessFor(courseId, CourseRole.CourseAdmin))
			{
				questionAnswersFrequency = slide.Blocks.OfType<ChoiceBlock>().ToDictionary(
					block => block.Id,
					block => userQuizzesRepo.GetAnswersFrequencyForChoiceBlock(courseId, slide.Id, block.Id).ToDefaultDictionary()
				).ToDefaultDictionary();
			}

			var model = new QuizModel
			{
				Course = course,
				Slide = slide,
				QuizState = state,
				MaxAttemptsCount = maxAttemptsCount,
				UserScores = userScores,
				AnswersToQuizzes = userAnswers,
				IsLti = isLti,
				Checking = manualQuizCheckQueueItem,
				ManualCheckingsLeft = manualQuizCheckQueueItem != null ? GetManualCheckingsCountInQueue(course, slide) : 0,
				CanUserFillQuiz = canUserFillQuiz,
				GroupsIds = Request.GetMultipleValuesFromQueryString("group"),
				QuestionAnswersFrequency = questionAnswersFrequency,
			};

			return PartialView(model);
		}

		private int GetManualCheckingsCountInQueue(ICourse course, QuizSlide slide)
		{
			var groupsIds = Request.GetMultipleValuesFromQueryString("group");
			return ControllerUtils.GetManualCheckingsCountInQueue(slideCheckingsRepo, groupsRepo, User, course.Id, slide, groupsIds);
		}

		[HttpPost]
		public async Task<ActionResult> SubmitQuiz(string courseId, Guid slideId, string answer, bool isLti)
		{
			metricSender.SendCount("quiz.submit");
			if (isLti)
				metricSender.SendCount("quiz.submit.lti");
			metricSender.SendCount($"quiz.submit.{courseId}");
			metricSender.SendCount($"quiz.submit.{courseId}.{slideId}");

			var course = courseManager.GetCourse(courseId);
			var slide = course.FindSlideById(slideId) as QuizSlide;
			if (slide == null)
				return new HttpNotFoundResult();

			var userId = User.Identity.GetUserId();
			var maxTriesCount = GetMaxAttemptsCount(courseId, slide);
			
			/* Not it's not important what user's score is, so just pass 0 */
			var state = GetQuizState(courseId, userId, slideId, 0, slide.MaxScore);
			if (!CanUserFillQuiz(state.Status))
				return new HttpStatusCodeResult(HttpStatusCode.OK, "Already answered");

			var attemptNumber = state.UsedAttemptsCount;
			metricSender.SendCount($"quiz.submit.try.{attemptNumber}");
			metricSender.SendCount($"quiz.submit.{courseId}.try.{attemptNumber}");
			metricSender.SendCount($"quiz.submit.{courseId}.{slideId}.try.{attemptNumber}");

			if (slide.ManualChecking && !groupsRepo.IsManualCheckingEnabledForUser(course, userId))
				return new HttpStatusCodeResult(HttpStatusCode.OK, "Manual checking is disabled for you");

			
			var answers = JsonConvert.DeserializeObject<List<QuizAnswer>>(answer).GroupBy(x => x.BlockId);
			var quizBlockWithTaskCount = slide.Blocks.Count(x => x is AbstractQuestionBlock);
			var allQuizInfos = new List<QuizInfoForDb>();
			foreach (var ans in answers)
			{
				var quizInfos = CreateQuizInfo(slide, ans);
				if (quizInfos != null)
					allQuizInfos.AddRange(quizInfos);
			}
			var blocksInAnswerCount = allQuizInfos.Select(x => x.BlockId).Distinct().Count();
			if (blocksInAnswerCount != quizBlockWithTaskCount)
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Has empty blocks");

			UserQuizSubmission submission;
			using (var transaction = db.Database.BeginTransaction())
			{
				submission = await userQuizzesRepo.AddSubmission(courseId, slideId, userId, DateTime.Now).ConfigureAwait(false);

				foreach (var quizInfoForDb in allQuizInfos)
					await userQuizzesRepo.AddUserQuizAnswer(
						submission.Id,
						quizInfoForDb.IsRightAnswer,
						quizInfoForDb.BlockId,
						quizInfoForDb.ItemId,
						quizInfoForDb.Text,
						quizInfoForDb.QuizBlockScore,
						quizInfoForDb.QuizBlockMaxScore
					).ConfigureAwait(false);

				transaction.Commit();
			}

			if (slide.ManualChecking)
			{
				/* If this quiz is already queued for checking for this user, remove waiting checkings */
				if (state.Status == QuizStatus.WaitsForManualChecking)
					await slideCheckingsRepo.RemoveWaitingManualCheckings<ManualQuizChecking>(courseId, slideId, userId).ConfigureAwait(false);
				
				await slideCheckingsRepo.AddManualQuizChecking(submission, courseId, slideId, userId).ConfigureAwait(false);
				await visitsRepo.MarkVisitsAsWithManualChecking(courseId, slideId, userId).ConfigureAwait(false);
			}
			/* Recalculate score for quiz if this attempt is allowed. Don't recalculate score if this attempt number is more then maxTriesCount */
			else if (attemptNumber < maxTriesCount)
			{
				var score = allQuizInfos
					.DistinctBy(forDb => forDb.BlockId)
					.Sum(forDb => forDb.QuizBlockScore);
				
				metricSender.SendCount($"quiz.submit.try.{attemptNumber}.score", score);
				metricSender.SendCount($"quiz.submit.{courseId}.try.{attemptNumber}.score", score);
				metricSender.SendCount($"quiz.submit.{courseId}.{slideId}.try.{attemptNumber}.score", score);
				metricSender.SendCount($"quiz.submit.score", score);
				metricSender.SendCount($"quiz.submit.{courseId}.score", score);
				metricSender.SendCount($"quiz.submit.{courseId}.{slideId}.score", score);

				if (score == slide.MaxScore)
				{
					metricSender.SendCount($"quiz.submit.try.{attemptNumber}.full_passed");
					metricSender.SendCount($"quiz.submit.{courseId}.try.{attemptNumber}.full_passed");
					metricSender.SendCount($"quiz.submit.{courseId}.{slideId}.try.{attemptNumber}.full_passed");
					metricSender.SendCount($"quiz.submit.full_passed");
					metricSender.SendCount($"quiz.submit.{courseId}.full_passed");
					metricSender.SendCount($"quiz.submit.{courseId}.{slideId}.full_passed");
				}
				
				await slideCheckingsRepo.AddAutomaticQuizChecking(submission, courseId, slideId, userId, score).ConfigureAwait(false);
				await visitsRepo.UpdateScoreForVisit(courseId, slideId, slide.MaxScore, userId).ConfigureAwait(false);
				if (isLti)
					LtiUtils.SubmitScore(courseId, slide, userId);
			}

			return Json(new
			{
				url = Url.RouteUrl("Course.SlideById", new { courseId = courseId, slideId = slide.Url, send = 1}) 
			});
		}

		private async Task NotifyAboutManualQuizChecking(ManualQuizChecking checking)
		{
			var notification = new PassedManualQuizCheckingNotification
			{
				Checking = checking,
			};
			await notificationsRepo.AddNotification(checking.CourseId, notification, User.Identity.GetUserId()).ConfigureAwait(false);
		}

		[HttpPost]
		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		public async Task<ActionResult> ScoreQuiz(int id, string nextUrl, string errorUrl = "")
		{
			metricSender.SendCount("quiz.manual_score");

			if (string.IsNullOrEmpty(errorUrl))
				errorUrl = nextUrl;

			using (var transaction = db.Database.BeginTransaction())
			{
				var checking = slideCheckingsRepo.FindManualCheckingById<ManualQuizChecking>(id);

				var course = courseManager.GetCourse(checking.CourseId);
				var unit = course.FindUnitBySlideId(checking.SlideId);
				var slide = course.GetSlideById(checking.SlideId);

				metricSender.SendCount($"quiz.manual_score.{checking.CourseId}");
				metricSender.SendCount($"quiz.manual_score.{checking.CourseId}.{checking.SlideId}");

				var totalScore = 0;

				var quiz = course.FindSlideById(checking.SlideId);
				if (quiz == null)
					return Redirect(errorUrl + "Этого теста больше нет в курсе");
						
				foreach (var question in quiz.Blocks.OfType<AbstractQuestionBlock>())
				{
					var scoreFieldName = "quiz__score__" + question.Id;
					var scoreStr = Request.Form[scoreFieldName];
					/* Invalid form: score isn't integer */
					if (!int.TryParse(scoreStr, out var score))
						return Redirect(errorUrl + $"Неверное количество баллов в задании «{question.QuestionIndex}. {question.Text.TruncateWithEllipsis(50)}»");
					/* Invalid form: score isn't from range 0..MAX_SCORE */
					if (score < 0 || score > question.MaxScore)
						return Redirect(errorUrl + $"Неверное количество баллов в задании «{question.QuestionIndex}. {question.Text.TruncateWithEllipsis(50)}»: {score}");

					await userQuizzesRepo.SetScoreForQuizBlock(checking.Submission.Id, question.Id, score).ConfigureAwait(false);
					totalScore += score;
				}

				await slideCheckingsRepo.MarkManualCheckingAsChecked(checking, totalScore).ConfigureAwait(false);
				
				await visitsRepo.UpdateScoreForVisit(checking.CourseId, checking.SlideId, slide.MaxScore, checking.UserId).ConfigureAwait(false);
				transaction.Commit();

				metricSender.SendCount($"quiz.manual_score.score", totalScore);
				metricSender.SendCount($"quiz.manual_score.{checking.CourseId}.score", totalScore);
				metricSender.SendCount($"quiz.manual_score.{checking.CourseId}.{checking.SlideId}.score", totalScore);
				if (totalScore == quiz.MaxScore)
				{
					metricSender.SendCount($"quiz.manual_score.full_scored");
					metricSender.SendCount($"quiz.manual_score.{checking.CourseId}.full_scored");
					metricSender.SendCount($"quiz.manual_score.{checking.CourseId}.{checking.SlideId}.full_scored");
				}
				
				if(unit != null && unitsRepo.IsUnitVisibleForStudents(course, unit.Id))
					await NotifyAboutManualQuizChecking(checking).ConfigureAwait(false);
			}

			return Redirect(nextUrl);
		}

		private IEnumerable<QuizInfoForDb> CreateQuizInfo(QuizSlide slide, IGrouping<string, QuizAnswer> answer)
		{
			var block = slide.FindBlockById(answer.Key);
			if (block is FillInBlock)
				return CreateQuizInfoForDb(block as FillInBlock, answer.First().Text);
			if (block is ChoiceBlock)
				return CreateQuizInfoForDb(block as ChoiceBlock, answer);
			if (block is OrderingBlock)
				return CreateQuizInfoForDb(block as OrderingBlock, answer);
			if (block is MatchingBlock)
				return CreateQuizInfoForDb(block as MatchingBlock, answer);
			if (block is IsTrueBlock)
				return CreateQuizInfoForDb(block as IsTrueBlock, answer);
			return null;
		}

		private IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(IsTrueBlock isTrueBlock, IGrouping<string, QuizAnswer> data)
		{
			var isTrue = isTrueBlock.IsRight(data.First().ItemId);
			var blockScore = isTrue ? isTrueBlock.MaxScore : 0;
			return new List<QuizInfoForDb>
			{
				new QuizInfoForDb
				{
					BlockId = isTrueBlock.Id,
					ItemId = null,
					IsRightAnswer = isTrue,
					Text = data.First().ItemId,
					BlockType = typeof(IsTrueBlock),
					QuizBlockScore = blockScore,
					QuizBlockMaxScore = isTrueBlock.MaxScore
				}
			};
		}

		private IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(ChoiceBlock choiceBlock, IGrouping<string, QuizAnswer> answers)
		{
			int blockScore;
			if (!choiceBlock.Multiple)
			{
				var answerItemId = answers.First().ItemId;
				var isCorrect = choiceBlock.Items.First(x => x.Id == answerItemId).IsCorrect.IsTrueOrMaybe();
				blockScore = isCorrect ? choiceBlock.MaxScore : 0;
				return new List<QuizInfoForDb>
				{
					new QuizInfoForDb
					{
						BlockId = choiceBlock.Id,
						ItemId = answerItemId,
						IsRightAnswer = isCorrect,
						Text = null,
						BlockType = typeof(ChoiceBlock),
						QuizBlockScore = blockScore,
						QuizBlockMaxScore = choiceBlock.MaxScore
					}
				};
			}
			var ans = answers.Select(x => x.ItemId).ToList()
				.Select(x => new QuizInfoForDb
				{
					BlockId = choiceBlock.Id,
					IsRightAnswer = choiceBlock.Items.Where(y => y.IsCorrect.IsTrueOrMaybe()).Any(y => y.Id == x),
					ItemId = x,
					Text = null,
					BlockType = typeof(ChoiceBlock),
					QuizBlockScore = 0,
					QuizBlockMaxScore = choiceBlock.MaxScore
				}).ToList();
			
			var mistakesCount = GetChoiceBlockMistakesCount(choiceBlock, ans);
			var isRightQuizBlock = mistakesCount.HasNotMoreThatAllowed(choiceBlock.AllowedMistakesCount);
			
			blockScore = isRightQuizBlock ? choiceBlock.MaxScore : 0;
			foreach (var info in ans)
				info.QuizBlockScore = blockScore;
			return ans;
		}

		private MistakesCount GetChoiceBlockMistakesCount(ChoiceBlock choiceBlock, List<QuizInfoForDb> ans)
		{
			var checkedUnnecessary = ans.Count(x => !x.IsRightAnswer);
			
			var totallyTrueItemIds = choiceBlock.Items.Where(x => x.IsCorrect == ChoiceItemCorrectness.True).Select(x => x.Id);
			var userItemIds = ans.Select(y => y.ItemId).ToImmutableHashSet();
			var notCheckedNecessary = totallyTrueItemIds.Count(x => ! userItemIds.Contains(x));
			
			return new MistakesCount(checkedUnnecessary, notCheckedNecessary);
		}

		private IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(OrderingBlock orderingBlock, IGrouping<string, QuizAnswer> answers)
		{
			var ans = answers.Select(x => x.ItemId).ToList()
				.Select(x => new QuizInfoForDb
				{
					BlockId = orderingBlock.Id,
					IsRightAnswer = true,
					ItemId = x,
					Text = null,
					BlockType = typeof(OrderingBlock),
					QuizBlockScore = 0,
					QuizBlockMaxScore = orderingBlock.MaxScore
				}).ToList();

			var isRightQuizBlock = answers.Count() == orderingBlock.Items.Length &&
									answers.Zip(orderingBlock.Items, (answer, item) => answer.ItemId == item.GetHash()).All(x => x);
			var blockScore = isRightQuizBlock ? orderingBlock.MaxScore : 0;
			foreach (var info in ans)
				info.QuizBlockScore = blockScore;

			return ans;
		}

		private IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(MatchingBlock matchingBlock, IGrouping<string, QuizAnswer> answers)
		{
			var ans = answers.ToList()
				.Select(x => new QuizInfoForDb
				{
					BlockId = matchingBlock.Id,
					IsRightAnswer = matchingBlock.Matches.FirstOrDefault(m => m.GetHashForFixedItem() == x.ItemId)?.
										GetHashForMovableItem() == x.Text,
					ItemId = x.ItemId,
					Text = x.Text,
					BlockType = typeof(MatchingBlock),
					QuizBlockScore = 0,
					QuizBlockMaxScore = matchingBlock.MaxScore
				}).ToList();

			var isRightQuizBlock = ans.All(x => x.IsRightAnswer);
			var blockScore = isRightQuizBlock ? matchingBlock.MaxScore : 0;
			foreach (var info in ans)
				info.QuizBlockScore = blockScore;

			return ans;
		}

		private IEnumerable<QuizInfoForDb> CreateQuizInfoForDb(FillInBlock fillInBlock, string data)
		{
			if (data.Length > MaxFillInBlockSize)
				data = data.Substring(0, MaxFillInBlockSize);
			var isRightAnswer = true;
			if (fillInBlock.Regexes != null)
				isRightAnswer = fillInBlock.Regexes.Any(regex => regex.Regex.IsMatch(data));
			var blockScore = isRightAnswer ? fillInBlock.MaxScore : 0;
			return new List<QuizInfoForDb>
			{
				new QuizInfoForDb
				{
					BlockId = fillInBlock.Id,
					ItemId = null,
					IsRightAnswer = isRightAnswer,
					Text = data,
					BlockType = typeof(FillInBlock),
					QuizBlockScore = blockScore,
					QuizBlockMaxScore = fillInBlock.MaxScore
				}
			};
		}

		[HttpPost]
		public async Task<ActionResult> RestartQuiz(string courseId, Guid slideId, bool isLti)
		{
			var slide = courseManager.GetCourse(courseId).GetSlideById(slideId);
			if (slide is QuizSlide)
			{
				var userId = User.Identity.GetUserId();
				var usedAttemptsCount = userQuizzesRepo.GetUsedAttemptsCount(courseId, userId, slideId);
				var maxTriesCount = GetMaxAttemptsCount(courseId, slide as QuizSlide);
				var isQuizScoredMaximum = userQuizzesRepo.IsQuizScoredMaximum(courseId, slideId, userId);
				if (usedAttemptsCount < maxTriesCount && !isQuizScoredMaximum)
				{
					await visitsRepo.UpdateScoreForVisit(courseId, slideId, slide.MaxScore, userId).ConfigureAwait(false);
					if (isLti)
						LtiUtils.SubmitScore(courseId, slide, userId);
				}
			}
			var model = new { courseId, slideId = slide.Id, isLti, attempt = true };
			if (isLti)
				return RedirectToAction("LtiSlide", "Course", model);
			return RedirectToAction("SlideById", "Course", model);
		}

		private QuizModel GuestQuiz(Course course, QuizSlide slide)
		{
			return new QuizModel
			{
				Course = course,
				Slide = slide,
				IsGuest = true,
				QuizState = new QuizState(QuizStatus.ReadyToSend, 0, 0, slide.MaxScore),
			};
		}

		private int GetMaxAttemptsCount(string courseId, QuizSlide quizSlide)
		{
			if (User.HasAccessFor(courseId, CourseRole.Tester))
				return InfinityTriesCount;

			if (quizSlide == null)
				return defaultMaxTriesCount;
			
			return quizSlide.MaxTriesCount;
		}

		private Dictionary<string, int> GetUserScoresForBlocks(string courseId, string userId, Guid slideId, UserQuizSubmission submission)
		{
			return userQuizzesRepo.GetUserScores(courseId, slideId, userId, submission);
		}

		private QuizState GetQuizState(string courseId, string userId, Guid slideId, int userScore, int maxScore, UserQuizSubmission submission=null)
		{
			log.Info($"Ищу статус прохождения теста {courseId}:{slideId} для пользователя {userId}");
			
			var lastSubmission = userQuizzesRepo.FindLastUserSubmission(courseId, slideId, userId);
			
			var manualChecking = submission?.ManualChecking ?? lastSubmission?.ManualChecking;
			if (manualChecking != null)
			{
				/* For manually checked quizzes attempts are counting by manual checkings, not by user quiz submissions
				   (because user can resend quiz before instructor checked and score it) */
				var manualCheckingCount = GetManualCheckingCount(courseId, userId, slideId, submission);
				
				log.Info($"Статус прохождения теста {courseId}:{slideId} для пользователя {userId}: есть ручная проверка №{manualChecking.Id}, проверяется ли сейчас: {manualChecking.IsLocked}");
				if (manualChecking.IsChecked)
					return new QuizState(QuizStatus.ReadyToSend, manualCheckingCount, userScore, maxScore);
				return new QuizState(manualChecking.IsLocked ? QuizStatus.IsCheckingByInstructor : QuizStatus.WaitsForManualChecking, manualCheckingCount, userScore, maxScore);
			}
			
			var usedAttemptsCount = userQuizzesRepo.GetUsedAttemptsCount(courseId, userId, slideId);
			return new QuizState(QuizStatus.ReadyToSend, usedAttemptsCount, userScore, maxScore);
		}

		private int GetManualCheckingCount(string courseId, string userId, Guid slideId, UserQuizSubmission beforeSubmissions=null)
		{
			var queue = slideCheckingsRepo.GetManualCheckingQueue<ManualQuizChecking>(new ManualCheckingQueueFilterOptions
			{
				CourseId = courseId,
				SlidesIds = new[] { slideId },
				UserIds = new List<string> { userId },
				OnlyChecked = null, // For calculating not checked submissions as well as checked ones
			}).ToList();

			if (beforeSubmissions != null)
				queue = queue.Where(s => s.Timestamp < beforeSubmissions.Timestamp).ToList();
					
			return queue.Count();
		}
	}
}