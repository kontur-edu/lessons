﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntiPlagiarism.Web.Configuration;
using AntiPlagiarism.Web.Database.Models;
using AntiPlagiarism.Web.Database.Repos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vostok.Logging.Abstractions;
using Ulearn.Common;

namespace AntiPlagiarism.Web.CodeAnalyzing
{
	public class NewSubmissionHandler
	{
		private readonly IWorkQueueRepo workQueueRepo;
		private readonly ISubmissionsRepo submissionsRepo;
		private readonly ITasksRepo tasksRepo;
		private readonly ISnippetsRepo snippetsRepo;
		private readonly SubmissionSnippetsExtractor submissionSnippetsExtractor;
		private readonly IServiceScopeFactory serviceScopeFactory;
		private readonly AntiPlagiarismConfiguration configuration;
		private static ILog log => LogProvider.Get().ForContext(typeof(NewSubmissionHandler));

		public NewSubmissionHandler(
			ISubmissionsRepo submissionsRepo, ISnippetsRepo snippetsRepo, ITasksRepo tasksRepo, IWorkQueueRepo workQueueRepo,
			SubmissionSnippetsExtractor submissionSnippetsExtractor,
			IServiceScopeFactory serviceScopeFactory,
			IOptions<AntiPlagiarismConfiguration> configuration)
		{
			this.submissionsRepo = submissionsRepo;
			this.snippetsRepo = snippetsRepo;
			this.tasksRepo = tasksRepo;
			this.workQueueRepo = workQueueRepo;
			this.submissionSnippetsExtractor = submissionSnippetsExtractor;
			this.serviceScopeFactory = serviceScopeFactory;
			this.configuration = configuration.Value;
		}
		
		public async Task<bool> HandleNewSubmission()
		{
			var handledSubmission = await FuncUtils.TrySeveralTimesAsync(ExtractSnippetsFromSubmissionFromQueue, 3);
			if (handledSubmission == null)
				return false;
			try
			{
				if (await NeedToRecalculateTaskStatistics(handledSubmission.ClientId, handledSubmission.TaskId).ConfigureAwait(false))
					await CalculateTaskStatisticsParametersAsync(handledSubmission.ClientId, handledSubmission.TaskId).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				log.Error(ex, "Exception during CalculateTaskStatistics in HandleNewSubmission");
			}
			return true;
		}

		private async Task<Submission> ExtractSnippetsFromSubmissionFromQueue()
		{
			var queueItem = await workQueueRepo.TakeNoTracking(QueueIds.NewSubmissionsQueue).ConfigureAwait(false);
			if (queueItem == null)
				return null;
			var submissionId = int.Parse(queueItem.ItemId);
			var submission = (await submissionsRepo.GetSubmissionsByIdsAsync(new List<int> { submissionId }).ConfigureAwait(false)).First();
			await ExtractSnippetsFromSubmissionAsync(submission).ConfigureAwait(false);
			await workQueueRepo.Remove(queueItem.Id);
			return submission;
		}

		/* Определяет, пора ли пересчитывать параметры Mean и Deviation для заданной задачи.
		   В конфигурации для этого есть специальный параметр configuration.StatisticsAnalyzing.RecalculateStatisticsAfterSubmisionsCount.
		   Если он равен, например, 1000, то параметры будут пересчитываться после каждого тысячного решения по этой задаче.
		   Если решений пока меньше 1000, то параметры будут пересчитываться после 1-го, 2-го, 4-го, 8-го, 16-го, 32-го решения и так далее.
		 */
		private async Task<bool> NeedToRecalculateTaskStatistics(int clientId, Guid taskId)
		{
			var submissionsCount = await submissionsRepo.GetSubmissionsCountAsync(clientId, taskId).ConfigureAwait(false);
			var oldSubmissionsCount = (await tasksRepo.FindTaskStatisticsParametersAsync(taskId).ConfigureAwait(false))?.SubmissionsCount ?? 0;
			var recalculateStatisticsAfterSubmissionsCount = configuration.AntiPlagiarism.StatisticsAnalyzing.RecalculateStatisticsAfterSubmissionsCount;
			log.Info($"Определяю, надо ли пересчитать статистические параметры задачи (TaskStatisticsParameters, параметры Mean и Deviation), задача {taskId}. " +
									$"Старое количество решений {oldSubmissionsCount}, новое {submissionsCount}, параметр recalculateStatisticsAfterSubmisionsCount={recalculateStatisticsAfterSubmissionsCount}.");

			if (submissionsCount < recalculateStatisticsAfterSubmissionsCount)
				return submissionsCount >= 2 * oldSubmissionsCount;

			return submissionsCount - oldSubmissionsCount >= recalculateStatisticsAfterSubmissionsCount;
		}

		public async Task ExtractSnippetsFromSubmissionAsync(Submission submission)
		{
			foreach (var (firstTokenIndex, snippet) in submissionSnippetsExtractor.ExtractSnippetsFromSubmission(submission))
				await snippetsRepo.AddSnippetOccurenceAsync(submission, snippet, firstTokenIndex, configuration.AntiPlagiarism.SubmissionInfluenceLimitInMonths).ConfigureAwait(false);
		}

		/// <returns>List of weights (numbers from [0, 1)) used for calculating mean and deviation for this task</returns>
		public async Task<List<double>> CalculateTaskStatisticsParametersAsync(int clientId, Guid taskId)
		{
			/* Create local repo for preventing memory leaks */
			using (var scope = serviceScopeFactory.CreateScope())
			{
				var localSubmissionsRepo = scope.ServiceProvider.GetService<ISubmissionsRepo>();
				var tasksRepo = scope.ServiceProvider.GetService<ITasksRepo>();
				var statisticsParametersFinder = scope.ServiceProvider.GetService<StatisticsParametersFinder>();

				log.Info($"Пересчитываю статистические параметры задачи (TaskStatisticsParameters) по задаче {taskId}");
				var lastAuthorsIds = await localSubmissionsRepo.GetLastAuthorsByTaskAsync(clientId, taskId, configuration.AntiPlagiarism.StatisticsAnalyzing.CountOfLastAuthorsForCalculatingMeanAndDeviation).ConfigureAwait(false);
				var lastSubmissions = await localSubmissionsRepo.GetLastSubmissionsByAuthorsForTaskAsync(clientId, taskId, lastAuthorsIds).ConfigureAwait(false);
				var currentSubmissionsCount = await localSubmissionsRepo.GetSubmissionsCountAsync(clientId, taskId).ConfigureAwait(false);

				var (taskStatisticsSourceData, statisticsParameters) = await statisticsParametersFinder.FindStatisticsParametersAsync(lastSubmissions).ConfigureAwait(false);
				log.Info($"Новые статистические параметры задачи (TaskStatisticsParameters) по задаче {taskId}: Mean={statisticsParameters.Mean}, Deviation={statisticsParameters.Deviation}");
				statisticsParameters.TaskId = taskId;
				statisticsParameters.SubmissionsCount = currentSubmissionsCount;
				statisticsParameters.Timestamp = DateTime.Now;

				await tasksRepo.SaveTaskStatisticsParametersAsync(statisticsParameters, taskStatisticsSourceData).ConfigureAwait(false);

				return taskStatisticsSourceData.Select(d => d.Weight).ToList();
			}
		}
	}
}