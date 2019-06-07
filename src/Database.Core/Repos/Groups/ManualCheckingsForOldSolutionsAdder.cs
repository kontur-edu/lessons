using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Quizzes;

namespace Database.Repos.Groups
{
	public class ManualCheckingsForOldSolutionsAdder : IManualCheckingsForOldSolutionsAdder
	{
		private readonly IUserSolutionsRepo userSolutionsRepo;
		private readonly ISlideCheckingsRepo slideCheckingsRepo;
		private readonly IVisitsRepo visitsRepo;
		private readonly IUserQuizzesRepo userQuizzesRepo;
		private readonly WebCourseManager courseManager;
		private readonly ILogger logger;

		public ManualCheckingsForOldSolutionsAdder(
			IUserSolutionsRepo userSolutionsRepo, ISlideCheckingsRepo slideCheckingsRepo, IVisitsRepo visitsRepo, IUserQuizzesRepo userQuizzesRepo,
			WebCourseManager courseManager, ILogger logger)
		{
			this.userSolutionsRepo = userSolutionsRepo;
			this.slideCheckingsRepo = slideCheckingsRepo;
			this.visitsRepo = visitsRepo;
			this.userQuizzesRepo = userQuizzesRepo;
			this.courseManager = courseManager;
			this.logger = logger;
		}

		public async Task AddManualCheckingsForOldSolutionsAsync(string courseId, IEnumerable<string> usersIds)
		{
			foreach (var userId in usersIds)
				await AddManualCheckingsForOldSolutionsAsync(courseId, userId).ConfigureAwait(false);
		}

		public async Task AddManualCheckingsForOldSolutionsAsync(string courseId, string userId)
		{
			logger.Information($"Создаю ручные проверки для всех решения пользователя {userId} в курсе {courseId}");

			var course = courseManager.GetCourse(courseId);
			
			/* For exercises */
			var acceptedSubmissionsBySlide = userSolutionsRepo.GetAllAcceptedSubmissionsByUser(courseId, userId)
				.GroupBy(s => s.SlideId)
				.ToDictionary(g => g.Key, g => g.ToList());
			foreach (var acceptedSubmissionsForSlide in acceptedSubmissionsBySlide.Values)
				/* If exists at least one manual checking for at least one submissions on slide, then ignore this slide */
				if (!acceptedSubmissionsForSlide.Any(s => s.ManualCheckings.Any()))
				{
					/* Otherwise found the latest accepted submission */
					var lastSubmission = acceptedSubmissionsForSlide.OrderByDescending(s => s.Timestamp).First();

					var slideId = lastSubmission.SlideId;
					var slide = course.FindSlideById(slideId) as ExerciseSlide;
					if (slide == null || !slide.Scoring.RequireReview)
						continue;

					logger.Information($"Создаю ручную проверку для решения {lastSubmission.Id}, слайд {slideId}");
					await slideCheckingsRepo.AddManualExerciseChecking(courseId, slideId, userId, lastSubmission).ConfigureAwait(false);
					await visitsRepo.MarkVisitsAsWithManualChecking(slideId, userId).ConfigureAwait(false);
				}

			/* For quizzes */
			var passedQuizzesIds = await userQuizzesRepo.GetPassedSlideIdsAsync(courseId, userId).ConfigureAwait(false);
			foreach (var quizSlideId in passedQuizzesIds)
			{
				var slide = course.FindSlideById(quizSlideId) as QuizSlide;
				if (slide == null || !slide.ManualChecking)
					continue;
				if (! await userQuizzesRepo.IsWaitingForManualCheckAsync(courseId, quizSlideId, userId).ConfigureAwait(false))
				{
					logger.Information($"Создаю ручную проверку для теста {slide.Id}");
					await slideCheckingsRepo.AddQuizAttemptForManualChecking(courseId, quizSlideId, userId).ConfigureAwait(false);
					await visitsRepo.MarkVisitsAsWithManualChecking(quizSlideId, userId).ConfigureAwait(false);
				}
			}
		}
	}
}