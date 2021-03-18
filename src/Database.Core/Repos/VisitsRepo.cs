using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Vostok.Logging.Abstractions;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;

namespace Database.Repos
{
	public class VisitsRepo : IVisitsRepo
	{
		private readonly UlearnDb db;
		private readonly ISlideCheckingsRepo slideCheckingsRepo;
		private static ILog log => LogProvider.Get().ForContext(typeof(VisitsRepo));

		public VisitsRepo(UlearnDb db, ISlideCheckingsRepo slideCheckingsRepo)
		{
			this.db = db;
			this.slideCheckingsRepo = slideCheckingsRepo;
		}

		public async Task<Visit> AddVisit(string courseId, Guid slideId, string userId, string ipAddress)
		{
			await SetLastVisit(courseId, slideId, userId);
			var visit = await FindVisit(courseId, slideId, userId);
			if (visit == null)
			{
				db.Visits.Add(new Visit
				{
					UserId = userId,
					CourseId = courseId,
					SlideId = slideId,
					Timestamp = DateTime.Now,
					IpAddress = ipAddress,
				});
				await db.SaveChangesAsync();
				return await FindVisit(courseId, slideId, userId);
			}
			if (visit.IpAddress != ipAddress)
			{
				visit.IpAddress = ipAddress;
			}
			await db.SaveChangesAsync();
			return visit;
		}

		private async Task SetLastVisit(string courseId, Guid slideId, string userId)
		{
			var lastVisit = await FindLastVisit(courseId, userId, slideId);
			if (lastVisit == null)
			{
				db.LastVisits.Add(new LastVisit
				{
					UserId = userId,
					CourseId = courseId,
					SlideId = slideId,
					Timestamp = DateTime.Now,
				});
			}
			else
			{
				lastVisit.Timestamp = DateTime.Now;
			}
		}

		public async Task<Visit> FindVisit(string courseId, Guid slideId, string userId)
		{
			return await db.Visits.FirstOrDefaultAsync(v => v.CourseId == courseId && v.SlideId == slideId && v.UserId == userId);
		}

		public async Task<LastVisit> FindLastVisit(string courseId, string userId, Guid? slideId = null)
		{
			if (slideId == null)
				return await db.LastVisits
					.OrderByDescending(v => v.Timestamp)
					.FirstOrDefaultAsync(v => v.CourseId == courseId && v.UserId == userId);
			return await db.LastVisits
				.FirstOrDefaultAsync(v => v.CourseId == courseId && v.UserId == userId && slideId == v.SlideId);
		}

		public async Task<Dictionary<Guid, LastVisit>> GetLastVisitsInCourse(string courseId, string userId)
		{
			return (await db.LastVisits
				.Where(v => v.CourseId == courseId && v.UserId == userId)
				.ToListAsync())
				.ToDictSafe(v => v.SlideId, v => v);
		}

		public async Task<Dictionary<string, DateTime>> GetLastVisitsForCourses(HashSet<string> courseIds, string userId)
		{
			var visits = await db.LastVisits
				.Where(v => courseIds.Contains(v.CourseId) && v.UserId == userId)
				.Select(v => v.CourseId)
				.Distinct()
				.Select(i => db.LastVisits
					.Where(v => v.CourseId == i)
					.OrderByDescending(v => v.Timestamp)
					.FirstOrDefault())
				.ToListAsync();
			return visits.ToDictionary(v => v!.CourseId, v => v!.Timestamp, StringComparer.OrdinalIgnoreCase);
		}

		public async Task<Dictionary<string, Visit>> FindLastVisit(List<string> userIds)
		{
			return await db.Visits
				.Where(v => userIds.Contains(v.UserId))
				.Select(v => v.UserId)
				.Distinct()
				.Select(u => db.Visits
					.Where(v => v.UserId == u)
					.OrderByDescending(v => v.Timestamp)
					.FirstOrDefault()
				)
				.ToDictionaryAsync(v => v.UserId, v => v);
		}

		public async Task<HashSet<Guid>> GetIdOfVisitedSlides(string courseId, string userId)
		{
			return new HashSet<Guid>(await db.Visits.Where(v => v.UserId == userId && v.CourseId == courseId).Select(x => x.SlideId).ToListAsync());
		}

		public async Task UpdateScoreForVisit(string courseId, Slide slide, string userId)
		{
			var maxSlideScore = slide.MaxScore;
			var newScore = slide is ExerciseSlide ex
				? (await slideCheckingsRepo.GetExerciseSlideScoreAndPercent(courseId, ex, userId)).Score
				: await slideCheckingsRepo.GetUserScoreForQuizSlide(courseId, slide.Id, userId);
			newScore = Math.Min(newScore, maxSlideScore);
			var isPassed = await slideCheckingsRepo.IsSlidePassed(courseId, slide.Id, userId);
			if (await IsSkipped(courseId, slide.Id, userId))
				newScore = 0;
			log.Info($"Обновляю количество баллов пользователя {userId} за слайд {slide.Id} в курсе \"{courseId}\". " +
					$"Новое количество баллов: {newScore}, слайд пройден: {isPassed}");
			await UpdateAttempts(courseId, slide.Id, userId, visit =>
			{
				visit.Score = newScore;
				visit.IsPassed = isPassed;
			});
		}

		private async Task UpdateAttempts(string courseId, Guid slideId, string userId, Action<Visit> action)
		{
			var visit = await FindVisit(courseId, slideId, userId);
			if (visit == null)
			{
				await AddVisit(courseId, slideId, userId, null);
				visit = await FindVisit(courseId, slideId, userId);
			}

			action(visit);
			await db.SaveChangesAsync();
		}

		public async Task RemoveAttempts(string courseId, Guid slideId, string userId)
		{
			await UpdateAttempts(courseId, slideId, userId, visit =>
			{
				visit.Score = 0;
				visit.IsPassed = false;
			});
		}

		public async Task<Dictionary<Guid, int>> GetScoresForSlides(string courseId, string userId, IEnumerable<Guid> slidesIds = null)
		{
			var visits = db.Visits.Where(v => v.CourseId == courseId && v.UserId == userId);
			if (slidesIds != null)
				visits = visits.Where(v => slidesIds.Contains(v.SlideId));
			return (await visits
					.Select(v => new { v.SlideId, v.Score })
					.ToListAsync())
				.GroupBy(v => v.SlideId, (s, v) => new { Key = s, Value = v.First().Score })
				.ToDictionary(g => g.Key, g => g.Value);
		}

		public async Task<Dictionary<string, Dictionary<Guid, int>>> GetScoresForSlides(string courseId, IEnumerable<string> userIds, IEnumerable<Guid> slidesIds = null)
		{
			var visits = db.Visits.Where(v => v.CourseId == courseId && userIds.Contains(v.UserId));
			if (slidesIds != null)
				visits = visits.Where(v => slidesIds.Contains(v.SlideId));
			return (await visits
					.Select(v => new { v.UserId, v.SlideId, v.Score })
					.ToListAsync())
				.GroupBy(v => new { v.UserId, v.SlideId }, (s, v) => new { s.UserId, s.SlideId, Value = v.First().Score })
				.GroupBy(g => g.UserId)
				.ToDictionary(g => g.Key, g => g.ToDictionary(k => k.SlideId, k => k.Value));
		}

		public async Task<Dictionary<string, HashSet<Guid>>> GetSkippedSlides(string courseId, IEnumerable<string> userIds, IEnumerable<Guid> slidesIds = null)
		{
			var visits = db.Visits.Where(v => v.CourseId == courseId && userIds.Contains(v.UserId));
			if (slidesIds != null)
				visits = visits.Where(v => slidesIds.Contains(v.SlideId));
			return (await visits
					.Where(s => s.IsSkipped)
					.Select(v => new { v.UserId, v.SlideId })
					.ToListAsync())
				.GroupBy(v => v.UserId)
				.ToDictionary(g => g.Key, g => g.Select(v => v.SlideId).ToHashSet());
		}

		public async Task<List<Guid>> GetSlidesWithUsersManualChecking(string courseId, string userId)
		{
			return await db.Visits.Where(v => v.CourseId == courseId && v.UserId == userId)
				.Where(v => v.HasManualChecking)
				.Select(v => v.SlideId)
				.ToListAsync();
		}

		public async Task MarkVisitsAsWithManualChecking(string courseId, Guid slideId, string userId)
		{
			await UpdateAttempts(courseId, slideId, userId, visit => { visit.HasManualChecking = true; });
		}

		public async Task<int> GetScore(string courseId, Guid slideId, string userId)
		{
			return await db.Visits
				.Where(v => v.CourseId == courseId && v.SlideId == slideId && v.UserId == userId)
				.Select(v => v.Score)
				.FirstOrDefaultAsync();
		}

		public async Task<Dictionary<string, int>> GetScore(string courseId, Guid slideId, List<string> userIds)
		{
			var scores = (await db.Visits
					.Where(v => v.CourseId == courseId && v.SlideId == slideId && userIds.Contains(v.UserId))
					.Select(v => new { v.UserId, v.Score })
					.ToListAsync())
				.GroupBy(v => v.UserId)
				.ToDictionary(v => v.Key, v => v.First().Score);
			foreach (var userId in userIds)
				if (!scores.ContainsKey(userId))
					scores[userId] = 0;
			return scores;
		}

		public async Task SkipSlide(string courseId, Guid slideId, string userId)
		{
			var visiter = await FindVisit(courseId, slideId, userId);
			if (visiter != null)
				visiter.IsSkipped = true;
			else
				db.Visits.Add(new Visit
				{
					UserId = userId,
					CourseId = courseId,
					SlideId = slideId,
					Timestamp = DateTime.Now,
					IsSkipped = true
				});
			await db.SaveChangesAsync();
		}

		public async Task<bool> IsSkipped(string courseId, Guid slideId, string userId)
		{
			return await db.Visits.AnyAsync(v => v.CourseId == courseId && v.SlideId == slideId && v.UserId == userId && v.IsSkipped);
		}

		public async Task UnskipAllSlides(string courseId, string userId)
		{
			var skippedVisits = await db.Visits.Where(v => v.CourseId == courseId && v.UserId == userId && v.IsSkipped).ToListAsync();
			foreach (var v in skippedVisits)
				v.IsSkipped = false;
			await db.SaveChangesAsync();
		}

		public async Task<bool> IsPassed(string courseId, Guid slideId, string userId)
		{
			return await db.Visits.AnyAsync(v => v.CourseId == courseId && v.SlideId == slideId && v.UserId == userId && v.IsPassed);
		}

		public async Task<bool> IsSkippedOrPassed(string courseId, Guid slideId, string userId)
		{
			return await db.Visits.AnyAsync(v => v.CourseId == courseId && v.SlideId == slideId && v.UserId == userId && (v.IsPassed || v.IsSkipped));
		}

		public async Task AddVisits(IEnumerable<Visit> visits)
		{
			foreach (var visit in visits)
			{
				if (await db.Visits.AnyAsync(x => x.UserId == visit.UserId && x.SlideId == visit.SlideId && x.CourseId == visit.CourseId))
					continue;
				db.Visits.Add(visit);
			}

			await db.SaveChangesAsync();
		}

		public IQueryable<Visit> GetVisitsInPeriod(string courseId, IEnumerable<Guid> slidesIds, DateTime periodStart, DateTime periodFinish, IEnumerable<string> usersIds = null)
		{
			var filteredVisits = db.Visits.Where(v => v.CourseId == courseId && slidesIds.Contains(v.SlideId));
			if (periodStart != DateTime.MinValue || periodFinish < DateTime.Now.AddSeconds(-10))
				filteredVisits = filteredVisits.Where(v => periodStart <= v.Timestamp && v.Timestamp <= periodFinish);
			if (usersIds != null)
				filteredVisits = filteredVisits.Where(v => usersIds.Contains(v.UserId));
			return filteredVisits;
		}

		public IQueryable<Visit> GetVisitsInPeriod(VisitsFilterOptions options)
		{
			var filteredVisits = db.Visits.Where(v => v.CourseId == options.CourseId);
			if (options.PeriodStart != DateTime.MinValue || options.PeriodFinish < DateTime.Now.AddSeconds(-10))
				filteredVisits = filteredVisits.Where(v => options.PeriodStart <= v.Timestamp && v.Timestamp <= options.PeriodFinish);
			if (options.SlidesIds != null)
				filteredVisits = filteredVisits.Where(v => options.SlidesIds.Contains(v.SlideId));
			if (options.UserIds != null)
			{
				if (options.IsUserIdsSupplement)
					filteredVisits = filteredVisits.Where(v => !options.UserIds.Contains(v.UserId));
				else
					filteredVisits = filteredVisits.Where(v => options.UserIds.Contains(v.UserId));
			}

			return filteredVisits;
		}

		public async Task<Dictionary<Guid, List<Visit>>> GetVisitsInPeriodForEachSlide(VisitsFilterOptions options)
		{
			return (await GetVisitsInPeriod(options)
					.ToListAsync())
				.GroupBy(v => v.SlideId)
				.ToDictionary(g => g.Key, g => g.ToList());
		}

		public async Task<List<string>> GetUsersVisitedAllSlides(VisitsFilterOptions options)
		{
			if (options.SlidesIds == null)
				throw new ArgumentNullException(nameof(options.SlidesIds));
			var slidesCount = options.SlidesIds.Count;
			return await GetVisitsInPeriod(options)
				.Select(v => new { v.UserId, v.SlideId })
				.Distinct()
				.GroupBy(v => v.UserId)
				.Where(g => g.Count() == slidesCount)
				.Select(g => g.Key)
				.ToListAsync();
		}

		public async Task<HashSet<string>> GetUserCourses(string userId)
		{
			return new HashSet<string>(await db.Visits.Where(v => v.UserId == userId).Select(v => v.CourseId).Distinct().ToListAsync());
		}

		public async Task<List<string>> GetCourseUsers(string courseId)
		{
			return await db.Visits.Where(v => v.CourseId == courseId).Select(v => v.UserId).Distinct().ToListAsync();
		}

		// Метод переписан, но может не работать, нужно тестировать совместимость с EF Core
		public async Task<List<RatingEntry>> GetCourseRating(string courseId, int minScore, List<Guid> requiredSlides)
		{
			var userId2TotalScore = await db.Visits.Where(v => v.CourseId == courseId)
				.GroupBy(v => v.UserId)
				.Select(g => new
				{
					UserId = g.Key,
					TotalScore = g.Sum(v => v.Score),
					LastVisit = g.Max(v => v.Timestamp)
				})
				.Where(p => p.TotalScore >= minScore)
				.ToListAsync();
			var userIds = userId2TotalScore.Select(g => g.UserId).ToList();

			List<string> usersWithAllRequiredSlides;
			if (requiredSlides.Count > 0)
			{
				usersWithAllRequiredSlides = (await db.Visits
						.Where(v => v.CourseId == courseId && requiredSlides.Contains(v.SlideId) && userIds.Contains(v.UserId))
						.GroupBy(v => v.UserId)
						.ToListAsync())
					.Where(g => g.DistinctBy(v => v.SlideId).Count() >= requiredSlides.Count)
					.Select(g => g.Key)
					.ToList();
			}
			else
			{
				usersWithAllRequiredSlides = userIds;
			}

			return userId2TotalScore
				.Where(p => usersWithAllRequiredSlides.Contains(p.UserId))
				.Select(p => new RatingEntry(p.UserId, p.TotalScore, p.LastVisit))
				.OrderByDescending(r => r.Score)
				.ToList();
		}
	}

	public class RatingEntry
	{
		public RatingEntry(string userId, int score, DateTime lastVisitTime)
		{
			UserId = userId;
			Score = score;
			LastVisitTime = lastVisitTime;
		}

		public readonly string UserId;
		public readonly int Score;
		public readonly DateTime LastVisitTime;
	}
}