using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Database.DataContexts;
using Database.Models;
using log4net;
using uLearn.Web.FilterAttributes;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;

namespace uLearn.Web.Controllers
{
	public class GraderController : BaseExerciseController
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(GraderController));

		public GradersRepo gradersRepo;

		public GraderController()
		{
			gradersRepo = new GradersRepo(db);
		}

		[HttpPost]
		public async Task<ActionResult> Submit(GraderSubmitRequest request)
		{
			if (string.IsNullOrEmpty(request.ClientUserId))
				request.ClientUserId = "";

			log.Info($"Got new submission for grader: {request.JsonSerialize()}");

			var client = gradersRepo.FindGraderClient(request.CourseId, request.ClientId);
			if (client == null)
				return Json(new ApiResult { Status = "error", Message = "Invalid client id" });

			string code;
			try
			{
				code = Encoding.UTF8.GetString(Convert.FromBase64String(request.Solution));
			}
			catch (Exception e) when (e is DecoderFallbackException || e is FormatException)
			{
				return Json(new ApiResult { Status = "error", Message = $"Can\'t decode solution from BASE64: {e.Message}" });
			}
			if (code.Length > TextsRepo.MaxTextSize)
				return Json(new ApiResult { Status = "error", Message = $"Too large solution. Max allowed size is {TextsRepo.MaxTextSize} bytes" });

			var slide = courseManager.FindCourse(request.CourseId)?.FindSlideById(request.SlideId) as ExerciseSlide;
			if (slide == null)
				return Json(new ApiResult { Status = "error", Message = "Invalid slide id" });

			log.Info("Send solution to checking, don't wait until it will be checked");

			var result = await CheckSolution(
				request.CourseId, slide, code, client.UserId, $"Grader client {client.Name}, user {request.ClientUserId}",
				waitUntilChecked: false, saveSubmissionOnCompileErrors: true
			);

			log.Info($"Result of check starting: {result.JsonSerialize()}");

			if (result.IsCompillerFailure)
				return Json(new ApiResult { Status = "error", Message = "Can\'t start solution checking, service is temporary unavailable. Try later" });

			var submissionId = result.SubmissionId;
			var solutionByGrader = await gradersRepo.AddSolutionFromGraderClient(request.ClientId, submissionId, request.ClientUserId);

			log.Info($"Saved to database as grader solution #{solutionByGrader.Id}");

			return Json(new SubmitResult { Status = "ok", SubmissionId = solutionByGrader.Id });
		}

		public ActionResult Submission(GraderSubmissionRequest request)
		{
			var clientId = request.ClientId;
			var submissionId = request.SubmissionId;
			log.Info($"Got a request about submission status. Submission id is {submissionId}, from client {clientId}");

			var solution = gradersRepo.FindSolutionFromGraderClient(submissionId);

			if (solution == null)
				return Json(new ApiResult { Status = "error", Message = "Invalid submission id" }, JsonRequestBehavior.AllowGet);

			if (clientId != solution.ClientId)
				return Json(new ApiResult { Status = "error", Message = "Invalid client id or it's not your submission" }, JsonRequestBehavior.AllowGet);

			var courseId = solution.Submission.CourseId;
			var slideId = solution.Submission.SlideId;
			var slide = courseManager.FindCourse(courseId)?.FindSlideById(slideId) as ExerciseSlide;
			if (slide == null)
				return Json(new ApiResult { Status = "error", Message = "Slide doesn't exist anymore" });

			return Json(SubmissionResult.FromSolution(solution, slide), JsonRequestBehavior.AllowGet);
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public ActionResult Clients(string courseId)
		{
			var model = new GraderClientsViewModel
			{
				CourseId = courseId,
				Clients = gradersRepo.GetGraderClients(courseId)
			};
			return View(model);
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		[System.Web.Mvc.HttpPost]
		public async Task<ActionResult> AddClient(string courseId, string name)
		{
			await gradersRepo.AddGraderClient(courseId, name);
			return RedirectToAction("Clients", new { courseId = courseId });
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public ActionResult Submissions(string courseId, Guid clientId, string search = "", int max = 200)
		{
			var client = gradersRepo.FindGraderClient(courseId, clientId);
			if (client == null)
				return HttpNotFound();

			var solutions = gradersRepo.GetClientSolutions(client, search, max + 1);
			var isMore = solutions.Count > max;
			solutions = solutions.Take(max).ToList();
			return View(new GraderSolutionsViewModel
			{
				CourseId = courseId,
				Client = client,
				Solutions = solutions,
				Search = search,
				IsMore = isMore,
			});
		}
	}

	[DataContract]
	public class ApiResult
	{
		[DataMember(Name = "status")]
		public string Status { get; set; }

		[DataMember(Name = "message", EmitDefaultValue = false)]
		public string Message { get; set; }
	}

	[DataContract]
	public class SubmitResult : ApiResult
	{
		[DataMember(Name = "submission_id")]
		public int SubmissionId { get; set; }
	}

	[DataContract]
	public class SubmissionResult : ApiResult
	{
		public static SubmissionResult FromSolution(ExerciseSolutionByGrader solution, ExerciseSlide slide)
		{
			var automaticChecking = solution.Submission?.AutomaticChecking;

			if (automaticChecking == null || automaticChecking.Status != AutomaticExerciseCheckingStatus.Done)
				return new SubmissionResult { Status = "IN_PROCESS" };

			var score = (double)automaticChecking.Score / slide.Scoring.PassedTestsScore;
			if (score > 1)
				score = 1;

			return new SubmissionResult
			{
				Status = "READY",
				Result = score,
				CompilationLog = automaticChecking.CompilationError?.Text ?? "",
				ExecutionLog = automaticChecking.Output?.Text ?? "",
			};
		}

		[DataMember(Name = "result")]
		public double Result { get; set; }

		[DataMember(Name = "compilation_log")]
		public string CompilationLog { get; set; }

		[DataMember(Name = "execution_log")]
		public string ExecutionLog { get; set; }
	}

	[DataContract]
	[JsonDataContractModelBinder]
	public class GraderSubmitRequest
	{
		[DataMember(Name = "client_id")]
		public Guid ClientId { get; set; }

		[DataMember(Name = "course_id")]
		public string CourseId { get; set; }

		[DataMember(Name = "slide_id")]
		public Guid SlideId { get; set; }

		[DataMember(Name = "solution")]
		public string Solution { get; set; }

		[DataMember(Name = "user_id")]
		public string ClientUserId { get; set; }
	}

	public class GraderSubmissionRequest
	{
		public Guid ClientId { get; set; }
		public int SubmissionId { get; set; }
	}

	public class GraderClientsViewModel
	{
		public string CourseId { get; set; }
		public List<GraderClient> Clients { get; set; }
	}

	public class GraderSolutionsViewModel
	{
		public string CourseId { get; set; }
		public GraderClient Client { get; set; }
		public string Search { get; set; }
		public List<ExerciseSolutionByGrader> Solutions { get; set; }
		public bool IsMore { get; set; }
	}
}