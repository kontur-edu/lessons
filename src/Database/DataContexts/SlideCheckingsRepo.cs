using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Database.Extensions;
using Database.Models;
using JetBrains.Annotations;
using log4net;
using Ulearn.Common.Extensions;
using Z.EntityFramework.Plus;

namespace Database.DataContexts
{
	public class SlideCheckingsRepo
	{
		private readonly ULearnDb db;
		private readonly ILog log = LogManager.GetLogger(typeof(SlideCheckingsRepo));

		public SlideCheckingsRepo(ULearnDb db)
		{
			this.db = db;
		}

		public async Task<ManualQuizChecking> AddManualQuizChecking(UserQuizSubmission submission, string courseId, Guid slideId, string userId)
		{
			var manualChecking = new ManualQuizChecking
			{
				Submission = submission,
				CourseId = courseId,
				SlideId = slideId,
				UserId = userId,
				Timestamp = DateTime.Now,
			};
			db.ManualQuizCheckings.Add(manualChecking);
			await db.SaveChangesAsync().ConfigureAwait(false);
			
			return manualChecking;
		}

		public async Task<AutomaticQuizChecking> AddAutomaticQuizChecking(UserQuizSubmission submission, string courseId, Guid slideId, string userId, int automaticScore)
		{
			var automaticChecking = new AutomaticQuizChecking
			{
				Submission = submission,
				CourseId = courseId,
				SlideId = slideId,
				UserId = userId,
				Timestamp = DateTime.Now,
				Score = automaticScore,
			};
			db.AutomaticQuizCheckings.Add(automaticChecking);
			await db.SaveChangesAsync().ConfigureAwait(false);

			return automaticChecking;
		}

		public IEnumerable<ManualExerciseChecking> GetUsersPassedManualExerciseCheckings(string courseId, string userId)
		{
			return db.ManualExerciseCheckings.Where(c => c.CourseId == courseId && c.UserId == userId && c.IsChecked).DistinctBy(c => c.SlideId);
		}
		
		public bool HasManualExerciseChecking(string courseId, Guid slideId, string userId, int submissionId)
		{
			return db.ManualExerciseCheckings.Any(c => c.CourseId == courseId && c.UserId == userId && c.SlideId == slideId && c.SubmissionId == submissionId);
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

		public async Task RemoveWaitingManualCheckings<T>(string courseId, Guid slideId, string userId, bool startTransaction = true) where T : AbstractManualSlideChecking
		{
			using (var transaction = startTransaction ? db.Database.BeginTransaction() : null)
			{
				var checkings = GetSlideCheckingsByUser<T>(courseId, slideId, userId, noTracking: false)
					.AsEnumerable()
					.Where(c => !c.IsChecked && !c.IsLocked)
					.ToList();
				foreach (var checking in checkings)
				{
					checking.PreRemove(db);
					db.Set<T>().Remove(checking);
				}

				await db.SaveChangesAsync().ConfigureAwait(false);
				transaction?.Commit();
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

		
		public IEnumerable<T> GetManualCheckingQueue<T>(ManualCheckingQueueFilterOptions options) where T : AbstractManualSlideChecking
		{
			var query = db.Set<T>().Where(c => c.CourseId == options.CourseId);
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

		public async Task LockManualChecking<T>(T checkingItem, string lockedById) where T : AbstractManualSlideChecking
		{
			checkingItem.LockedById = lockedById;
			checkingItem.LockedUntil = DateTime.Now.Add(TimeSpan.FromMinutes(30));
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task MarkManualCheckingAsChecked<T>(T queueItem, int score) where T : AbstractManualSlideChecking
		{
			queueItem.LockedBy = null;
			queueItem.LockedUntil = null;
			queueItem.IsChecked = true;
			queueItem.Score = score;
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task ProhibitFurtherExerciseManualChecking(ManualExerciseChecking checking)
		{
			checking.ProhibitFurtherManualCheckings = true;
			await db.SaveChangesAsync().ConfigureAwait(false);
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
			return db.ExerciseCodeReviews.AsNoTracking().FirstOrDefault(r => r.Id == review.Id);
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

		public async Task DeleteExerciseCodeReview(ExerciseCodeReview review)
		{
			review.IsDeleted = true;
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task UpdateExerciseCodeReview(ExerciseCodeReview review, string newComment)
		{
			review.Comment = newComment;
			await db.SaveChangesAsync().ConfigureAwait(false);
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
			var sw = Stopwatch.StartNew();
			var result = db.ExerciseCodeReviews.Include(r => r.ExerciseChecking)
				.Where(r => r.ExerciseChecking.CourseId == courseId &&
							r.ExerciseChecking.SlideId == slideId &&
							r.AuthorId == userId &&
							!r.HiddenFromTopComments &&
							!r.IsDeleted)
				.ToList()
				.GroupBy(r => r.Comment)
				.OrderByDescending(g => g.Count())
				.ThenByDescending(g => g.Max(r => r.ExerciseChecking.Timestamp))
				.Take(count)
				.Select(g => g.Key)
				.ToList();
			log.Info("GetTopUserReviewComments " + sw.ElapsedMilliseconds + " ms");
			return result;
		}

		public List<string> GetTopOtherUsersReviewComments(string courseId, Guid slideId, string userId, int count, List<string> excludeComments)
		{
			var sw = Stopwatch.StartNew();
			var excludeCommentsSet = excludeComments.ToImmutableHashSet();
			var result = db.ExerciseCodeReviews.Include(r => r.ExerciseChecking)
				.Where(r => r.ExerciseChecking.CourseId == courseId &&
							r.ExerciseChecking.SlideId == slideId &&
							r.AuthorId != userId &&
							!r.HiddenFromTopComments &&
							!r.IsDeleted)
				.GroupBy(r => r.Comment)
				.OrderByDescending(g => g.Count())
				.Take(count * 2)
				.Select(g => g.Key)
				.ToList()
				.Where(c => !excludeCommentsSet.Contains(c))
				.Take(count)
				.ToList();
			log.Info("GetTopOtherUsersReviewComments " + sw.ElapsedMilliseconds + " ms");
			return result;
		}

		public async Task HideFromTopCodeReviewComments(string courseId, Guid slideId, string userId, string comment)
		{
			var reviews = db.ExerciseCodeReviews.Include(r => r.ExerciseChecking)
				.Where(r => r.ExerciseChecking.CourseId == courseId &&
							r.ExerciseChecking.SlideId == slideId &&
							r.AuthorId == userId &&
							r.Comment == comment &&
							!r.IsDeleted);

			foreach (var review in reviews)
				review.HiddenFromTopComments = true;
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public List<ExerciseCodeReview> GetLastYearReviewComments(string courseId, Guid slideId)
		{
			var sw = Stopwatch.StartNew();
			var lastYear = DateTime.Today.AddYears(-1);
			var result = db.ExerciseCodeReviews.Where(
				r => r.ExerciseChecking.CourseId == courseId &&
					r.ExerciseChecking.SlideId == slideId &&
					!r.IsDeleted &&
					r.ExerciseChecking.Timestamp > lastYear
			).ToList();
			log.Info("GetLastYearReviewComments " + sw.ElapsedMilliseconds + " ms");
			return result;
		}

		public async Task<ExerciseCodeReviewComment> AddExerciseCodeReviewComment(string authorId, int reviewId, string text)
		{
			var codeReviewComment = new ExerciseCodeReviewComment
			{
				AuthorId = authorId,
				ReviewId = reviewId,
				Text = text,
				IsDeleted = false,
				AddingTime = DateTime.Now,
			};

			db.ExerciseCodeReviewComments.Add(codeReviewComment);
			await db.SaveChangesAsync().ConfigureAwait(false);

			/* Extract review from database to fill review.Author by EF's DynamicProxy */
			return db.ExerciseCodeReviewComments.AsNoTracking().FirstOrDefault(r => r.Id == codeReviewComment.Id);
		}

		public ExerciseCodeReviewComment FindExerciseCodeReviewCommentById(int commentId)
		{
			return db.ExerciseCodeReviewComments.Find(commentId);
		}
		
		public async Task DeleteExerciseCodeReviewComment(ExerciseCodeReviewComment comment)
		{
			comment.IsDeleted = true;
			await db.SaveChangesAsync().ConfigureAwait(false);
		}
	}
}