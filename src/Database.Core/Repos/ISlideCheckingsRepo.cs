using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos
{
	public interface ISlideCheckingsRepo
	{
		Task AddQuizAttemptForManualChecking(string courseId, Guid slideId, string userId);
		Task AddQuizAttemptWithAutomaticChecking(string courseId, Guid slideId, string userId, int automaticScore);
		IEnumerable<ManualExerciseChecking> GetUsersPassedManualExerciseCheckings(string courseId, string userId);
		Task<ManualExerciseChecking> AddManualExerciseChecking(string courseId, Guid slideId, string userId, UserExerciseSubmission submission);
		Task RemoveWaitingManualExerciseCheckings(string courseId, Guid slideId, string userId);
		Task RemoveAttempts(string courseId, Guid slideId, string userId, bool saveChanges = true);
		bool IsSlidePassed(string courseId, Guid slideId, string userId);
		int GetManualScoreForSlide(string courseId, Guid slideId, string userId);
		Dictionary<string, int> GetManualScoresForSlide(string courseId, Guid slideId, List<string> userIds);
		int GetAutomaticScoreForSlide(string courseId, Guid slideId, string userId);
		Dictionary<string, int> GetAutomaticScoresForSlide(string courseId, Guid slideId, List<string> userIds);
		IQueryable<T> GetManualCheckingQueueAsync<T>(ManualCheckingQueueFilterOptions options) where T : AbstractManualSlideChecking;
		T FindManualCheckingById<T>(int id) where T : AbstractManualSlideChecking;
		bool IsProhibitedToSendExerciseToManualChecking(string courseId, Guid slideId, string userId);
		Task LockManualChecking<T>(T checkingItem, string lockedById) where T : AbstractManualSlideChecking;
		Task MarkManualCheckingAsChecked<T>(T queueItem, int score) where T : AbstractManualSlideChecking;
		Task ProhibitFurtherExerciseManualChecking(ManualExerciseChecking checking);
		Task<ExerciseCodeReview> AddExerciseCodeReview(ManualExerciseChecking checking, string userId, int startLine, int startPosition, int finishLine, int finishPosition, string comment, bool setAddingTime=true);
		Task<ExerciseCodeReview> AddExerciseCodeReview(UserExerciseSubmission submission, string userId, int startLine, int startPosition, int finishLine, int finishPosition, string comment, bool setAddingTime=false);
		ExerciseCodeReview FindExerciseCodeReviewById(int reviewId);
		Task DeleteExerciseCodeReview(ExerciseCodeReview review);
		Task UpdateExerciseCodeReview(ExerciseCodeReview review, string newComment);
		Dictionary<int, List<ExerciseCodeReview>> GetExerciseCodeReviewForCheckings(IEnumerable<int> checkingsIds);
		List<string> GetTopUserReviewComments(string courseId, Guid slideId, string userId, int count);
		Task HideFromTopCodeReviewComments(string courseId, Guid slideId, string userId, string comment);
		List<ExerciseCodeReview> GetAllReviewComments(string courseId, Guid slideId);
	}
}