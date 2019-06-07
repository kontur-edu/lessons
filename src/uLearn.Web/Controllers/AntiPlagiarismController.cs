using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using AntiPlagiarism.Api;
using AntiPlagiarism.Api.Models.Parameters;
using AntiPlagiarism.Api.Models.Results;
using Database;
using Database.DataContexts;
using Database.Models;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using uLearn.Web.FilterAttributes;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;

namespace uLearn.Web.Controllers
{
	[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
	public class AntiPlagiarismController : JsonDataContractController
	{
		private readonly ULearnDb db;
		private readonly UserSolutionsRepo userSolutionsRepo;
		private readonly GroupsRepo groupsRepo;
		private readonly CourseManager courseManager;
		private static readonly IAntiPlagiarismClient antiPlagiarismClient;

		static AntiPlagiarismController()
		{
			var serilogLogger = new LoggerConfiguration().WriteTo.Log4Net().CreateLogger();
			var antiPlagiarismEndpointUrl = WebConfigurationManager.AppSettings["ulearn.antiplagiarism.endpoint"];
			var antiPlagiarismToken = WebConfigurationManager.AppSettings["ulearn.antiplagiarism.token"];
			antiPlagiarismClient = new AntiPlagiarismClient(antiPlagiarismEndpointUrl, antiPlagiarismToken, serilogLogger);
		}

		public AntiPlagiarismController(ULearnDb db, UserSolutionsRepo userSolutionsRepo, GroupsRepo groupsRepo)
		{
			this.db = db;
			this.userSolutionsRepo = userSolutionsRepo;
			this.groupsRepo = groupsRepo;
		}

		public AntiPlagiarismController(ULearnDb db, CourseManager courseManager)
			:this(db, new UserSolutionsRepo(db, courseManager), new GroupsRepo(db, courseManager))
		{
			this.courseManager = courseManager;
		}
		
		public AntiPlagiarismController()
			: this(new ULearnDb(), WebCourseManager.Instance)
		{
		}

		public async Task<ActionResult> Info(string courseId, int submissionId)
		{
			var submission = userSolutionsRepo.FindSubmissionById(submissionId);
			if (!string.Equals(submission.CourseId, courseId, StringComparison.InvariantCultureIgnoreCase))
				return HttpNotFound();

			var slide = courseManager.FindCourse(courseId)?.FindSlideById(submission.SlideId) as ExerciseSlide;
			if (slide == null)
				return HttpNotFound();

			if (!slide.Exercise.CheckForPlagiarism)
				return Json(new AntiPlagiarismInfoModel
				{
					Status = "not_checked",
				}, JsonRequestBehavior.AllowGet);
			
			var antiPlagiarismsResult = await GetAuthorPlagiarismsAsync(submission).ConfigureAwait(false);

			var model = new AntiPlagiarismInfoModel
			{
				Status = "checked",
				SuspicionLevel = SuspicionLevel.None,
				SuspiciousAuthorsCount = 0,
			};
			var faintSuspicionAuthorsIds = new HashSet<Guid>();
			var strongSuspicionAuthorsIds = new HashSet<Guid>();
			foreach (var researchedSubmission in antiPlagiarismsResult.ResearchedSubmissions)
			{
				foreach (var plagiarism in researchedSubmission.Plagiarisms)
				{
					if (plagiarism.Weight >= antiPlagiarismsResult.SuspicionLevels.StrongSuspicion)
					{
						strongSuspicionAuthorsIds.Add(plagiarism.SubmissionInfo.AuthorId);
						model.SuspicionLevel = SuspicionLevel.Strong;
					}
					else if (plagiarism.Weight >= antiPlagiarismsResult.SuspicionLevels.FaintSuspicion && model.SuspicionLevel != SuspicionLevel.Strong)
					{
						faintSuspicionAuthorsIds.Add(plagiarism.SubmissionInfo.AuthorId);
						model.SuspicionLevel = SuspicionLevel.Faint;
					}
				}
			}

			model.SuspiciousAuthorsCount = model.SuspicionLevel == SuspicionLevel.Faint ? faintSuspicionAuthorsIds.Count : strongSuspicionAuthorsIds.Count;

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		public async Task<ActionResult> Details(string courseId, int submissionId)
		{
			var submission = userSolutionsRepo.FindSubmissionById(submissionId);
			if (! submission.CourseId.EqualsIgnoreCase(courseId))
				return HttpNotFound();

			var antiPlagiarismsResult = await GetAuthorPlagiarismsAsync(submission);

			var antiPlagiarismSubmissionInfos = antiPlagiarismsResult.ResearchedSubmissions.Select(s => s.SubmissionInfo);
			var plagiarismsSubmissionInfos = antiPlagiarismsResult.ResearchedSubmissions
				.SelectMany(s => s.Plagiarisms).Select(p => p.SubmissionInfo);

			var submissionsIds = antiPlagiarismSubmissionInfos.Concat(plagiarismsSubmissionInfos)
				.Select(si => JsonConvert.DeserializeObject<AdditionalInfo>(si.AdditionalInfo).SubmissionId.ToString())
				.ToList();

			var submissions = userSolutionsRepo.FindSubmissionsByIds(submissionsIds)
				.ToDictionary(s => s.Id);
			submissions[submissionId] = submission;

			var userIds = new HashSet<string>(antiPlagiarismsResult.ResearchedSubmissions.SelectMany(s => s.Plagiarisms).Select(s => s.SubmissionInfo.AuthorId.ToString()));
			userIds.Add(submission.UserId);
			/* Use special MockUserCanSeeAllGroups() instead of User because we want to show all users groups, not only available */
			var usersGroups = groupsRepo.GetUsersGroupsNamesAsStrings(courseId, userIds, new MockUserCanSeeAllGroups()).ToDefaultDictionary();
			var usersArchivedGroups = groupsRepo.GetUsersGroupsNamesAsStrings(courseId, userIds, new MockUserCanSeeAllGroups(), onlyArchived: true).ToDefaultDictionary();

			var course = courseManager.FindCourse(courseId);
			var slide = course?.FindSlideById(submission.SlideId);
			var details = new AntiPlagiarismDetailsModel
			{
				Course = course,
				Slide = slide,
				SubmissionId = submissionId,
				Submissions = submissions,
				UsersGroups = usersGroups,
				UsersArchivedGroups = usersArchivedGroups,
				AntiPlagiarismResponse = antiPlagiarismsResult,
			};
			var json = JsonConvert.SerializeObject(details, Formatting.None, new JsonSerializerSettings()
			{
				NullValueHandling = NullValueHandling.Ignore,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			});
			return View(details);
		}
		
		public ActionResult SubmissionsPanel(int submissionId, Dictionary<int, double> submissionWeights)
		{
			var submission = userSolutionsRepo.FindSubmissionById(submissionId);
			if (submission == null)
				return HttpNotFound();

			var courseId = submission.CourseId;
			var slideId = submission.SlideId;
			var userId = submission.UserId;
			var slide = courseManager.FindCourse(courseId)?.FindSlideById(slideId);
			if (slide == null)
				return HttpNotFound();
			var submissions = userSolutionsRepo.GetAllAcceptedSubmissionsByUser(courseId, slideId, userId).ToList();

			return PartialView("~/Views/Exercise/SubmissionsPanel.cshtml", new ExerciseSubmissionsPanelModel(courseId, slide)
			{
				Submissions = submissions,
				CurrentSubmissionId = submissionId,
				CanTryAgain = false,
				GetSubmissionDescription = s => GetSubmissionDescriptionForPanel(submissionWeights.ContainsKey(s.Id) ? (double?) submissionWeights[s.Id] : null),
				FormUrl = Url.Action("Details", new { courseId }),
				ShowButtons = false,
				SelectControlName = "submissionId",
			});
		}

		private static string GetSubmissionDescriptionForPanel(double? submissionWeight)
		{
			if (!submissionWeight.HasValue)
				return ", не проверялась на списывание";
			var weightPercents = (int)(submissionWeight.Value * 100);
			return $", подозрительность — {weightPercents}%";
		}

		private static readonly ConcurrentDictionary<Tuple<Guid, Guid>, Tuple<DateTime, GetAuthorPlagiarismsResponse>> plagiarismsCache = new ConcurrentDictionary<Tuple<Guid, Guid>, Tuple<DateTime, GetAuthorPlagiarismsResponse>>();
		private static readonly TimeSpan cacheLifeTime = TimeSpan.FromMinutes(10);

		private static async Task<GetAuthorPlagiarismsResponse> GetAuthorPlagiarismsAsync(UserExerciseSubmission submission)
		{
			RemoveOldValuesFromCache();
			var userId = Guid.Parse(submission.UserId);
			var taskId = submission.SlideId;
			var cacheKey = Tuple.Create(userId, taskId);
			if (plagiarismsCache.TryGetValue(cacheKey, out var cachedValue))
			{
				return cachedValue.Item2;
			}

			var value = await antiPlagiarismClient.GetAuthorPlagiarismsAsync(new GetAuthorPlagiarismsParameters
			{
				AuthorId = userId,
				TaskId = taskId
			}).ConfigureAwait(false);
			plagiarismsCache.AddOrUpdate(cacheKey, key => Tuple.Create(DateTime.Now, value), (key, old) => Tuple.Create(DateTime.Now, value));
			return value;
		}

		private static void RemoveOldValuesFromCache()
		{
			foreach (var key in plagiarismsCache.Keys.ToList())
			{
				if (plagiarismsCache.TryGetValue(key, out var cachedValue))
				{
					/* Remove cached value if it is too old */
					if (DateTime.Now.Subtract(cachedValue.Item1) > cacheLifeTime)
						plagiarismsCache.TryRemove(key, out _);
				}
			}
		}
	}

	[JsonConverter(typeof(StringEnumConverter), true)]
	public enum SuspicionLevel : short
	{
		None = 0,
		Faint = 1,
		Strong = 2,
	}
	
	[DataContract]
	public class AntiPlagiarismInfoModel
	{
		[DataMember(Name = "status")]
		public string Status { get; set; }
		
		[DataMember(Name = "suspicion_level")]
		public SuspicionLevel SuspicionLevel { get; set; }
		
		[DataMember(Name = "suspicious_authors_count")]
		public int SuspiciousAuthorsCount { get; set; }
	}

	public class AntiPlagiarismDetailsModel
	{
		public int SubmissionId { get; set; }
		
		public Course Course { get; set; }
		
		public Slide Slide { get; set; }
		
		public DefaultDictionary<string, string> UsersGroups { get; set; }
		
		public DefaultDictionary<string, string> UsersArchivedGroups { get; set; }
		
		public GetAuthorPlagiarismsResponse AntiPlagiarismResponse { get; set; }
		
		public Dictionary<int, UserExerciseSubmission> Submissions { get; set; }
	}

	public class MockUserCanSeeAllGroups : IPrincipal
	{
		public bool IsInRole(string role)
		{
			return true;
		}

		public IIdentity Identity => new ClaimsIdentity();
	}
}