using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Extensions;
using Database.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common.Extensions;

namespace Database.Repos
{
	/* TODO (andgein): This repo is not fully migrated to .NET Core and EF Core */
	public class SlideCheckingsRepo : ISlideCheckingsRepo
	{
		private readonly UlearnDb db;

		public SlideCheckingsRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task AddQuizAttemptForManualChecking(string courseId, Guid slideId, string userId)
		{
			var manualChecking = new ManualQuizChecking
			{
				CourseId = courseId,
				SlideId = slideId,
				UserId = userId,
				Timestamp = DateTime.Now,
			};
			db.ManualQuizCheckings.Add(manualChecking);

			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task AddQuizAttemptWithAutomaticChecking(string courseId, Guid slideId, string userId, int automaticScore)
		{
			var automaticChecking = new AutomaticQuizChecking
			{
				CourseId = courseId,
				SlideId = slideId,
				UserId = userId,
				Timestamp = DateTime.Now,
				Score = automaticScore,
			};
			db.AutomaticQuizCheckings.Add(automaticChecking);

			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public IEnumerable<ManualExerciseChecking> GetUsersPassedManualExerciseCheckings(string courseId, string userId)
		{
			return db.ManualExerciseCheckings.Where(c => c.CourseId == courseId && c.UserId == userId && c.IsChecked).DistinctBy(c => c.SlideId);
		}

		public async Task<ManualExerciseChecking> AddManualExerciseChecking(string courseId, Guid slideId, string userId, UserExerciseSubmission submission)
		{
			var manualChecking = new ManualExerciseChecking
			{
				CourseId = courseId,
				SlideId = slideId,
				UserId = userId,
				Timestamp = DateTime.Now,
				SubmissionId = submission.Id,
			};
			db.ManualExerciseCheckings.Add(manualChecking);

			await db.SaveChangesAsync().ConfigureAwait(false);

			return manualChecking;
		}

		public async Task RemoveWaitingManualExerciseCheckings(string courseId, Guid slideId, string userId)
		{
			using (var transaction = db.Database.BeginTransaction())
			{
				var checkings = GetSlideCheckingsByUser<ManualExerciseChecking>(courseId, slideId, userId, noTracking: false)
					.AsEnumerable()
					.Where(c => !c.IsChecked && !c.IsLocked)
					.ToList();
				foreach (var checking in checkings)
				{
					// Use EntityState.Deleted because EF could don't know about these checkings (they have been retrieved via AsNoTracking())
					// TODO (andgein): Now it's not retrieved via AsNoTracking(). Fix this.
					foreach (var review in checking.Reviews.ToList())
						db.Entry(review).State = EntityState.Deleted;
					
					db.Entry(checking).State = EntityState.Deleted;
				}

				await db.SaveChangesAsync().ConfigureAwait(false);
				transaction.Commit();
			}
		}

		private IQueryable<T> GetSlideCheckingsByUser<T>(string courseId, Guid slideId, string userId, bool noTracking=true) where T : AbstractSlideChecking
		{
			return GetSlideCheckingsByUsers<T>(courseId, slideId, new List<string> { userId }, noTracking);
		}

		private IQueryable<T> GetSlideCheckingsByUsers<T>(string courseId, Guid slideId, IEnumerable<string> userIds, bool noTracking=true) where T : AbstractSlideChecking
		{
			IQueryable<T> dbRef = db.Set<T>();
			if (noTracking)
				dbRef = dbRef.AsNoTracking();
			var items = dbRef.Where(c => c.CourseId == courseId && c.SlideId == slideId && userIds.Contains(c.UserId));
			return items;
		}

		public async Task RemoveAttempts(string courseId, Guid slideId, string userId, bool saveChanges = true)
		{
			db.ManualQuizCheckings.RemoveSlideAction(courseId, slideId, userId);
			db.AutomaticQuizCheckings.RemoveSlideAction(courseId, slideId, userId);
			db.ManualExerciseCheckings.RemoveSlideAction(courseId, slideId, userId);
			db.AutomaticExerciseCheckings.RemoveSlideAction(courseId, slideId, userId);
			if (saveChanges)
				await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public bool IsSlidePassed(string courseId, Guid slideId, string userId)
		{
			return GetSlideCheckingsByUser<ManualQuizChecking>(courseId, slideId, userId).Any() ||
					GetSlideCheckingsByUser<AutomaticQuizChecking>(courseId, slideId, userId).Any() ||
					GetSlideCheckingsByUser<ManualExerciseChecking>(courseId, slideId, userId).Any() ||
					GetSlideCheckingsByUser<AutomaticExerciseChecking>(courseId, slideId, userId).Any(c => c.Score > 0);
		}

		#region Slide Score Calculating
		
		private int GetUserScoreForSlide<T>(string courseId, Guid slideId, string userId) where T : AbstractSlideChecking
		{
			return GetSlideCheckingsByUser<T>(courseId, slideId, userId).Select(c => c.Score).DefaultIfEmpty(0).Max();
		}

		private Dictionary<string, int> GetUserScoresForSlide<T>(string courseId, Guid slideId, IEnumerable<string> userIds) where T : AbstractSlideChecking
		{
			return GetSlideCheckingsByUsers<T>(courseId, slideId, userIds)
				.GroupBy(c => c.UserId)
				.Select(g => new { UserId = g.Key, Score = g.Select(c => c.Score).DefaultIfEmpty(0).Max() })
				.ToDictionary(g => g.UserId, g => g.Score);
		}

		public int GetManualScoreForSlide(string courseId, Guid slideId, string userId)
		{
			var quizScore = GetUserScoreForSlide<ManualQuizChecking>(courseId, slideId, userId);
			var exerciseScore = GetUserScoreForSlide<ManualExerciseChecking>(courseId, slideId, userId);

			return Math.Max(quizScore, exerciseScore);
		}
		
		public Dictionary<string, int> GetManualScoresForSlide(string courseId, Guid slideId, List<string> userIds)
		{
			var quizScore = GetUserScoresForSlide<ManualQuizChecking>(courseId, slideId, userIds);
			var exerciseScore = GetUserScoresForSlide<ManualExerciseChecking>(courseId, slideId, userIds);

			return userIds.ToDictionary(
				userId => userId,
				userId => Math.Max(quizScore.GetOrDefault(userId, 0), exerciseScore.GetOrDefault(userId, 0))
			);
		}

		public int GetAutomaticScoreForSlide(string courseId, Guid slideId, string userId)
		{
			var quizScore = GetUserScoreForSlide<AutomaticQuizChecking>(courseId, slideId, userId);
			var exerciseScore = GetUserScoreForSlide<AutomaticExerciseChecking>(courseId, slideId, userId);

			return Math.Max(quizScore, exerciseScore);
		}
		
		public Dictionary<string, int> GetAutomaticScoresForSlide(string courseId, Guid slideId, List<string> userIds)
		{
			var quizScore = GetUserScoresForSlide<AutomaticQuizChecking>(courseId, slideId, userIds);
			var exerciseScore = GetUserScoresForSlide<AutomaticExerciseChecking>(courseId, slideId, userIds);

			return userIds.ToDictionary(
				userId => userId,
				userId => Math.Max(quizScore.GetOrDefault(userId, 0), exerciseScore.GetOrDefault(userId, 0))
			);
		}
		
		#endregion

		public IQueryable<T> GetManualCheckingQueueAsync<T>(ManualCheckingQueueFilterOptions options) where T : AbstractManualSlideChecking
		{
			var query = db.Set<T>().Where(c => c.CourseId == options.CourseId && c.Timestamp >= options.From && c.Timestamp <= options.To);
			if (options.OnlyChecked.HasValue)
				query = options.OnlyChecked.Value ? query.Where(c => c.IsChecked) : query.Where(c => !c.IsChecked);
			if (options.SlidesIds != null)
				query = query.Where(c => options.SlidesIds.Contains(c.SlideId));
			if (options.UserIds != null)
			{
				if (options.IsUserIdsSupplement)
					query = query.Where(c => !options.UserIds.Contains(c.UserId));
				else
					query = query.Where(c => options.UserIds.Contains(c.UserId));
			}
			query = query.OrderByDescending(c => c.Timestamp);
			if (options.Count > 0)
				query = query.Take(options.Count);
			return query;
		}

		public T FindManualCheckingById<T>(int id) where T : AbstractManualSlideChecking
		{
			return db.Set<T>().Find(id);
		}
		
		public bool IsProhibitedToSendExerciseToManualChecking(string courseId, Guid slideId, string userId)
		{
			return GetSlideCheckingsByUser<ManualExerciseChecking>(courseId, slideId, userId).Any(c => c.ProhibitFurtherManualCheckings);
		}

		public Task LockManualChecking<T>(T checkingItem, string lockedById) where T : AbstractManualSlideChecking
		{
			checkingItem.LockedById = lockedById;
			checkingItem.LockedUntil = DateTime.Now.Add(TimeSpan.FromMinutes(30));
			return db.SaveChangesAsync();
		}

		public Task MarkManualCheckingAsChecked<T>(T queueItem, int score) where T : AbstractManualSlideChecking
		{
			queueItem.LockedBy = null;
			queueItem.LockedUntil = null;
			queueItem.IsChecked = true;
			queueItem.Score = score;
			return db.SaveChangesAsync();
		}

		public Task ProhibitFurtherExerciseManualChecking(ManualExerciseChecking checking)
		{
			checking.ProhibitFurtherManualCheckings = true;
			return db.SaveChangesAsync();
		}

		private async Task<ExerciseCodeReview> AddExerciseCodeReview([CanBeNull] UserExerciseSubmission submission, [CanBeNull] ManualExerciseChecking checking, string userId, int startLine, int startPosition, int finishLine, int finishPosition, string comment, bool setAddingTime)
		{
			var review = db.ExerciseCodeReviews.Add(new ExerciseCodeReview
			{
				AuthorId = userId,
				Comment = comment,
				ExerciseCheckingId = checking?.Id,
				SubmissionId = submission?.Id,
				StartLine = startLine,
				StartPosition = startPosition,
				FinishLine = finishLine,
				FinishPosition = finishPosition,
				AddingTime = setAddingTime ? DateTime.Now : ExerciseCodeReview.NullAddingTime,
			});

			await db.SaveChangesAsync().ConfigureAwait(false);

			/* Extract review from database to fill review.Author by EF's DynamicProxy */
			return await db.ExerciseCodeReviews.AsNoTracking().FirstOrDefaultAsync(r => r.Id == review.Entity.Id).ConfigureAwait(false);
		}
		
		public Task<ExerciseCodeReview> AddExerciseCodeReview(ManualExerciseChecking checking, string userId, int startLine, int startPosition, int finishLine, int finishPosition, string comment, bool setAddingTime=true)
		{
			return AddExerciseCodeReview(null, checking, userId, startLine, startPosition, finishLine, finishPosition, comment, setAddingTime);
		}

		public Task<ExerciseCodeReview> AddExerciseCodeReview(UserExerciseSubmission submission, string userId, int startLine, int startPosition, int finishLine, int finishPosition, string comment, bool setAddingTime=false)
		{
			return AddExerciseCodeReview(submission, null, userId, startLine, startPosition, finishLine, finishPosition, comment, setAddingTime);
		}

		public ExerciseCodeReview FindExerciseCodeReviewById(int reviewId)
		{
			return db.ExerciseCodeReviews.Find(reviewId);
		}

		public Task DeleteExerciseCodeReview(ExerciseCodeReview review)
		{
			review.IsDeleted = true;
			return db.SaveChangesAsync();
		}

		public Task UpdateExerciseCodeReview(ExerciseCodeReview review, string newComment)
		{
			review.Comment = newComment;
			return db.SaveChangesAsync();
		}

		public Dictionary<int, List<ExerciseCodeReview>> GetExerciseCodeReviewForCheckings(IEnumerable<int> checkingsIds)
		{
			return db.ExerciseCodeReviews
				.Where(r => r.ExerciseCheckingId.HasValue && checkingsIds.Contains(r.ExerciseCheckingId.Value) && !r.IsDeleted)
				.GroupBy(r => r.ExerciseCheckingId)
				.ToDictionary(g => g.Key.Value, g => g.ToList());
		}

		public List<string> GetTopUserReviewComments(string courseId, Guid slideId, string userId, int count)
		{
			return db.ExerciseCodeReviews.Include(r => r.ExerciseChecking)
				.Where(r => r.ExerciseChecking.CourseId == courseId &&
							r.ExerciseChecking.SlideId == slideId &&
							r.AuthorId == userId &&
							!r.HiddenFromTopComments &&
							!r.IsDeleted)
				.GroupBy(r => r.Comment)
				.OrderByDescending(g => g.Count())
				.ThenByDescending(g => g.Max(r => r.ExerciseChecking.Timestamp))
				.Take(count)
				.Select(g => g.Key)
				.ToList();
		}
		
		public List<string> GetTopOtherUsersReviewComments(string courseId, Guid slideId, string userId, int count, IEnumerable<string> excludeComments)
		{
			return db.ExerciseCodeReviews.Include(r => r.ExerciseChecking)
				.Where(r => r.ExerciseChecking.CourseId == courseId &&
							r.ExerciseChecking.SlideId == slideId &&
							! excludeComments.Contains(r.Comment) &&
							r.AuthorId != userId &&
							!r.HiddenFromTopComments &&
							!r.IsDeleted)
				.GroupBy(r => r.Comment)
				.OrderByDescending(g => g.Select(r => r.AuthorId).Distinct().Count())
				.ThenByDescending(g => g.Count())
				.ThenByDescending(g => g.Max(r => r.ExerciseChecking.Timestamp))
				.Take(count)
				.Select(g => g.Key)
				.ToList();
		}

		public Task HideFromTopCodeReviewComments(string courseId, Guid slideId, string userId, string comment)
		{
			var reviews = db.ExerciseCodeReviews.Include(r => r.ExerciseChecking)
				.Where(r => r.ExerciseChecking.CourseId == courseId &&
							r.ExerciseChecking.SlideId == slideId &&
							r.AuthorId == userId &&
							r.Comment == comment &&
							!r.IsDeleted);

			foreach (var review in reviews)
				review.HiddenFromTopComments = true;
			return db.SaveChangesAsync();
		}

		public List<ExerciseCodeReview> GetAllReviewComments(string courseId, Guid slideId)
		{
			return db.ExerciseCodeReviews.Where(
				r => r.ExerciseChecking.CourseId == courseId &&
					r.ExerciseChecking.SlideId == slideId &&
					!r.IsDeleted
			).ToList();
		}
	}
}