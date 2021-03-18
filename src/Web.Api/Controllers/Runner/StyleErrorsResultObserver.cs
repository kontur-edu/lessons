using System.Collections.Generic;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Metrics;
using Ulearn.Core.RunCheckerJobApi;

namespace Ulearn.Web.Api.Controllers.Runner
{
	public class StyleErrorsResultObserver : IResultObserver
	{
		private static string ulearnBotUserId;

		private readonly IWebCourseManager courseManager;
		private readonly MetricSender metricSender;
		private readonly ISlideCheckingsRepo slideCheckingsRepo;
		private readonly IUsersRepo usersRepo;

		public StyleErrorsResultObserver(IWebCourseManager courseManager, MetricSender metricSender,
			IUsersRepo usersRepo, ISlideCheckingsRepo slideCheckingsRepo)
		{
			this.courseManager = courseManager;
			this.metricSender = metricSender;
			this.slideCheckingsRepo = slideCheckingsRepo;
			this.usersRepo = usersRepo;
		}

		public async Task ProcessResult(UserExerciseSubmission submission, RunningResults result)
		{
			if (result.StyleErrors == null || result.StyleErrors.Count == 0)
				return;

			if (result.Verdict != Verdict.Ok)
				return;

			var checking = submission.AutomaticChecking;
			if (!checking.IsRightAnswer)
				return;

			var exerciseSlide = (await courseManager.FindCourseAsync(submission.CourseId))
				?.FindSlideById(submission.SlideId, true) as ExerciseSlide;
			if (exerciseSlide == null)
				return;

			if (ulearnBotUserId == null)
				ulearnBotUserId = await usersRepo.GetUlearnBotUserId();

			var exerciseMetricId = RunnerSetResultController.GetExerciseMetricId(submission.CourseId, exerciseSlide);

			await CreateStyleErrorsReviewsForSubmission(submission.Id, result.StyleErrors, exerciseMetricId);
		}

		public async Task<List<ExerciseCodeReview>> CreateStyleErrorsReviewsForSubmission(int? submissionId, List<StyleError> styleErrors, string exerciseMetricId)
		{
			if (ulearnBotUserId == null)
				ulearnBotUserId = await usersRepo.GetUlearnBotUserId();

			metricSender.SendCount($"exercise.{exerciseMetricId}.StyleViolation");

			var result = new List<ExerciseCodeReview>();
			foreach (var error in styleErrors)
			{
				var review = await slideCheckingsRepo.AddExerciseCodeReview(
					submissionId,
					ulearnBotUserId,
					error.Span.StartLinePosition.Line,
					error.Span.StartLinePosition.Character,
					error.Span.EndLinePosition.Line,
					error.Span.EndLinePosition.Character,
					error.Message
				);
				result.Add(review);

				var errorName = error.ErrorType;
				metricSender.SendCount("exercise.style_error");
				metricSender.SendCount($"exercise.style_error.{errorName}");
				metricSender.SendCount($"exercise.{exerciseMetricId}.style_error");
				metricSender.SendCount($"exercise.{exerciseMetricId}.style_error.{errorName}");
			}
			return result;
		}
	}
}