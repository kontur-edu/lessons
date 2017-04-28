using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using log4net;
using RunCsJob.Api;
using uLearn.Web.DataContexts;
using uLearn.Web.Models;

namespace uLearn.Web.Controllers
{
	public class RunnerController : ApiController
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(RunnerController));

		private readonly UserSolutionsRepo userSolutionsRepo = new UserSolutionsRepo(new ULearnDb());
		private readonly CourseManager courseManager = WebCourseManager.Instance;

		[HttpGet]
		[Route("GetSubmissions")]
		public async Task<List<RunnerSubmission>> GetSubmissions([FromUri] string token, [FromUri] int count)
		{
			CheckRunner(token);
			var sw = Stopwatch.StartNew();
			while (true)
			{
				var repo = new UserSolutionsRepo(new ULearnDb());
				var submissions = await repo.GetUnhandledSubmissions(count);
				if (submissions.Any() || sw.Elapsed > TimeSpan.FromSeconds(15))
				{
					if (submissions.Any())
						log.Info($"Отдаю на проверку решения: [{string.Join(",", submissions.Select(c => c.Id))}]");
					return submissions.Select(ToRunnerSubmission).ToList();
				}
				await repo.WaitAnyUnhandledSubmissions(TimeSpan.FromSeconds(10));
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
			var exerciseSlide = courseManager.FindCourse(submission.CourseId)?.FindSlideById(submission.SlideId) as ExerciseSlide;
			if (exerciseSlide == null)
				return new FileRunnerSubmission
				{
					Id = submission.Id.ToString(),
					Code = "// no slide anymore",
					Input = "",
					NeedRun = true
				};

			courseManager.WaitWhileCourseIsLocked(submission.CourseId);

			return exerciseSlide.Exercise.CreateSubmition(
				submission.Id.ToString(),
				submission.SolutionCode.Text
			);
		}

		[HttpPost]
		[Route("PostResults")]
		public async Task PostResults([FromUri] string token, List<RunningResults> results)
		{
			if (!ModelState.IsValid)
				throw new HttpResponseException(HttpStatusCode.BadRequest);
			CheckRunner(token);
			log.Info($"Получил от RunCsJob информацию о проверке решений: [{string.Join(", ", results.Select(r => r.Id))}]");
			await FuncUtils.TrySeveralTimesAsync(() => userSolutionsRepo.SaveResults(results), 3);
		}


		private void CheckRunner(string token)
		{
			var expectedToken = ConfigurationManager.AppSettings["runnerToken"];
			if (expectedToken != token)
				throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
		}
	}
}