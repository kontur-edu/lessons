using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using RunCsJob.Api;
using uLearn;
using Ulearn.Core;

namespace Database.Repos
{
	public interface IUserSolutionsRepo
	{
		Task<UserExerciseSubmission> AddUserExerciseSubmission(
			string courseId, Guid slideId,
			string code, string compilationError, string output,
			string userId, string executionServiceName, string displayName,
			AutomaticExerciseCheckingStatus status = AutomaticExerciseCheckingStatus.Waiting);

		Task RemoveSubmission(UserExerciseSubmission submission);
		Task SetAntiPlagiarismSubmissionId(UserExerciseSubmission submission, int antiPlagiarismSubmissionId);
		UserExerciseSubmission FindSubmissionByAntiPlagiarismSubmissionId(int antiPlagiarismSubmissionId);

		///<returns>(likesCount, isLikedByThisUsed)</returns>
		Task<Tuple<int, bool>> Like(int solutionId, string userId);

		IQueryable<UserExerciseSubmission> GetAllSubmissions(string courseId, bool includeManualCheckings=true);
		IQueryable<UserExerciseSubmission> GetAllSubmissions(string courseId, IEnumerable<Guid> slidesIds);
		IQueryable<UserExerciseSubmission> GetAllSubmissions(string courseId, IEnumerable<Guid> slidesIds, DateTime periodStart, DateTime periodFinish);
		IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissions(string courseId, IEnumerable<Guid> slidesIds, DateTime periodStart, DateTime periodFinish);
		IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissions(string courseId, IEnumerable<Guid> slidesIds);
		IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissions(string courseId);
		IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissionsByUser(string courseId, IEnumerable<Guid> slideIds, string userId);
		IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissionsByUser(string courseId, string userId);
		IQueryable<UserExerciseSubmission> GetAllAcceptedSubmissionsByUser(string courseId, Guid slideId, string userId);
		IQueryable<UserExerciseSubmission> GetAllSubmissionsByUser(string courseId, Guid slideId, string userId);
		IQueryable<UserExerciseSubmission> GetAllSubmissionsByUsers(SubmissionsFilterOptions filterOptions);
		List<AcceptedSolutionInfo> GetBestTrendingAndNewAcceptedSolutions(string courseId, List<Guid> slidesIds);
		List<AcceptedSolutionInfo> GetBestTrendingAndNewAcceptedSolutions(string courseId, Guid slideId);
		int GetAcceptedSolutionsCount(string courseId, Guid slideId);
		bool IsCheckingSubmissionByUser(string courseId, Guid slideId, string userId, DateTime periodStart, DateTime periodFinish);
		HashSet<Guid> GetIdOfPassedSlides(string courseId, string userId);
		IQueryable<UserExerciseSubmission> GetAllSubmissions(int max, int skip);
		UserExerciseSubmission FindNoTrackingSubmission(int id);
		Task<UserExerciseSubmission> GetUnhandledSubmission(string agentName, Language language);
		UserExerciseSubmission FindSubmissionById(int id);
		UserExerciseSubmission FindSubmissionById(string id);
		List<UserExerciseSubmission> FindSubmissionsByIds(List<string> checkingsIds);
		Task SaveResults(List<RunningResults> results);
		Task RunAutomaticChecking(UserExerciseSubmission submission, TimeSpan timeout, bool waitUntilChecked);
		Dictionary<int, string> GetSolutionsForSubmissions(IEnumerable<int> submissionsIds);
		Task WaitAnyUnhandledSubmissions(TimeSpan timeout);
		Task WaitUntilSubmissionHandled(TimeSpan timeout, int submissionId);
	}
}