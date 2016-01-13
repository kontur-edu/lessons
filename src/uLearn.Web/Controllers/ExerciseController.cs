using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using uLearn.Web.DataContexts;
using uLearn.Web.FilterAttributes;
using uLearn.Web.LTI;
using uLearn.Web.Models;

namespace uLearn.Web.Controllers
{
	[ULearnAuthorize]
	public class ExerciseController : Controller
	{
		private readonly CourseManager courseManager;
		private readonly UserSolutionsRepo solutionsRepo = new UserSolutionsRepo();
		private readonly VisitsRepo visitsRepo = new VisitsRepo();

		private readonly static TimeSpan executionTimeout = TimeSpan.FromSeconds(30);

		public ExerciseController()
			: this(WebCourseManager.Instance)
		{
		}

		public ExerciseController(CourseManager courseManager)
		{
			this.courseManager = courseManager;
		}

		[HttpPost]
		public async Task<ActionResult> RunSolution(string courseId, int slideIndex = 0, bool isLti = false)
		{
			var code = Request.InputStream.GetString();
			if (code.Length > TextsRepo.MaxTextSize)
			{
				return Json(new RunSolutionResult
				{
					IsCompileError = true,
					CompilationError = "Слишком большой код."
				});
			}
			var exerciseSlide = (ExerciseSlide)courseManager.GetCourse(courseId).Slides[slideIndex];
			
			var result = await CheckSolution(courseId, exerciseSlide, code);
			if (isLti)
				LtiUtils.SubmitScore(exerciseSlide, User.Identity.GetUserId());
			return Json(result);
		}


		private async Task<RunSolutionResult> CheckSolution(string courseId, ExerciseSlide exerciseSlide, string code)
		{
			var exerciseBlock = exerciseSlide.Exercise;
			var solution = exerciseBlock.Solution.BuildSolution(code);
			if (solution.HasErrors)
				return new RunSolutionResult { IsCompileError = true, CompilationError = solution.ErrorMessage, ExecutionServiceName = "uLearn" };
			if (solution.HasStyleIssues)
				return new RunSolutionResult { IsStyleViolation = true, CompilationError = solution.StyleMessage, ExecutionServiceName = "uLearn" };

			var submissionDetails = await solutionsRepo.RunUserSolution(
				courseId, exerciseSlide.Id, User.Identity.GetUserId(), 
				code, null, null, false, "uLearn", 
				GenerateSubmissionName(exerciseSlide), executionTimeout
			);

			if (submissionDetails == null)
				return new RunSolutionResult
				{
					IsCompillerFailure = true,
					CompilationError = "Ой-ой, штуковина, которая проверяет решения сломалась (или просто устала). Попробуйте отправить решение позже (когда она немного отдохнет).",
					ExecutionServiceName = "uLearn"
				};
			var output = submissionDetails.Output.Text;
			var expectedOutput = exerciseBlock.ExpectedOutput.NormalizeEoln();
			var isRightAnswer = submissionDetails.GetVerdict() == "Accepted" && output.Equals(expectedOutput);

			await visitsRepo.AddSolutionAttempt(exerciseSlide.Id, User.Identity.GetUserId(), isRightAnswer);

			return new RunSolutionResult
			{
				IsCompileError = submissionDetails.IsCompilationError,
				CompilationError = submissionDetails.CompilationError.Text,
				IsRightAnswer = isRightAnswer,
				ExpectedOutput = exerciseBlock.HideExpectedOutputOnError ? null : expectedOutput,
				ActualOutput = output,
				ExecutionServiceName = submissionDetails.ExecutionServiceName
			};
		}

		private string GenerateSubmissionName(Slide exerciseSlide)
		{
			return string.Format("{0}: {1} - {2}", User.Identity.Name, exerciseSlide.Info.UnitName, exerciseSlide.Title);
		}
	}
}
