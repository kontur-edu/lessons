using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos
{
	public interface IVisitsRepo
	{
		Task AddVisit(string courseId, Guid slideId, string userId, string ipAddress);
		int GetVisitsCount(Guid slideId, string courseId);
		Visit FindVisit(string courseId, Guid slideId, string userId);
		bool IsUserVisitedSlide(string courseId, Guid slideId, string userId);
		HashSet<Guid> GetIdOfVisitedSlides(string courseId, string userId);
		bool HasVisitedSlides(string courseId, string userId);
		Task UpdateScoreForVisit(string courseId, Guid slideId, string userId);
		Task RemoveAttempts(Guid slideId, string userId);
		Dictionary<Guid, int> GetScoresForSlides(string courseId, string userId, IEnumerable<Guid> slidesIds = null);
		List<Guid> GetSlidesWithUsersManualChecking(string courseId, string userId);
		Task MarkVisitsAsWithManualChecking(Guid slideId, string userId);
		int GetScore(Guid slideId, string userId);
		Task SkipSlide(string courseId, Guid slideId, string userId);
		bool IsSkipped(string courseId, Guid slideId, string userId);
		bool IsPassed(Guid slideId, string userId);
		bool IsSkippedOrPassed(Guid slideId, string userId);
		Task AddVisits(IEnumerable<Visit> visits);
		IQueryable<Visit> GetVisitsInPeriod(IEnumerable<Guid> slidesIds, DateTime periodStart, DateTime periodFinish, IEnumerable<string> usersIds = null);
		IQueryable<Visit> GetVisitsInPeriod(VisitsFilterOptions options);
		Dictionary<Guid, List<Visit>> GetVisitsInPeriodForEachSlide(VisitsFilterOptions options);
		IEnumerable<string> GetUsersVisitedAllSlides(IImmutableSet<Guid> slidesIds, DateTime periodStart, DateTime periodFinish, IEnumerable<string> usersIds = null);
		IEnumerable<string> GetUsersVisitedAllSlides(VisitsFilterOptions options);
		HashSet<string> GetUserCourses(string userId);
		Task<List<string>> GetCourseUsersAsync(string courseId);
		List<RatingEntry> GetCourseRating(string courseId, int minScore);
	}
}