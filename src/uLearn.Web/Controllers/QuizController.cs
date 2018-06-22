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
using uLearn.Extensions;
using uLearn.Quizes;
using uLearn.Web.Extensions;
using uLearn.Web.FilterAttributes;
using uLearn.Web.LTI;
using uLearn.Web.Models;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace uLearn.Web.Controllers
{
	[ULearnAuthorize]
	public class QuizController : Controller
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(QuizController));

		private const int defaultMaxTriesCount = 2;
		public const int InfinityTriesCount = int.MaxValue - 1;
		public const int MaxFillinblockSize = 1024;

		private readonly ULearnDb db = new ULearnDb();
		private readonly CourseManager courseManager = WebCourseManager.Instance;
		protected readonly MetricSender metricSender;

		private readonly UserQuizzesRepo userQuizzesRepo;
		private readonly VisitsRepo visitsRepo;
		private readonly QuizzesRepo quizzesRepo;
		private readonly GroupsRepo groupsRepo;
		private readonly SlideCheckingsRepo slideCheckingsRepo;
		private readonly NotificationsRepo notificationsRepo;

		public QuizController()
		{
			metricSender = new MetricSender("web");

			userQuizzesRepo = new UserQuizzesRepo(db);
			visitsRepo = new VisitsRepo(db);
			quizzesRepo = new QuizzesRepo(db);
			groupsRepo = new GroupsRepo(db, courseManager);
			slideCheckingsRepo = new SlideCheckingsRepo(db);
			notificationsRepo = new NotificationsRepo(db);
		}

		[UsedImplicitly]
		private class QuizAnswer
		{
			public readonly string QuizType;
			public readonly string QuizId;
			public readonly string ItemId;
			public readonly string Text;

			public QuizAnswer(string type, string quizId, string itemId, string text)
			{
				QuizType = type;
				QuizId = quizId;
				ItemId = itemId;
				Text = text;
			}
		}

		private class QuizInfoForDb
		{
			public string QuizId;
			public string ItemId;
			public string Text;
			public bool IsRightAnswer;
			public Type QuizType;
			public int QuizBlockScore;
			public int QuizBlockMaxScore;
		}

		public bool CanUserFillQuiz(QuizState state)
		{
			return state == QuizState.NotPassed || state == QuizState.WaitForCheck;
		}

		[AllowAnonymous]
		public ActionResult Quiz(QuizSlide slide, string courseId, string userId, bool isGuest, bool isLti = false, ManualQuizChecking manualQuizCheckQueueItem = null, int? send = null)
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
			var maxTriesCount = GetMaxTriesCount(courseId, slide);
			var state = GetQuizState(courseId, userId, slideId);
			var quizState = state.Item1;
			var tryNumber = state.Item2;
			var resultsForQuizes = GetResultForQuizes(courseId, userId, slideId, state.Item1);

			log.Info($"Создаю тест для пользователя {userId} в слайде {courseId}:{slide.Id}, isLti = {isLti}");

			var quizVersion = quizzesRepo.GetLastQuizVersion(courseId, slideId);
			if (quizState != QuizState.NotPassed)
				quizVersion = userQuizzesRepo.FindQuizVersionFromUsersAnswer(courseId, slideId, userId);

			/* If we haven't quiz version in database, create it */
			if (quizVersion == null)
				quizVersion = quizzesRepo.AddQuizVersionIfNeeded(courseId, slide);

			/* Restore quiz slide from version stored in the database */
			var quiz = quizVersion.GetRestoredQuiz(course, course.FindUnitBySlideId(slide.Id));
			slide = new QuizSlide(slide.Info, quiz);
			
			if (quizState == QuizState.Subtotal)
			{
				var score = resultsForQuizes?.AsEnumerable().Sum(res => res.Value) ?? 0;
				/* QuizState.Subtotal is partially obsolete. If user fully solved quiz, then show answers. Else show empty quiz for the new try... */
				if (score == quiz.MaxScore)
					quizState = QuizState.Total;
				/* ... and show last try's answers only if argument `send` has been passed in query */
				else if (!send.HasValue)
				{
					quizState = QuizState.NotPassed;
					/* ... if we will show answers from last try then drop quiz */
					userQuizzesRepo.DropQuiz(userId, slideId);
				}
			}			

			var userAnswers = userQuizzesRepo.GetAnswersForShowOnSlide(courseId, slide, userId);
			var canUserFillQuiz = CanUserFillQuiz(quizState);

			var questionAnswersFrequency = new DefaultDictionary<string, DefaultDictionary<string, int>>();
			if (User.HasAccessFor(courseId, CourseRole.CourseAdmin))
			{
				questionAnswersFrequency = quiz.Blocks.OfType<ChoiceBlock>().ToDictionary(
					block => block.Id,
					block => userQuizzesRepo.GetAnswersFrequencyForChoiceBlock(courseId, slide.Id, block.Id).ToDefaultDictionary()
				).ToDefaultDictionary();
			}

			var model = new QuizModel
			{
				Course = course,
				Slide = slide,
				QuizState = quizState,
				TryNumber = tryNumber,
				MaxTriesCount = maxTriesCount,
				ResultsForQuizes = resultsForQuizes,
				AnswersToQuizes = userAnswers,
				IsLti = isLti,
				ManualQuizCheckQueueItem = manualQuizCheckQueueItem,
				CanUserFillQuiz = canUserFillQuiz,
				GroupsIds = Request.GetMultipleValuesFromQueryString("group"),
				QuestionAnswersFrequency = questionAnswersFrequency,
			};

			return PartialView(model);
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
			var maxTriesCount = GetMaxTriesCount(courseId, slide);
			var quizState = GetQuizState(courseId, userId, slideId);
			if (!CanUserFillQuiz(quizState.Item1))
				return new HttpStatusCodeResult(HttpStatusCode.OK, "Already answered");

			var tryIndex = quizState.Item2;
			metricSender.SendCount($"quiz.submit.try.{tryIndex}");
			metricSender.SendCount($"quiz.submit.{courseId}.try.{tryIndex}");
			metricSender.SendCount($"quiz.submit.{courseId}.{slideId}.try.{tryIndex}");

			if (slide.ManualChecking && !groupsRepo.IsManualCheckingEnabledForUser(course, userId))
				return new HttpStatusCodeResult(HttpStatusCode.OK, "Manual checking is disabled for you");

			var time = DateTime.Now;
			var answers = JsonConvert.DeserializeObject<List<QuizAnswer>>(answer).GroupBy(x => x.QuizId);
			var quizBlockWithTaskCount = slide.Blocks.Count(x => x is AbstractQuestionBlock);
			var allQuizInfos = new List<QuizInfoForDb>();
			foreach (var ans in answers)
			{
				var quizInfos = CreateQuizInfo(slide, ans);
				if (quizInfos != null)
					allQuizInfos.AddRange(quizInfos);
			}
			var blocksInAnswerCount = allQuizInfos.Select(x => x.QuizId).Distinct().Count();
			if (blocksInAnswerCount != quizBlockWithTaskCount)
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Has empty blocks");

			using (var transaction = db.Database.BeginTransaction())
			{
				await userQuizzesRepo.RemoveUserQuizzes(courseId, slideId, userId);

				foreach (var quizInfoForDb in allQuizInfos)
					await userQuizzesRepo.AddUserQuiz(courseId, quizInfoForDb.IsRightAnswer, quizInfoForDb.ItemId, quizInfoForDb.QuizId,
						slideId, quizInfoForDb.Text, userId, time, quizInfoForDb.QuizBlockScore, quizInfoForDb.QuizBlockMaxScore);

				transaction.Commit();
			}

			if (slide.ManualChecking)
			{
				/* If this quiz is already queued for checking for this user, don't add it to queue again */
				if (quizState.Item1 != QuizState.WaitForCheck)
				{
					await slideCheckingsRepo.AddQuizAttemptForManualChecking(courseId, slideId, userId);
					await visitsRepo.MarkVisitsAsWithManualChecking(slideId, userId);
				}
			}
			/* Recalculate score for quiz if this attempt is allowed. Don't recalculate score if this attempt is more then maxTriesCount */
			else if (tryIndex < maxTriesCount)
			{
				var score = allQuizInfos
					.DistinctBy(forDb => forDb.QuizId)
					.Sum(forDb => forDb.QuizBlockScore);
				
				metricSender.SendCount($"quiz.submit.try.{tryIndex}.score", score);
				metricSender.SendCount($"quiz.submit.{courseId}.try.{tryIndex}.score", score);
				metricSender.SendCount($"quiz.submit.{courseId}.{slideId}.try.{tryIndex}.score", score);
				metricSender.SendCount($"quiz.submit.score", score);
				metricSender.SendCount($"quiz.submit.{courseId}.score", score);
				metricSender.SendCount($"quiz.submit.{courseId}.{slideId}.score", score);

				if (score == slide.MaxScore)
				{
					metricSender.SendCount($"quiz.submit.try.{tryIndex}.full_passed");
					metricSender.SendCount($"quiz.submit.{courseId}.try.{tryIndex}.full_passed");
					metricSender.SendCount($"quiz.submit.{courseId}.{slideId}.try.{tryIndex}.full_passed");
					metricSender.SendCount($"quiz.submit.full_passed");
					metricSender.SendCount($"quiz.submit.{courseId}.full_passed");
					metricSender.SendCount($"quiz.submit.{courseId}.{slideId}.full_passed");
				}
				
				await slideCheckingsRepo.AddQuizAttemptWithAutomaticChecking(courseId, slideId, userId, score);
				await visitsRepo.UpdateScoreForVisit(courseId, slideId, userId);
				if (isLti)
					LtiUtils.SubmitScore(slide, userId);
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
			await notificationsRepo.AddNotification(checking.CourseId, notification, User.Identity.GetUserId());
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

				if (checking.IsChecked)
					return Redirect(errorUrl + "Эта работа уже была проверена");

				if (!checking.IsLockedBy(User.Identity))
					return Redirect(errorUrl + "Эта работа проверяется другим инструктором");

				var course = courseManager.GetCourse(checking.CourseId);
				var unit = course.FindUnitBySlideId(checking.SlideId);

				metricSender.SendCount($"quiz.manual_score.{checking.CourseId}");
				metricSender.SendCount($"quiz.manual_score.{checking.CourseId}.{checking.SlideId}");

				var answers = userQuizzesRepo.GetAnswersForUser(checking.SlideId, checking.UserId);

				QuizVersion quizVersion;
				/* If there is no user's answers for quiz, get the latest quiz version */
				if (answers.Count == 0)
					quizVersion = quizzesRepo.GetLastQuizVersion(checking.CourseId, checking.SlideId);
				else
				{
					var firstAnswer = answers.FirstOrDefault().Value.FirstOrDefault();
					quizVersion = firstAnswer != null
						? firstAnswer.QuizVersion
						: quizzesRepo.GetFirstQuizVersion(checking.CourseId, checking.SlideId);
				}

				var totalScore = 0;

				var quiz = quizVersion.GetRestoredQuiz(course, unit);
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

					await userQuizzesRepo.SetScoreForQuizBlock(checking.UserId, checking.SlideId, question.Id, score);
					totalScore += score;
				}

				await slideCheckingsRepo.MarkManualCheckingAsChecked(checking, totalScore);
				await visitsRepo.UpdateScoreForVisit(checking.CourseId, checking.SlideId, checking.UserId);
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

				await NotifyAboutManualQuizChecking(checking);
			}

			return Redirect(nextUrl);
		}

		private IEnumerable<QuizInfoForDb> CreateQuizInfo(QuizSlide slide, IGrouping<string, QuizAnswer> answer)
		{
			var block = slide.GetBlockById(answer.Key);
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
					QuizId = isTrueBlock.Id,
					ItemId = null,
					IsRightAnswer = isTrue,
					Text = data.First().ItemId,
					QuizType = typeof(IsTrueBlock),
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
						QuizId = choiceBlock.Id,
						ItemId = answerItemId,
						IsRightAnswer = isCorrect,
						Text = null,
						QuizType = typeof(ChoiceBlock),
						QuizBlockScore = blockScore,
						QuizBlockMaxScore = choiceBlock.MaxScore
					}
				};
			}
			var ans = answers.Select(x => x.ItemId).ToList()
				.Select(x => new QuizInfoForDb
				{
					QuizId = choiceBlock.Id,
					IsRightAnswer = choiceBlock.Items.Where(y => y.IsCorrect.IsTrueOrMaybe()).Any(y => y.Id == x),
					ItemId = x,
					Text = null,
					QuizType = typeof(ChoiceBlock),
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
					QuizId = orderingBlock.Id,
					IsRightAnswer = true,
					ItemId = x,
					Text = null,
					QuizType = typeof(OrderingBlock),
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
					QuizId = matchingBlock.Id,
					IsRightAnswer = matchingBlock.Matches.FirstOrDefault(m => m.GetHashForFixedItem() == x.ItemId)?.
										GetHashForMovableItem() == x.Text,
					ItemId = x.ItemId,
					Text = x.Text,
					QuizType = typeof(MatchingBlock),
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
			if (data.Length > MaxFillinblockSize)
				data = data.Substring(0, MaxFillinblockSize);
			var isRightAnswer = true;
			if (fillInBlock.Regexes != null)
				isRightAnswer = fillInBlock.Regexes.Any(regex => regex.Regex.IsMatch(data));
			var blockScore = isRightAnswer ? fillInBlock.MaxScore : 0;
			return new List<QuizInfoForDb>
			{
				new QuizInfoForDb
				{
					QuizId = fillInBlock.Id,
					ItemId = null,
					IsRightAnswer = isRightAnswer,
					Text = data,
					QuizType = typeof(FillInBlock),
					QuizBlockScore = blockScore,
					QuizBlockMaxScore = fillInBlock.MaxScore
				}
			};
		}

		[HttpPost]
		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		public async Task<ActionResult> MergeQuizVersions(string courseId, Guid slideId, int quizVersionId, int mergeWithQuizVersionId)
		{
			using (var transcation = db.Database.BeginTransaction())
			{
				var quizVersion = db.QuizVersions.FirstOrDefault(x => x.CourseId == courseId && x.SlideId == slideId && x.Id == quizVersionId);
				var mergeWithQuizVersion = db.QuizVersions.FirstOrDefault(x => x.CourseId == courseId && x.SlideId == slideId && x.Id == mergeWithQuizVersionId);
				if (quizVersion == null || mergeWithQuizVersion == null)
					return HttpNotFound();

				foreach (var userQuiz in db.UserQuizzes.Where(x => x.QuizVersionId == mergeWithQuizVersionId))
					userQuiz.QuizVersionId = quizVersionId;

				quizVersion.NormalizedXml = mergeWithQuizVersion.NormalizedXml;

				db.QuizVersions.Remove(mergeWithQuizVersion);

				await db.SaveChangesAsync();

				transcation.Commit();
			}

			return RedirectToAction("SlideById", "Course", new { courseId, slideId });
		}

		[HttpGet]
		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		public ActionResult Analytics(string courseId, Guid slideId, DateTime periodStart)
		{
			var course = courseManager.GetCourse(courseId);
			var unit = course.FindUnitBySlideId(slideId);
			var quizSlide = (QuizSlide)course.GetSlideById(slideId);
			
			var quizVersions = quizzesRepo.GetQuizVersions(courseId, quizSlide.Id).ToList();
			var dict = new SortedDictionary<string, List<QuizAnswerInfo>>();
			var passes = db.UserQuizzes
				.Where(q => quizSlide.Id == q.SlideId && !q.isDropped && periodStart <= q.Timestamp)
				.GroupBy(q => q.UserId)
				.Join(db.Users, g => g.Key, user => user.Id, (g, user) => new { UserId = user.Id, user.UserName, UserQuizzes = g.ToList(), QuizVersion = g.FirstOrDefault().QuizVersion })
				.ToList();
			
			foreach (var pass in passes)
			{
				var slide = quizSlide;
				if (pass.QuizVersion != null)
					slide = new QuizSlide(quizSlide.Info, pass.QuizVersion.GetRestoredQuiz(course, unit));
				dict[pass.UserName] = GetUserQuizAnswers(slide, pass.UserQuizzes).ToList();
			}
			
			var userIds = passes.Select(p => p.UserId).Distinct().ToList();
			var userNameById = passes.ToDictionary(p => p.UserId, p => p.UserName);
			var groups = groupsRepo.GetUsersGroupsNamesAsStrings(courseId, userIds, User).ToDictionary(kv => userNameById[kv.Key], kv => kv.Value);
			var rightAnswersCount = dict.Values
				.SelectMany(list => list
					.Where(info => info.Score == info.MaxScore))
				.GroupBy(arg => arg.Id)
				.ToDictionary(grouping => grouping.Key, grouping => grouping.Count());

			var usersByQuizVersion = passes.GroupBy(p => p.QuizVersion?.Id).ToDictionary(g => g.Key, g => g.Select(u => u.UserName).ToList());
			var usersWaitsForManualCheck = slideCheckingsRepo.GetManualCheckingQueue<ManualQuizChecking>(
				new ManualCheckingQueueFilterOptions { CourseId = courseId, SlidesIds = new List<Guid> { quizSlide.Id } }
			).ToList().Select(i => i.User.UserName).ToImmutableHashSet();

			return PartialView(new QuizAnalyticsModel
			{
				Course = course,
				Unit = course.FindUnitBySlideId(quizSlide.Id),
				SlideId = quizSlide.Id,

				UserAnswers = dict,
				QuizVersions = quizVersions,
				UsersByQuizVersion = usersByQuizVersion,
				RightAnswersCount = rightAnswersCount,
				GroupByUser = groups,
				UsersWaitsForManualCheck = usersWaitsForManualCheck,
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
				else if (block is OrderingBlock)
					yield return GetOrderingBlockAnswerInfo(answers, (OrderingBlock)block, block.QuestionIndex);
				else if (block is MatchingBlock)
					yield return GetMatchingBlockAnswerInfo(answers, (MatchingBlock)block, block.QuestionIndex);
		}

		private static QuizAnswerInfo GetFillInBlockAnswerInfo(IReadOnlyDictionary<string, List<UserQuiz>> answers, string quizId, int questionIndex)
		{
			UserQuiz answer = null;
			if (answers.ContainsKey(quizId))
				answer = answers[quizId].FirstOrDefault();
			return new FillInBlockAnswerInfo
			{
				Answer = answer?.Text,
				IsRight = answer != null && answer.IsRightAnswer,
				Score = answer?.QuizBlockScore ?? 0,
				MaxScore = answer?.QuizBlockMaxScore ?? 0,
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
			var score = 0;
			var maxScore = 0;
			foreach (var quizItem in answer.Where(quizItem => ans.ContainsKey(quizItem.ItemId)))
			{
				isRight = quizItem.IsQuizBlockScoredMaximum;
				ans[quizItem.ItemId] = true;
				score = quizItem.QuizBlockScore;
				maxScore = quizItem.QuizBlockMaxScore;
			}

			return new ChoiceBlockAnswerInfo
			{
				AnswersId = ans,
				Id = questionIndex.ToString(),
				RealyRightAnswer = new HashSet<string>(block.Items.Where(x => x.IsCorrect.IsTrueOrMaybe()).Select(x => x.Id)),
				Score = score,
				MaxScore = maxScore,
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
				IsRight = answer != null && answer.IsRightAnswer,
				Score = answer?.QuizBlockScore ?? 0,
				MaxScore = answer?.QuizBlockMaxScore ?? 0,
			};
		}

		private static QuizAnswerInfo GetOrderingBlockAnswerInfo(IReadOnlyDictionary<string, List<UserQuiz>> answers, OrderingBlock block, int questionIndex)
		{
			IEnumerable<UserQuiz> userAnswers = new List<UserQuiz>();
			UserQuiz firstAnswer = null;
			if (answers.ContainsKey(block.Id))
			{
				userAnswers = answers[block.Id].Where(q => q.ItemId != null);
				firstAnswer = userAnswers.FirstOrDefault();
			}

			var answersPositions = userAnswers.Select(
				userAnswer => block.Items.FindIndex(i => i.GetHash() == userAnswer.ItemId)
			).ToList();

			return new OrderingBlockAnswerInfo
			{
				Id = questionIndex.ToString(),
				AnswersPositions = answersPositions,
				Score = firstAnswer?.QuizBlockScore ?? 0,
				MaxScore = firstAnswer?.QuizBlockMaxScore ?? 0,
			};
		}

		private static QuizAnswerInfo GetMatchingBlockAnswerInfo(IReadOnlyDictionary<string, List<UserQuiz>> answers, MatchingBlock block, int questionIndex)
		{
			IEnumerable<UserQuiz> userAnswers = new List<UserQuiz>();
			UserQuiz firstAnswer = null;
			if (answers.ContainsKey(block.Id))
			{
				userAnswers = answers[block.Id].Where(q => q.ItemId != null);
				firstAnswer = userAnswers.FirstOrDefault();
			}

			var isRightMatches = new List<bool>();
			foreach (var match in block.Matches)
			{
				var userAnswer = userAnswers.FirstOrDefault(a => a.ItemId == match.GetHashForFixedItem());
				isRightMatches.Add(userAnswer != null && userAnswer.IsRightAnswer);
			}

			return new MatchingBlockAnswerInfo
			{
				Id = questionIndex.ToString(),
				IsRightMatches = isRightMatches,
				Score = firstAnswer?.QuizBlockScore ?? 0,
				MaxScore = firstAnswer?.QuizBlockMaxScore ?? 0,
			};
		}

		[HttpPost]
		public async Task<ActionResult> DropQuiz(string courseId, Guid slideId, bool isLti)
		{
			var slide = courseManager.GetCourse(courseId).GetSlideById(slideId);
			if (slide is QuizSlide)
			{
				var userId = User.Identity.GetUserId();
				var userQuizDrops = userQuizzesRepo.GetQuizDropStates(courseId, userId, slideId).Count(b => b);
				var maxTriesCount = GetMaxTriesCount(courseId, slide as QuizSlide);
				var isQuizScoredMaximum = userQuizzesRepo.IsQuizScoredMaximum(courseId, userId, slideId);
				if (userQuizDrops + 1 < maxTriesCount && !isQuizScoredMaximum)
				{
					await userQuizzesRepo.DropQuizAsync(userId, slideId);
					await visitsRepo.UpdateScoreForVisit(courseId, slideId, userId);
					if (isLti)
						LtiUtils.SubmitScore(slide, userId);
				}
				else if ((userQuizDrops + 1 >= maxTriesCount || isQuizScoredMaximum) && !(slide as QuizSlide).ManualChecking)
				{
					/* Allow user to drop quiz after all tries are exceeded, but don't update score */
					await userQuizzesRepo.DropQuizAsync(userId, slideId);
				}
				
			}
			var model = new { courseId, slideId = slide.Id, isLti };
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
				QuizState = QuizState.NotPassed,
			};
		}

		private int GetMaxTriesCount(string courseId, QuizSlide quizSlide)
		{
			if (User.HasAccessFor(courseId, CourseRole.Tester))
				return InfinityTriesCount;

			if (quizSlide == null)
				return defaultMaxTriesCount;
			
			return quizSlide.MaxTriesCount;
		}

		private Dictionary<string, int> GetResultForQuizes(string courseId, string userId, Guid slideId, QuizState state)
		{
			return userQuizzesRepo.GetQuizBlocksTruth(courseId, userId, slideId);
		}

		private Tuple<QuizState, int> GetQuizState(string courseId, string userId, Guid slideId)
		{
			log.Info($"Ищу статус прохождения теста {courseId}:{slideId} для пользователя {userId}");
			var states = userQuizzesRepo.GetQuizDropStates(courseId, userId, slideId).ToList();

			var queueItem = userQuizzesRepo.FindManualQuizChecking(courseId, slideId, userId);
			if (queueItem != null)
			{
				log.Info($"Статус прохождения теста {courseId}:{slideId} для пользователя {userId}: есть ручная проверка №{queueItem.Id}, проверяется ли сейчас: {queueItem.IsLocked}");
				return Tuple.Create(queueItem.IsLocked ? QuizState.IsChecking : QuizState.WaitForCheck, states.Count);
			}

			if (states.Any(b => !b))
				return Tuple.Create(QuizState.Subtotal, states.Count);
			return Tuple.Create(QuizState.NotPassed, states.Count);
		}
	}
}