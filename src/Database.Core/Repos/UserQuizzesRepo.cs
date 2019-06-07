using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Models.Quizzes;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Slides.Quizzes.Blocks;

namespace Database.Repos
{
	/* This repo is fully migrated to .NET Core and EF Core */
	/* TODO (andgein): BE CAREFUL. This repo is not tested absolutely, it was just copied from legacy Database/UserQuizzesRepo and refactored. */
	public class UserQuizzesRepo : IUserQuizzesRepo
	{
		private readonly UlearnDb db;

		public UserQuizzesRepo(UlearnDb db)
		{
			this.db = db;
		}

		public Task<UserQuizSubmission> FindLastUserSubmissionAsync(string courseId, Guid slideId, string userId)
		{
			return db.UserQuizSubmissions.Where(s => s.CourseId == courseId && s.UserId == userId && s.SlideId == slideId).OrderByDescending(s => s.Timestamp).FirstOrDefaultAsync();
		}		
		
		public async Task<UserQuizSubmission> AddSubmissionAsync(string courseId, Guid slideId, string userId, DateTime timestamp)
		{
			var submission = new UserQuizSubmission
			{
				CourseId = courseId,
				SlideId = slideId,
				UserId = userId,
				Timestamp = timestamp,
			};
			db.UserQuizSubmissions.Add(submission);
			await db.SaveChangesAsync().ConfigureAwait(false);
			return submission;
		}
		
		public async Task<UserQuizAnswer> AddUserQuizAnswerAsync(int submissionId, bool isRightAnswer, string blockId, string itemId, string text, int quizBlockScore, int quizBlockMaxScore)
		{
			var answer = new UserQuizAnswer
			{
				SubmissionId = submissionId,
				IsRightAnswer = isRightAnswer,
				BlockId = blockId,
				ItemId = itemId,
				Text = text,
				QuizBlockScore = quizBlockScore,
				QuizBlockMaxScore = quizBlockMaxScore
			};
			db.UserQuizAnswers.Add(answer);
			await db.SaveChangesAsync().ConfigureAwait(false);
			return answer;
		}

		public Task<bool> IsWaitingForManualCheckAsync(string courseId, Guid slideId, string userId)
		{
			return db.ManualQuizCheckings.AnyAsync(c => c.CourseId == courseId && c.SlideId == slideId && c.UserId == userId && !c.IsChecked);
		}

		public Task<int> GetUsedAttemptsCountAsync(string courseId, string userId, Guid slideId)
		{
			return db.UserQuizSubmissions
				.CountAsync(s => s.CourseId == courseId && s.UserId == userId && s.SlideId == slideId);
		}

		public async Task<HashSet<Guid>> GetPassedSlideIdsAsync(string courseId, string userId)
		{
			return new HashSet<Guid>(await db.UserQuizSubmissions.Where(x => x.CourseId == courseId && x.UserId == userId).Select(x => x.SlideId).Distinct().ToListAsync().ConfigureAwait(false));
		}

		public async Task<HashSet<Guid>> GetPassedSlideIdsWithMaximumScoreAsync(string courseId, string userId)
		{
			var passedQuizzes = await GetPassedSlideIdsAsync(courseId, userId).ConfigureAwait(false);
			var notScoredMaximumSlides = await db.UserQuizAnswers
				.Include(a => a.Submission)
				.Where(x => x.Submission.CourseId == courseId && x.Submission.UserId == userId && x.QuizBlockScore != x.QuizBlockMaxScore)
				.Select(x => x.Submission.SlideId)
				.Distinct()
				.ToListAsync()
				.ConfigureAwait(false);
			passedQuizzes.ExceptWith(notScoredMaximumSlides);
			return passedQuizzes;
		}

		public async Task<Dictionary<string, List<UserQuizAnswer>>> GetAnswersForShowingOnSlideAsync(string courseId, QuizSlide slide, string userId, UserQuizSubmission submission = null)
		{
			if (slide == null)
				return null;
			
			if (submission == null)
				submission = await FindLastUserSubmissionAsync(courseId, slide.Id, userId).ConfigureAwait(false);
			
			var answer = new Dictionary<string, List<UserQuizAnswer>>();
			foreach (var block in slide.Blocks.OfType<AbstractQuestionBlock>())
			{
				if (submission != null)
				{
					var ans = await db.UserQuizAnswers
						.Where(q => q.SubmissionId == submission.Id && q.BlockId == block.Id)
						.OrderBy(x => x.Id)
						.ToListAsync()
						.ConfigureAwait(false);

					answer[block.Id] = ans;
				}
				else
					answer[block.Id] = new List<UserQuizAnswer>();
				
			}
			return answer;
		}
		
		public async Task<Dictionary<string, int>> GetUserScoresAsync(string courseId, Guid slideId, string userId, UserQuizSubmission submission = null)
		{
			if (submission == null)
			{
				submission = await FindLastUserSubmissionAsync(courseId, slideId, userId).ConfigureAwait(false);
				if (submission == null)
					return new Dictionary<string, int>();
			}

			var submissionAnswers = await db.UserQuizAnswers.Where(q => q.SubmissionId == submission.Id).ToListAsync().ConfigureAwait(false);
			return submissionAnswers
				.DistinctBy(q => q.BlockId)
				.ToDictionary(q => q.BlockId, q => q.QuizBlockScore);
		}

		public async Task<bool> IsQuizScoredMaximumAsync(string courseId, Guid slideId, string userId)
		{
			var submission = await FindLastUserSubmissionAsync(courseId, slideId, userId).ConfigureAwait(false);
			if (submission == null)
				return false;
			
			return await db.UserQuizAnswers
				.Where(q => q.SubmissionId == submission.Id)
				.AllAsync(q => q.QuizBlockScore == q.QuizBlockMaxScore)
				.ConfigureAwait(false);
		}

		public async Task SetScoreForQuizBlock(int submissionId, string blockId, int score)
		{
			var answers = await db.UserQuizAnswers
				.Where(q => q.SubmissionId == submissionId && q.BlockId == blockId)
				.ToListAsync()
				.ConfigureAwait(false);
			
			answers.ForEach(q => q.QuizBlockScore = score);
			
			await db.SaveChangesAsync().ConfigureAwait(false);
		}
		
		public async Task<Dictionary<string, int>> GetAnswersFrequencyForChoiceBlock(string courseId, Guid slideId, string quizId)
		{
			var answers = db.UserQuizAnswers.Include(q => q.SubmissionId).Where(q => q.Submission.CourseId == courseId && q.Submission.SlideId == slideId && q.BlockId == quizId);
			var totalTries = await answers.Select(q => new { q.Submission.UserId, q.Submission.Timestamp }).Distinct().CountAsync().ConfigureAwait(false);
			
			/* Due to performance issues don't call GroupBy().ToDictionary[Async](), call GroupBy().Select().ToDictionary() instead.
			   See http://code-ninja.org/blog/2014/07/24/entity-framework-never-call-groupby-todictionary/ for details */
			return await answers
				.GroupBy(q => q.ItemId)
				.Select(g => new { g.Key, Count = g.Count() })
				.ToDictionaryAsync(p => p.Key, p => p.Count.PercentsOf(totalTries))
				.ConfigureAwait(false);
		}
	}
}