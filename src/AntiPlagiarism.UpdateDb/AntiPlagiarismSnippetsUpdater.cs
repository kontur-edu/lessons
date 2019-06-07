using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntiPlagiarism.Web.CodeAnalyzing;
using AntiPlagiarism.Web.CodeAnalyzing.CSharp;
using AntiPlagiarism.Web.Database.Models;
using AntiPlagiarism.Web.Database.Repos;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace AntiPlagiarism.UpdateDb
{
	public class AntiPlagiarismSnippetsUpdater
	{
		private readonly SubmissionSnippetsExtractor submissionSnippetsExtractor;
		private readonly CodeUnitsExtractor codeUnitsExtractor;
		private readonly IServiceScopeFactory serviceScopeFactory;
		private readonly ILogger logger;

		public AntiPlagiarismSnippetsUpdater(
			SubmissionSnippetsExtractor submissionSnippetsExtractor,
			CodeUnitsExtractor codeUnitsExtractor,
			IServiceScopeFactory serviceScopeFactory,
			ILogger logger)
		{
			this.submissionSnippetsExtractor = submissionSnippetsExtractor;
			this.codeUnitsExtractor = codeUnitsExtractor;
			this.serviceScopeFactory = serviceScopeFactory;
			this.logger = logger;
		}

		public async Task UpdateAsync(int startFromIndex=0, bool updateOnlyTokensCount=false)
		{
			logger.Information("Начинаю обновлять информацию о сниппетах в базе данных");

			const int maxSubmissionsCount = 1000;
			while (true)
			{
				int lastSubmissionId;
				using (var scope = serviceScopeFactory.CreateScope())
				{
					/* Re-create submissions repo each time for preventing memory leaks */					
					var submissionsRepo = scope.ServiceProvider.GetService<ISubmissionsRepo>();

					var submissions = await submissionsRepo.GetSubmissionsAsync(startFromIndex, maxSubmissionsCount);
					if (submissions.Count == 0)
						break;

					var snippetsRepo = scope.ServiceProvider.GetService<ISnippetsRepo>();

					var firstSubmissionId = submissions.First().Id;
					lastSubmissionId = submissions.Last().Id;
					logger.Information($"Получил {submissions.Count} следующих решений из базы данных. Идентификаторы решений от {firstSubmissionId} до {lastSubmissionId}");

					foreach (var submission in submissions)
					{
						await submissionsRepo.UpdateSubmissionTokensCountAsync(submission, GetTokensCount(submission.ProgramText));
						if (updateOnlyTokensCount)
							continue;
						
						try
						{
							await UpdateSnippetsFromSubmissionAsync(snippetsRepo, submission).ConfigureAwait(false);
						}
						catch (Exception e)
						{
							logger.Error(e, $"Ошибка при обновлении списка сниппетов решения #{submission.Id}. Продолжаю работу со следующего решения");
						}
					}
				}

				startFromIndex = lastSubmissionId + 1;
				
				logger.Information("Запускаю сборку мусора");
				logger.Information($"Потребление памяти до сборки мусора: {GC.GetTotalMemory(false) / 1024}Кб. GC's Gen0: {GC.CollectionCount(0)} Gen1: {GC.CollectionCount(1)} Gen2: {GC.CollectionCount(2)}");
				GC.Collect();
				logger.Information($"Потребление памяти после сборки мусора: {GC.GetTotalMemory(false) / 1024}Кб. GC's Gen0: {GC.CollectionCount(0)} Gen1: {GC.CollectionCount(1)} Gen2: {GC.CollectionCount(2)}");
			}
			
			logger.Information("AntiPlagiarismSnippetsUpdater закончил свою работу");
		}

		private async Task UpdateSnippetsFromSubmissionAsync(ISnippetsRepo snippetsRepo, Submission submission)
		{
			var occurences = new HashSet<Tuple<int, int>>(
				(await snippetsRepo.GetSnippetsOccurencesForSubmissionAsync(submission).ConfigureAwait(false))
				.Select(o => Tuple.Create(o.SnippetId, o.FirstTokenIndex))
			);
			
			foreach (var (firstTokenIndex, snippet) in submissionSnippetsExtractor.ExtractSnippetsFromSubmission(submission))
			{
				var foundSnippet = await snippetsRepo.GetOrAddSnippetAsync(snippet);
				if (!occurences.Contains(Tuple.Create(foundSnippet.Id, firstTokenIndex)))
				{
					logger.Information($"Информация о сниппете #{foundSnippet.Id} в решении #{submission.Id} не найдена, добавляю");
					try
					{
						await snippetsRepo.AddSnippetOccurenceAsync(submission, foundSnippet, firstTokenIndex);
					}
					catch (Exception e)
					{
						logger.Error(e, $"Ошибка при добавлении сниппета #{foundSnippet.Id} в решении #{submission.Id}");
					}
				}
			}
		}
		
		private int GetTokensCount(string code)
		{
			var codeUnits = codeUnitsExtractor.Extract(code);
			return codeUnits.Select(u => u.Tokens.Count).Sum();
		}
	}
}