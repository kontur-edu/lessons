using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using uLearn.Web.DataContexts;
using uLearn.Web.FilterAttributes;
using uLearn.Web.Models;

namespace uLearn.Web.Controllers
{
	[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
	public class SandboxController : Controller
	{
		private readonly UserSolutionsRepo solutionsRepo = new UserSolutionsRepo(new ULearnDb());
		private readonly CourseManager courseManager = WebCourseManager.Instance;

		private static readonly TimeSpan timeout = TimeSpan.FromSeconds(30);

		public ActionResult Index(int max = 200, int skip = 0)
		{
			var submissions = solutionsRepo.GetAllSubmissions(max, skip).ToList();
			return View(new SubmissionsListModel
			{
				Submissions = submissions
			});
		}

		public ActionResult RunCode()
		{
			return View();
		}

		[HttpPost]
		public async Task<ActionResult> Run()
		{
			var code = Request.InputStream.GetString();

			var submission = await solutionsRepo.AddUserExerciseSubmission("web", Guid.Empty, code, null, null, User.Identity.GetUserId(), "null", User.Identity.Name + ": CsSandbox Web Executor");
			try
			{
				await solutionsRepo.RunSubmission(submission, timeout, waitUntilChecked: true);
			}
			catch (SubmissionCheckingTimeout)
			{
				return Json(new RunSolutionResult
				{
					ErrorMessage = "Что-то пошло не так :(",
					IsCompileError = true,
				});
			}

			/* Update the submission */
			submission = solutionsRepo.FindNoTrackingSubmission(submission.Id);
			
			var automaticChecking = submission.AutomaticChecking;

			return Json(new RunSolutionResult
			{
				ActualOutput = automaticChecking.Output.Text,
				ErrorMessage = automaticChecking.CompilationError.Text,
				ExecutionServiceName = automaticChecking.ExecutionServiceName,
				IsCompileError = automaticChecking.IsCompilationError,
				IsRightAnswer = automaticChecking.IsRightAnswer,
				SubmissionId = submission.Id,
			});
		}

		public ActionResult GetDetails(int id)
		{
			var submission = solutionsRepo.FindNoTrackingSubmission(id);

			if (submission == null)
				return HttpNotFound();

			submission.SolutionCode.Text = ((ExerciseSlide)courseManager
				.GetCourse(submission.CourseId)
				.GetSlideById(submission.SlideId))
				.Exercise
				.GetSourceCode(submission.SolutionCode.Text);

			return View(submission);
		}
	}
}