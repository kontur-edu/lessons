﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using AntiPlagiarism.Api;
using AntiPlagiarism.Api.Models.Parameters;
using Database;
using Database.DataContexts;
using Database.Models;
using log4net;
using LtiLibrary.Core.Extensions;
using Metrics;
using RunCsJob.Api;
using Serilog;
using Telegram.Bot.Types.Enums;
using uLearn.Web.AntiPlagiarismUsage;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Telegram;
using XQueue;
using XQueue.Models;

namespace uLearn.Web.Controllers
{
	public class RunnerController : ApiController
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(RunnerController));

		private readonly UserSolutionsRepo userSolutionsRepo;
		private readonly SlideCheckingsRepo slideCheckingsRepo;
		private readonly VisitsRepo visitsRepo;
		private readonly GroupsRepo groupsRepo;
		private readonly UsersRepo usersRepo;
		private readonly ULearnDb db;
		private readonly CourseManager courseManager;
		private readonly MetricSender metricSender;

		private static readonly List<IResultObserver> resultObserveres = new List<IResultObserver>
		{
			new XQueueResultObserver(),
			new SandboxErrorsResultObserver(),
			new AntiPlagiarismResultObserver(),
		};

		public RunnerController(ULearnDb db, CourseManager courseManager)
		{
			this.db = db;
			this.courseManager = courseManager;
			userSolutionsRepo = new UserSolutionsRepo(db, courseManager);
			slideCheckingsRepo = new SlideCheckingsRepo(db);
			visitsRepo = new VisitsRepo(db);
			groupsRepo = new GroupsRepo(db, courseManager);
			usersRepo = new UsersRepo(db);
			metricSender = new MetricSender(ApplicationConfiguration.Read<UlearnConfiguration>().GraphiteServiceName);
		}

		public RunnerController()
			: this(new ULearnDb(), WebCourseManager.Instance)
		{
		}

		[System.Web.Http.HttpGet]
		[System.Web.Http.Route("GetSubmissions")]
		public async Task<List<RunnerSubmission>> GetSubmissions([FromUri] string token, [FromUri] string language, [FromUri] string agent = "")
		{
			CheckRunner(token);			
			
			if (!LanguageHelpers.TryParseByName(language, out var submissionLanguage))
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));
			
			var sw = Stopwatch.StartNew();
			while (true)
			{
				var repo = new UserSolutionsRepo(new ULearnDb(), courseManager);
				var submission = await repo.GetUnhandledSubmission(agent, submissionLanguage).ConfigureAwait(false);
				if (submission != null || sw.Elapsed > TimeSpan.FromSeconds(15))
				{
					if (submission != null)
						log.Info($"Отдаю на проверку решение: [{submission.Id}], агент {agent}, только сначала соберу их");
					else
						return new List<RunnerSubmission>();

					var builtSubmissions = new List<RunnerSubmission> { ToRunnerSubmission(submission) };
					log.Info($"Собрал решения: [{submission.Id}], отдаю их агенту {agent}");
					return builtSubmissions;
				}

				await repo.WaitAnyUnhandledSubmissions(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
			}
		}

		private RunnerSubmission ToRunnerSubmission(UserExerciseSubmission submission)
		{
			if (submission.IsWebSubmission)
			{
				return new FileRunnerSubmission
				{
					Id = submission.Id.ToString(),
					Code = submission.SolutionCode.Text,
					Input = "",
					NeedRun = true
				};
			}

			log.Info($"Собираю для отправки в RunCsJob решение {submission.Id}");

			var exerciseSlide = courseManager.FindCourse(submission.CourseId)?.FindSlideById(submission.SlideId) as ExerciseSlide;
			if (exerciseSlide == null)
				return new FileRunnerSubmission
				{
					Id = submission.Id.ToString(),
					Code = "// no slide anymore",
					Input = "",
					NeedRun = true
				};

			log.Info($"Ожидаю, если курс {submission.CourseId} заблокирован");
			courseManager.WaitWhileCourseIsLocked(submission.CourseId);
			log.Info($"Курс {submission.CourseId} разблокирован");

			return exerciseSlide.Exercise.CreateSubmission(
				submission.Id.ToString(),
				submission.SolutionCode.Text
			);
		}

		[System.Web.Http.HttpPost]
		[System.Web.Http.Route("PostResults")]
		public async Task PostResults([FromUri] string token, [FromUri] string agent, List<RunningResults> results)
		{
			if (!ModelState.IsValid)
			{
				var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
				log.Error($"Не могу принять от RunCsJob результаты проверки решений, ошибки: {string.Join(", ", errors)}");
				throw new HttpResponseException(HttpStatusCode.BadRequest);
			}
			CheckRunner(token);
			log.Info($"Получил от RunCsJob результаты проверки решений: [{string.Join(", ", results.Select(r => r.Id))}] от агента {agent}");

			foreach (var result in results)
				await FuncUtils.TrySeveralTimesAsync(() => userSolutionsRepo.SaveResult(result, 
					submission => BaseExerciseController.SendToReviewAndUpdateScore(submission, courseManager, slideCheckingsRepo, groupsRepo, visitsRepo, metricSender, false)
				), 3).ConfigureAwait(false);

			var submissionsByIds = userSolutionsRepo
				.FindSubmissionsByIds(results.Select(result => result.Id).ToList())
				.ToDictionary(s => s.Id.ToString());

			foreach (var result in results)
			{
				if (!submissionsByIds.ContainsKey(result.Id))
					continue;
				await SendResultToObservers(submissionsByIds[result.Id], result).ConfigureAwait(false);
			}
		}
		

		private Task SendResultToObservers(UserExerciseSubmission submission, RunningResults result)
		{
			var tasks = resultObserveres.Select(o => o.ProcessResult(db, submission, result));
			return Task.WhenAll(tasks);
		}

		private void CheckRunner(string token)
		{
			var expectedToken = ConfigurationManager.AppSettings["runnerToken"];
			if (expectedToken != token)
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
		}
	}

	public interface IResultObserver
	{
		Task ProcessResult(ULearnDb db, UserExerciseSubmission submission, RunningResults result);
	}

	public class XQueueResultObserver : IResultObserver
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(XQueueResultObserver));

		public async Task ProcessResult(ULearnDb db, UserExerciseSubmission submission, RunningResults result)
		{
			var courseManager = WebCourseManager.Instance;

			var xQueueRepo = new XQueueRepo(db, courseManager);
			var xQueueSubmission = xQueueRepo.FindXQueueSubmission(submission);
			if (xQueueSubmission == null)
				return;
				
			var watcher = xQueueSubmission.Watcher;
			var client = new XQueueClient(watcher.BaseUrl, watcher.UserName, watcher.Password);
			await client.Login().ConfigureAwait(false);
			if (await SendSubmissionResultsToQueue(client, xQueueSubmission).ConfigureAwait(false))
				await xQueueRepo.MarkXQueueSubmissionThatResultIsSent(xQueueSubmission).ConfigureAwait(false);
		}

		private Task<bool> SendSubmissionResultsToQueue(XQueueClient client, XQueueExerciseSubmission submission)
		{
			return FuncUtils.TrySeveralTimesAsync(() => TrySendSubmissionResultsToQueue(client, submission), 5, () => Task.Delay(TimeSpan.FromMilliseconds(1)));
		}

		private async Task<bool> TrySendSubmissionResultsToQueue(XQueueClient client, XQueueExerciseSubmission submission)
		{
			var checking = submission.Submission.AutomaticChecking;

			var courseManager = WebCourseManager.Instance;
			var slide = courseManager.FindCourse(checking.CourseId)?.FindSlideById(checking.SlideId) as ExerciseSlide;
			if (slide == null)
			{
				log.Warn($"Can't find exercise slide {checking.SlideId} in course {checking.CourseId}. Exit");
				return false;
			}

			var score = (double)checking.Score / slide.Scoring.PassedTestsScore;
			if (score > 1)
				score = 1;

			var message = checking.IsCompilationError ? checking.CompilationError.Text : checking.Output.Text;
			return await client.PutResult(new XQueueResult
			{
				header = submission.XQueueHeader,
				Body = new XQueueResultBody
				{
					IsCorrect = checking.IsRightAnswer,
					Message = message,
					Score = score,
				}
			}).ConfigureAwait(false);
		}
	}

	public class SandboxErrorsResultObserver : IResultObserver
	{
		private static readonly ErrorsBot bot = new ErrorsBot();

		public async Task ProcessResult(ULearnDb db, UserExerciseSubmission submission, RunningResults result)
		{
			/* Ignore all verdicts except SandboxError */
			if (result.Verdict != Verdict.SandboxError)
				return;

			var output = result.Output;
			await bot.PostToChannelAsync(
				$"<b>Решение #{submission.Id} не запустилось в песочнице (SandboxError).</b>\n" +
				(string.IsNullOrEmpty(output) ? "" : $"Вывод:\n<pre>{output.EscapeHtml()}</pre>"), 
				ParseMode.Html
			).ConfigureAwait(false);
		}
	}

	public class AntiPlagiarismResultObserver : IResultObserver
	{
		private static readonly IAntiPlagiarismClient antiPlagiarismClient;
		private static readonly ILog log = LogManager.GetLogger(typeof(AntiPlagiarismResultObserver));
		private static readonly bool isEnabled;

		static AntiPlagiarismResultObserver()
		{
			isEnabled = Convert.ToBoolean(WebConfigurationManager.AppSettings["ulearn.antiplagiarism.enabled"] ?? "false");
			if (!isEnabled)
				return;
			
			var serilogLogger = new LoggerConfiguration().WriteTo.Log4Net().CreateLogger();
			var antiPlagiarismEndpointUrl = WebConfigurationManager.AppSettings["ulearn.antiplagiarism.endpoint"];
			var antiPlagiarismToken = WebConfigurationManager.AppSettings["ulearn.antiplagiarism.token"];
			antiPlagiarismClient = new AntiPlagiarismClient(antiPlagiarismEndpointUrl, antiPlagiarismToken, serilogLogger);
		}
		
		public async Task ProcessResult(ULearnDb db, UserExerciseSubmission submission, RunningResults result)
		{
			if (!isEnabled)
				return;
			
			if (result.Verdict != Verdict.Ok)
				return;

			/* Send to antiplagiarism service only accepted submissions */
			var checking = submission.AutomaticChecking;
			if (!checking.IsRightAnswer)
				return;

			var parameters = new AddSubmissionParameters
			{
				TaskId = submission.SlideId,
				Language = AntiPlagiarism.Api.Models.Language.CSharp,
				Code = submission.SolutionCode.Text,
				AuthorId = Guid.Parse(submission.UserId),
				AdditionalInfo = new AntiPlagiarismAdditionalInfo { SubmissionId = submission.Id }.ToJsonString(),
			};
			var antiPlagiarismResult = await antiPlagiarismClient.AddSubmissionAsync(parameters).ConfigureAwait(false);
			
			log.Info($"Получил ответ от сервиса антиплагиата: {antiPlagiarismResult}");
			
			var userSolutionsRepo = new UserSolutionsRepo(db, WebCourseManager.Instance);
			await userSolutionsRepo.SetAntiPlagiarismSubmissionId(submission, antiPlagiarismResult.SubmissionId).ConfigureAwait(false);
		}
	}
}