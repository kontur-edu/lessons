using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AntiPlagiarism.Api.Models;
using AntiPlagiarism.Web.Database.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AntiPlagiarism.Web.Database.Repos
{
	public interface ISnippetsRepo
	{
		Task<Snippet> AddSnippetAsync(int tokensCount, SnippetType type, int hash);
		Task<bool> IsSnippetExistsAsync(int tokensCount, SnippetType type, int hash);
		Task<SnippetOccurence> AddSnippetOccurenceAsync(Submission submission, Snippet snippet, int firstTokenIndex);
		Task<List<SnippetOccurence>> GetSnippetsOccurencesForSubmissionAsync(Submission submission, int maxCount, int authorsCountMinThreshold, int authorsCountMaxThreshold);
		Task<List<SnippetOccurence>> GetSnippetsOccurencesForSubmissionAsync(Submission submission);
		Task<Dictionary<int, SnippetStatistics>> GetSnippetsStatisticsAsync(int clientId, Guid taskId, IEnumerable<int> snippetsIds);
		Task<List<SnippetOccurence>> GetSnippetsOccurrencesAsync(int snippetId);
		Task<List<SnippetOccurence>> GetSnippetsOccurrencesAsync(int snippetId, Expression<Func<SnippetOccurence, bool>> filterFunction);
		Task<List<SnippetOccurence>> GetSnippetsOccurrencesAsync(IEnumerable<int> snippetIds, Expression<Func<SnippetOccurence, bool>> filterFunction);
		List<SnippetOccurence> GetSnippetsOccurrences(IEnumerable<int> snippetIds, Expression<Func<SnippetOccurence, bool>> filterFunction);
		List<int> GetSubmissionIdsWithSameSnippets(IEnumerable<int> snippetIds, Expression<Func<SnippetOccurence, bool>> filterFunction, int maxSubmissionsCount);
		Task RemoveSnippetsOccurrencesForTaskAsync(Guid taskId);
		Task<Snippet> GetOrAddSnippetAsync(Snippet snippet);
	}

	public class SnippetsRepo : ISnippetsRepo
	{
		private readonly AntiPlagiarismDb db;
		private readonly ILogger logger;

		public SnippetsRepo(AntiPlagiarismDb db, ILogger logger)
		{
			this.db = db;
			this.logger = logger;
		}

		public async Task<Snippet> AddSnippetAsync(int tokensCount, SnippetType type, int hash)
		{
			var snippet = new Snippet
			{
				TokensCount = tokensCount,
				SnippetType = type,
				Hash = hash
			};
			db.Snippets.Add(snippet);
			await db.SaveChangesAsync().ConfigureAwait(false);
			return snippet;
		}

		public Task<bool> IsSnippetExistsAsync(int tokensCount, SnippetType type, int hash)
		{
			return db.Snippets.AnyAsync(s => s.TokensCount == tokensCount && s.SnippetType == type && s.Hash == hash);
		}

		public async Task<SnippetOccurence> AddSnippetOccurenceAsync(Submission submission, Snippet snippet, int firstTokenIndex)
		{
			logger.Information($"Сохраняю в базу информацию о сниппете {snippet} в решении #{submission.Id} в позиции {firstTokenIndex}");
			logger.Information($"Ищу сниппет {snippet} в базе (или создаю новый)");
			var foundSnippet = await GetOrAddSnippetAsync(snippet).ConfigureAwait(false);
			logger.Information($"Сниппет в базе имеет номер {foundSnippet.Id}");
			var snippetOccurence = new SnippetOccurence
			{
				SubmissionId = submission.Id,
				Snippet = foundSnippet,
				FirstTokenIndex = firstTokenIndex,
			};
			logger.Information($"Добавляю в базу объект {snippetOccurence}");
			
			
			DisableAutoDetectChanges();
			
			/* ...and use non-async Add() here because of perfomance issues with async versions */
			db.SnippetsOccurences.Add(snippetOccurence);
			db.Entry(snippetOccurence).State = EntityState.Added;
			await db.SaveChangesAsync().ConfigureAwait(false);
			db.Entry(snippetOccurence).State = EntityState.Unchanged;
		
			EnableAutoDetectChanges();
			
			
			logger.Information($"Добавил. Пересчитываю статистику сниппета (количество авторов, у которых он встречается)");
			var snippetStatistics = await GetOrAddSnippetStatisticsAsync(foundSnippet, submission.TaskId, submission.ClientId);
			
			
			DisableAutoDetectChanges();
			
			logger.Information($"Старая статистика сниппета {foundSnippet}: {snippetStatistics}");
			/* Use non-async Add() here because of performance issues with async versions */
			snippetStatistics.AuthorsCount = db.SnippetsOccurences.Include(o => o.Submission)
				.Where(o => o.SnippetId == foundSnippet.Id &&
							o.Submission.ClientId == submission.ClientId &&
							o.Submission.TaskId == submission.TaskId)
				.Select(o => o.Submission.AuthorId)
				.Distinct()
				.Count();
			db.Entry(snippetStatistics).State = EntityState.Modified;
			logger.Information($"Количество авторов, у которых встречается сниппет {foundSnippet} — {snippetStatistics.AuthorsCount}");
			await db.SaveChangesAsync().ConfigureAwait(false);
			db.Entry(snippetStatistics).State = EntityState.Unchanged;
		
			EnableAutoDetectChanges();

			
			logger.Information($"Закончил сохранение в базу информации о сниппете {foundSnippet} в решении #{submission.Id} в позиции {firstTokenIndex}");
			return snippetOccurence;
		}

		private async Task<SnippetStatistics> GetOrAddSnippetStatisticsAsync(Snippet snippet, Guid taskId, int clientId)
		{
			DisableAutoDetectChanges();
			try
			{
				var foundStatistics = await db.SnippetsStatistics.SingleOrDefaultAsync(
					s => s.SnippetId == snippet.Id &&
						s.TaskId == taskId &&
						s.ClientId == clientId
				).ConfigureAwait(false);
				if (foundStatistics != null)
					return foundStatistics;

				var addedStatistics = db.SnippetsStatistics.Add(new SnippetStatistics
				{
					SnippetId = snippet.Id,
					ClientId = clientId,
					TaskId = taskId,
				});
				addedStatistics.State = EntityState.Added;
				
				try
				{
					await db.SaveChangesAsync().ConfigureAwait(false);
					addedStatistics.State = EntityState.Unchanged;
				}
				catch (DbUpdateException e)
				{
					logger.Warning(e, $"Возникла ошибка при добавлении объекта SnippetStatistics для сниппета {snippet}. Но это ничего, просто найду себе такой же в базе");
					/* It's ok: this statistics already exists */
					foundStatistics = await db.SnippetsStatistics.AsNoTracking().SingleOrDefaultAsync(
						s => s.SnippetId == snippet.Id &&
							s.TaskId == taskId &&
							s.ClientId == clientId
					).ConfigureAwait(false);
					if (foundStatistics != null)
						return foundStatistics;
					
					throw;				
				}

				return addedStatistics.Entity;
			}
			finally
			{
				EnableAutoDetectChanges();
			}
		}

		/* We stands with perfomance issue on EF Core: https://github.com/aspnet/EntityFrameworkCore/issues/11680
  		   So we decided to disable AutoDetectChangesEnabled temporary for some queries */
		private void DisableAutoDetectChanges()
		{
			logger.Debug("Выключаю AutoDetectChangesEnabled");
			db.DisableAutoDetectChanges();
		}

		private void EnableAutoDetectChanges()
		{
			logger.Debug("Включаю AutoDetectChangesEnabled обратно");
			db.EnableAutoDetectChanges();
		}

		public async Task<Snippet> GetOrAddSnippetAsync(Snippet snippet)
		{
			DisableAutoDetectChanges();

			try
			{
				var foundSnippet = await db.Snippets.SingleOrDefaultAsync(
					s => s.SnippetType == snippet.SnippetType
						&& s.TokensCount == snippet.TokensCount
						&& s.Hash == snippet.Hash
				).ConfigureAwait(false);

				if (foundSnippet != null)
					return foundSnippet;

				db.Snippets.Add(snippet);
				db.Entry(snippet).State = EntityState.Added;
				try
				{
					await db.SaveChangesAsync().ConfigureAwait(false);
					db.Entry(snippet).State = EntityState.Unchanged;
					return snippet;
				}
				catch (DbUpdateException e)
				{
					logger.Warning(e, $"Возникла ошибка при добавлении сниппета {snippet}. Но это ничего, просто найду себе такой же в базе");
					/* It's ok: this snippet already exists */
					foundSnippet = await db.Snippets.AsNoTracking().SingleOrDefaultAsync(
						s => s.SnippetType == snippet.SnippetType
							&& s.TokensCount == snippet.TokensCount
							&& s.Hash == snippet.Hash
					).ConfigureAwait(false);
					if (foundSnippet != null)
						return foundSnippet;

					throw;
				}
			}
			finally
			{
				EnableAutoDetectChanges();
			}
		}

		public async Task<List<SnippetOccurence>> GetSnippetsOccurencesForSubmissionAsync(Submission submission, int maxCount, int authorsCountMinThreshold, int authorsCountMaxThreshold)
		{
			if (maxCount == 0)
				return await db.SnippetsOccurences.Where(o => o.SubmissionId == submission.Id).ToListAsync().ConfigureAwait(false);
			
			var selectedSnippetsStatistics = db.SnippetsStatistics.Where(s => s.TaskId == submission.TaskId && s.ClientId == submission.ClientId);
			return (await db.SnippetsOccurences.Include(o => o.Snippet)
					.Join(selectedSnippetsStatistics, o => o.SnippetId, s => s.SnippetId, (occurence, statistics) => new { occurence, statistics })
					.Where(o => o.occurence.SubmissionId == submission.Id && o.statistics.AuthorsCount >= authorsCountMinThreshold && o.statistics.AuthorsCount <= authorsCountMaxThreshold)
					.OrderBy(o => o.statistics.AuthorsCount)
					.Take(maxCount)
					.ToListAsync()
					.ConfigureAwait(false))
				.Select(o => o.occurence)
				.ToList();
		}

		public Task<List<SnippetOccurence>> GetSnippetsOccurencesForSubmissionAsync(Submission submission)
		{
			return GetSnippetsOccurencesForSubmissionAsync(submission, 0, 0, int.MaxValue);
		}

		public Task<Dictionary<int, SnippetStatistics>> GetSnippetsStatisticsAsync(int clientId, Guid taskId, IEnumerable<int> snippetsIds)
		{
			return db.SnippetsStatistics
				.Where(s => s.ClientId == clientId && s.TaskId == taskId && snippetsIds.Contains(s.SnippetId))
				.ToDictionaryAsync(s => s.SnippetId);
		}

		public Task<List<SnippetOccurence>> GetSnippetsOccurrencesAsync(int snippetId)
		{
			return GetSnippetsOccurrencesAsync(snippetId, o => true);
		}

		public Task<List<SnippetOccurence>> GetSnippetsOccurrencesAsync(int snippetId, Expression<Func<SnippetOccurence, bool>> filterFunction)
		{
			return db.SnippetsOccurences.Include(o => o.Submission).Where(o => o.SnippetId == snippetId).Where(filterFunction).ToListAsync();
		}

		public Task<List<SnippetOccurence>> GetSnippetsOccurrencesAsync(IEnumerable<int> snippetIds, Expression<Func<SnippetOccurence, bool>> filterFunction)
		{
			return InternalGetSnippetsOccurences(snippetIds, filterFunction).ToListAsync();
		}

		public List<SnippetOccurence> GetSnippetsOccurrences(IEnumerable<int> snippetIds, Expression<Func<SnippetOccurence, bool>> filterFunction)
		{
			return InternalGetSnippetsOccurences(snippetIds, filterFunction).ToList();
		}

		public List<int> GetSubmissionIdsWithSameSnippets(IEnumerable<int> snippetIds, Expression<Func<SnippetOccurence, bool>> filterFunction, int maxSubmissionsCount)
		{
			var submissionsWithSnippetsCount = db.SnippetsOccurences
				.Where(o => snippetIds.Contains(o.SnippetId))
				.Where(filterFunction)
				.GroupBy(o => o.SubmissionId)
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Count())
				.ToList();
			
			if (!submissionsWithSnippetsCount.Any())
				return new List<int>();
			
			/* Sort by occurrences count descendants */
			submissionsWithSnippetsCount.Sort((first, second) => second.Value.CompareTo(first.Value));
			var maxOccurrencesCount = submissionsWithSnippetsCount[0].Value;
			
			/* Take at least maxSubmissionsCount submissions, but also take submissions while it's occurencesCount is greater than 0.9 * maxOccurencesCount */
			return submissionsWithSnippetsCount.TakeWhile((kvp, index) => index < maxSubmissionsCount || kvp.Value >= 0.9 * maxOccurrencesCount).Select(kvp => kvp.Key).ToList();
		}

		private IQueryable<SnippetOccurence> InternalGetSnippetsOccurences(IEnumerable<int> snippetIds, Expression<Func<SnippetOccurence, bool>> filterFunction)
		{
			return db.SnippetsOccurences.Include(o => o.Submission).Where(o => snippetIds.Contains(o.SnippetId)).Where(filterFunction);
		}

		public async Task RemoveSnippetsOccurrencesForTaskAsync(Guid taskId)
		{
			db.SnippetsOccurences.RemoveRange(
				await db.SnippetsOccurences.Include(o => o.Submission).Where(o => o.Submission.TaskId == taskId).ToListAsync().ConfigureAwait(false)
			);
		}

		public async Task<List<Snippet>> GetSnippetsForSubmission(Submission submission, int maxCount, int authorsCountThreshold)
		{
			return (await GetSnippetsOccurencesForSubmissionAsync(submission, maxCount, 0, authorsCountThreshold).ConfigureAwait(false)).Select(o => o.Snippet).ToList();
		}
	}

	internal static class DbErrors
	{
		/* Cannot insert duplicate key row in object, see https://msdn.microsoft.com/en-us/library/cc645728.aspx */
		public const int CanNotInsertDuplicateKeyRowInObject = 2601;
	}
}