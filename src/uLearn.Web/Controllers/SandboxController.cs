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
	[ULearnAuthorize(MinAccessLevel = CourseRoles.Instructor)]
	public class SandboxController : Controller
	{
		private readonly UserSolutionsRepo solutionsRepo = new UserSolutionsRepo();
		private readonly CourseManager courseManager = WebCourseManager.Instance;

		private readonly static TimeSpan timeout = TimeSpan.FromSeconds(30);

		public ActionResult Index(int max = 200, int skip = 0)
		{
			var submissions = solutionsRepo
				.GetAllSolutions(max, skip).ToList();
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

			var solution = await solutionsRepo.RunUserSolution(
				"web", "runner", User.Identity.GetUserId(), 
				code, null, null, false, "null", 
				User.Identity.Name + ": CsSandbox Web Executor", timeout
			);

			return Json(new RunSolutionResult
			{
				ActualOutput = solution.Output.Text, 
				CompilationError = solution.CompilationError.Text, 
				ExecutionServiceName = solution.ExecutionServiceName, 
				IsCompileError = solution.IsCompilationError, 
				IsRightAnswer = solution.IsRightAnswer
			});
		}

		public ActionResult GetDetails(int id)
		{
			var details = solutionsRepo.GetDetails(id);
			details.SolutionCode.Text = Utils.GetSource(
				details.CourseId, 
				details.SlideId, 
				courseManager, 
				details.SolutionCode.Text);
			return View(solutionsRepo.GetDetails(id));
		}
	}
}