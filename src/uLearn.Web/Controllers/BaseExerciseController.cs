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
using uLearn.CSharp;
using uLearn.Telegram;
using uLearn.Web.Models;
using Ulearn.Common.Extensions;

namespace uLearn.Web.Controllers
{
	public class BaseExerciseController : JsonDataContractController
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(BaseExerciseController));
		private readonly ErrorsBot errorsBot = new ErrorsBot();

		protected readonly ULearnDb db;
		protected readonly CourseManager courseManager;
		protected readonly GraphiteMetricSender metricSender;

		protected readonly UserSolutionsRepo userSolutionsRepo;
		protected readonly SlideCheckingsRepo slideCheckingsRepo;
		protected readonly GroupsRepo groupsRepo;
		protected readonly VisitsRepo visitsRepo;
		protected readonly NotificationsRepo notificationsRepo;
		protected readonly UsersRepo usersRepo;
		protected readonly StyleErrorsRepo styleErrorsRepo;
		
		private static readonly TimeSpan executionTimeout = TimeSpan.FromSeconds(45);
		
		public BaseExerciseController()
			: this(new ULearnDb(), WebCourseManager.Instance, new GraphiteMetricSender("web"))
		{
		}

		public BaseExerciseController(ULearnDb db, CourseManager courseManager, GraphiteMetricSender metricSender)
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
		}

		private string GenerateSubmissionName(Slide exerciseSlide, string userName)
		{
			return $"{userName}: {exerciseSlide.Info.Unit.Title} - {exerciseSlide.Title}";
		}

		protected async Task<RunSolutionResult> CheckSolution(string courseId, ExerciseSlide exerciseSlide, string userCode, string userId, string userName, bool waitUntilChecked, bool saveSubmissionOnCompileErrors)
		{
			var slideTitleForMetric = exerciseSlide.LatinTitle.Replace(".", "_").ToLower(CultureInfo.InvariantCulture);
			if (slideTitleForMetric.Length > 25)
				slideTitleForMetric = slideTitleForMetric.Substring(0, 25);
			var exerciseMetricId = $"{courseId.ToLower(CultureInfo.InvariantCulture)}.{exerciseSlide.Id.ToString("N").Substring(32 - 25)}.{slideTitleForMetric}";
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
			var submission = await userSolutionsRepo.AddUserExerciseSubmission(
				courseId, exerciseSlide.Id,
				userCode, compilationErrorMessage, null,
				userId, "uLearn", GenerateSubmissionName(exerciseSlide, userName),
				dontRunSubmission ? AutomaticExerciseCheckingStatus.Done : AutomaticExerciseCheckingStatus.Waiting
			);

			if (buildResult.HasErrors)
				return new RunSolutionResult { IsCompileError = true, ErrorMessage = buildResult.ErrorMessage, SubmissionId = submission.Id, ExecutionServiceName = "uLearn" };

			try
			{
				await userSolutionsRepo.RunSubmission(submission, executionTimeout, waitUntilChecked);
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
					ExecutionServiceName = "uLearn"
				};
			}

			if (!waitUntilChecked)
			{
				metricSender.SendCount($"exercise.{exerciseMetricId}.dont_wait_result");
				return new RunSolutionResult { SubmissionId = submission.Id };
			}

			/* Update the submission */
			submission = userSolutionsRepo.FindNoTrackingSubmission(submission.Id);

			var automaticChecking = submission.AutomaticChecking;
			var isProhibitedUserToSendForReview = slideCheckingsRepo.IsProhibitedToSendExerciseToManualChecking(courseId, exerciseSlide.Id, userId);
			var sendToReview = exerciseBlock.RequireReview &&
								automaticChecking.IsRightAnswer &&
								!isProhibitedUserToSendForReview &&
								groupsRepo.IsManualCheckingEnabledForUser(course, userId);
			if (sendToReview)
			{
				await slideCheckingsRepo.RemoveWaitingManualExerciseCheckings(courseId, exerciseSlide.Id, userId);
				await slideCheckingsRepo.AddManualExerciseChecking(courseId, exerciseSlide.Id, userId, submission);
				await visitsRepo.MarkVisitsAsWithManualChecking(exerciseSlide.Id, userId);
				metricSender.SendCount($"exercise.{exerciseMetricId}.sent_to_review");
				metricSender.SendCount("exercise.sent_to_review");
			}
			await visitsRepo.UpdateScoreForVisit(courseId, exerciseSlide.Id, userId);

			var verdictForMetric = automaticChecking.GetVerdict().Replace(" ", "");
			metricSender.SendCount($"exercise.{exerciseMetricId}.{verdictForMetric}");

			if (automaticChecking.IsRightAnswer)
				await CreateStyleErrorsReviewsForSubmission(submission, buildResult.StyleErrors);

			var result = new RunSolutionResult
			{
				IsCompileError = automaticChecking.IsCompilationError,
				ErrorMessage = automaticChecking.CompilationError.Text,
				IsRightAnswer = automaticChecking.IsRightAnswer,
				ExpectedOutput = exerciseBlock.HideExpectedOutputOnError ? null : exerciseSlide.Exercise.ExpectedOutput.NormalizeEoln(),
				ActualOutput = automaticChecking.Output.Text,
				ExecutionServiceName = automaticChecking.ExecutionServiceName,
				SentToReview = sendToReview,
				SubmissionId = submission.Id,
			};
			if (buildResult.HasStyleErrors)
			{
				result.IsStyleViolation = true;
				result.StyleMessage = string.Join("\n", buildResult.StyleErrors.Select(e => e.GetMessageWithPositions()));
			}
			return result;
		}

		private async Task CreateStyleErrorsReviewsForSubmission(UserExerciseSubmission submission, IEnumerable<SolutionStyleError> styleErrors)
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
			}
		}
	}
}