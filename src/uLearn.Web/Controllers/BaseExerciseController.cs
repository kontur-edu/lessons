using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.DataContexts;
using Database.Models;
using log4net;
using Metrics;
using uLearn.Web.Models;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.CSharp;
using Ulearn.Core.Telegram;

namespace uLearn.Web.Controllers
{
	public class BaseExerciseController : JsonDataContractController
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(BaseExerciseController));
		private readonly ErrorsBot errorsBot = new ErrorsBot();

		protected readonly ULearnDb db;
		protected readonly CourseManager courseManager;
		protected readonly MetricSender metricSender;

		protected readonly UserSolutionsRepo userSolutionsRepo;
		protected readonly SlideCheckingsRepo slideCheckingsRepo;
		protected readonly GroupsRepo groupsRepo;
		protected readonly VisitsRepo visitsRepo;
		protected readonly NotificationsRepo notificationsRepo;
		protected readonly UsersRepo usersRepo;
		protected readonly StyleErrorsRepo styleErrorsRepo;
		protected readonly UnitsRepo unitsRepo;
		
		private static readonly TimeSpan executionTimeout = TimeSpan.FromSeconds(45);
		
		public BaseExerciseController()
			: this(new ULearnDb(), WebCourseManager.Instance, new MetricSender(ApplicationConfiguration.Read<UlearnConfiguration>().GraphiteServiceName))
		{
		}

		public BaseExerciseController(ULearnDb db, CourseManager courseManager, MetricSender metricSender)
		{
			this.db = db;
			this.courseManager = courseManager;
			this.metricSender = metricSender;

			userSolutionsRepo = new UserSolutionsRepo(db, courseManager);
			slideCheckingsRepo = new SlideCheckingsRepo(db);
			groupsRepo = new GroupsRepo(db, courseManager);
			visitsRepo = new VisitsRepo(db);
			notificationsRepo = new NotificationsRepo(db);
			usersRepo = new UsersRepo(db);
			styleErrorsRepo = new StyleErrorsRepo(db);
			unitsRepo = new UnitsRepo(db);
		}

		private string GenerateSubmissionName(Slide exerciseSlide, string userName)
		{
			return $"{userName}: {exerciseSlide.Info.Unit.Title} - {exerciseSlide.Title}";
		}

		protected async Task<RunSolutionResult> CheckSolution(string courseId, ExerciseSlide exerciseSlide, string userCode, string userId, string userName, bool waitUntilChecked, bool saveSubmissionOnCompileErrors)
		{
			var exerciseMetricId = GetExerciseMetricId(courseId, exerciseSlide);
			metricSender.SendCount("exercise.try");
			metricSender.SendCount($"exercise.{courseId.ToLower(CultureInfo.InvariantCulture)}.try");
			metricSender.SendCount($"exercise.{exerciseMetricId}.try");

			var course = courseManager.GetCourse(courseId);
			var exerciseBlock = exerciseSlide.Exercise;
			var buildResult = exerciseBlock.BuildSolution(userCode);

			if (buildResult.HasErrors)
				metricSender.SendCount($"exercise.{exerciseMetricId}.CompilationError");
			if (buildResult.HasStyleErrors)
				metricSender.SendCount($"exercise.{exerciseMetricId}.StyleViolation");

			if (!saveSubmissionOnCompileErrors)
			{
				if (buildResult.HasErrors)
					return new RunSolutionResult { IsCompileError = true, ErrorMessage = buildResult.ErrorMessage, ExecutionServiceName = "uLearn" };
			}

			var compilationErrorMessage = buildResult.HasErrors ? buildResult.ErrorMessage : null;
			var dontRunSubmission = buildResult.HasErrors;
			var submissionLanguage = exerciseSlide.Exercise.Language.Value;
			var submission = await userSolutionsRepo.AddUserExerciseSubmission(
				courseId, exerciseSlide.Id,
				userCode, compilationErrorMessage, null,
				userId, "uLearn", GenerateSubmissionName(exerciseSlide, userName),
				submissionLanguage,
				dontRunSubmission ? AutomaticExerciseCheckingStatus.Done : AutomaticExerciseCheckingStatus.Waiting
			);

			if (buildResult.HasErrors)
				return new RunSolutionResult { IsCompileError = true, ErrorMessage = buildResult.ErrorMessage, SubmissionId = submission.Id, ExecutionServiceName = "uLearn" };

			try
			{
				if (submissionLanguage.HasAutomaticChecking())
					await userSolutionsRepo.RunAutomaticChecking(submission, executionTimeout, waitUntilChecked);
			}
			catch (SubmissionCheckingTimeout)
			{
				log.Error($"Не смог запустить проверку решения, никто не взял его на проверку за {executionTimeout.TotalSeconds} секунд.\nКурс «{course.Title}», слайд «{exerciseSlide.Title}» ({exerciseSlide.Id})");
				errorsBot.PostToChannel($"Не смог запустить проверку решения, никто не взял его на проверку за {executionTimeout.TotalSeconds} секунд.\nКурс «{course.Title}», слайд «{exerciseSlide.Title}» ({exerciseSlide.Id})\n\nhttps://ulearn.me/Sandbox");
				return new RunSolutionResult
				{
					IsCompillerFailure = true,
					ErrorMessage = "К сожалению, из-за большой нагрузки мы не смогли оперативно проверить ваше решение. " +
									"Мы попробуем проверить его позже, просто подождите и обновите страницу. ",
					ExecutionServiceName = "ulearn"
				};
			}

			if (!waitUntilChecked)
			{
				metricSender.SendCount($"exercise.{exerciseMetricId}.dont_wait_result");
				return new RunSolutionResult { SubmissionId = submission.Id };
			}
			
			/* Update the submission */
			submission = userSolutionsRepo.FindNoTrackingSubmission(submission.Id);
			
			if (submission.AutomaticCheckingIsRightAnswer)
				await CreateStyleErrorsReviewsForSubmission(submission, buildResult.StyleErrors, exerciseMetricId);
			
			var automaticChecking = submission.AutomaticChecking;
			bool sentToReview;
			if (!submissionLanguage.HasAutomaticChecking())
				sentToReview = await SendToReviewAndUpdateScore(submission, courseManager, slideCheckingsRepo, groupsRepo, visitsRepo, metricSender, true).ConfigureAwait(false);
			else
				sentToReview = slideCheckingsRepo.HasManualExerciseChecking(courseId, exerciseSlide.Id, userId, submission.Id);
			
			var result = new RunSolutionResult
			{
				IsCompileError = automaticChecking?.IsCompilationError ?? false,
				ErrorMessage = automaticChecking?.CompilationError.Text ?? "",
				IsRightAnswer = submission.AutomaticCheckingIsRightAnswer,
				ExpectedOutput = exerciseBlock.HideExpectedOutputOnError ? null : exerciseSlide.Exercise.ExpectedOutput?.NormalizeEoln(),
				ActualOutput = automaticChecking?.Output.Text ?? "",
				ExecutionServiceName = automaticChecking?.ExecutionServiceName ?? "ulearn",
				SentToReview = sentToReview,
				SubmissionId = submission.Id,
			};
			if (buildResult.HasStyleErrors)
			{
				result.IsStyleViolation = true;
				result.StyleMessage = string.Join("\n", buildResult.StyleErrors.Select(e => e.GetMessageWithPositions()));
			}
			return result;
		}
		
		public static async Task<bool> SendToReviewAndUpdateScore(UserExerciseSubmission submission,
			CourseManager courseManager, SlideCheckingsRepo slideCheckingsRepo, GroupsRepo groupsRepo, VisitsRepo visitsRepo, MetricSender metricSender,
			bool startTransaction)
		{
			var userId = submission.User.Id;
			var courseId = submission.CourseId;
			var course = courseManager.GetCourse(courseId);
			var exerciseSlide = course.FindSlideById(submission.SlideId) as ExerciseSlide;
			if (exerciseSlide == null)
				return false;
			var exerciseMetricId = GetExerciseMetricId(courseId, exerciseSlide);
			var automaticChecking = submission.AutomaticChecking;
			var isProhibitedUserToSendForReview = slideCheckingsRepo.IsProhibitedToSendExerciseToManualChecking(courseId, exerciseSlide.Id, userId);
			var sendToReview = exerciseSlide.Scoring.RequireReview
								&& submission.AutomaticCheckingIsRightAnswer
								&& !isProhibitedUserToSendForReview
								&& groupsRepo.IsManualCheckingEnabledForUser(course, userId);
			if (sendToReview)
			{
				await slideCheckingsRepo.RemoveWaitingManualCheckings<ManualExerciseChecking>(courseId, exerciseSlide.Id, userId, false);
				await slideCheckingsRepo.AddManualExerciseChecking(courseId, exerciseSlide.Id, userId, submission);
				await visitsRepo.MarkVisitsAsWithManualChecking(courseId, exerciseSlide.Id, userId);
				metricSender.SendCount($"exercise.{exerciseMetricId}.sent_to_review");
				metricSender.SendCount("exercise.sent_to_review");
			}

			await visitsRepo.UpdateScoreForVisit(courseId, exerciseSlide.Id, exerciseSlide.MaxScore, userId);

			if (automaticChecking != null)
			{
				var verdictForMetric = automaticChecking.GetVerdict().Replace(" ", "");
				metricSender.SendCount($"exercise.{exerciseMetricId}.{verdictForMetric}");
			}

			return sendToReview;
		}

		private static string GetExerciseMetricId(string courseId, ExerciseSlide exerciseSlide)
		{
			var slideTitleForMetric = exerciseSlide.LatinTitle.Replace(".", "_").ToLower(CultureInfo.InvariantCulture);
			if (slideTitleForMetric.Length > 25)
				slideTitleForMetric = slideTitleForMetric.Substring(0, 25);
			return $"{courseId.ToLower(CultureInfo.InvariantCulture)}.{exerciseSlide.Id.ToString("N").Substring(32 - 25)}.{slideTitleForMetric}";
		}
		
		private async Task CreateStyleErrorsReviewsForSubmission(UserExerciseSubmission submission, IEnumerable<SolutionStyleError> styleErrors, string exerciseMetricId)
		{
			var ulearnBotUserId = usersRepo.GetUlearnBotUserId();
			foreach (var error in styleErrors)
			{
				if (! await styleErrorsRepo.IsStyleErrorEnabledAsync(error.ErrorType))
					continue;
				
				await slideCheckingsRepo.AddExerciseCodeReview(
					submission,
					ulearnBotUserId,
					error.Span.StartLinePosition.Line,
					error.Span.StartLinePosition.Character,
					error.Span.EndLinePosition.Line,
					error.Span.EndLinePosition.Character,
					error.Message
				);
				
				var errorName = Enum.GetName(typeof(StyleErrorType), error.ErrorType);
				metricSender.SendCount("exercise.style_error");
				metricSender.SendCount($"exercise.style_error.{errorName}");
				metricSender.SendCount($"exercise.{exerciseMetricId}.style_error");
				metricSender.SendCount($"exercise.{exerciseMetricId}.style_error.{errorName}");
			}
		}
	}
}
