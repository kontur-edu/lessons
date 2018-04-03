using log4net;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ApprovalUtilities.Utilities;
using Database;
using Database.DataContexts;
using Database.Extensions;
using Database.Models;
using Microsoft.VisualBasic.FileIO;
using uLearn.CSharp;
using uLearn.Extensions;
using uLearn.Model.Blocks;
using uLearn.Quizes;
using uLearn.Web.Extensions;
using uLearn.Web.FilterAttributes;
using uLearn.Web.Models;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace uLearn.Web.Controllers
{
	[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
	public class AdminController : Controller
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(AdminController));

		private readonly WebCourseManager courseManager = WebCourseManager.Instance;
		private readonly ULearnDb db;
		private readonly UsersRepo usersRepo;
		private readonly UserRolesRepo userRolesRepo;
		private readonly CommentsRepo commentsRepo;
		private readonly UserManager<ApplicationUser> userManager;
		private readonly QuizzesRepo quizzesRepo;
		private readonly CoursesRepo coursesRepo;
		private readonly GroupsRepo groupsRepo;
		private readonly SlideCheckingsRepo slideCheckingsRepo;
		private readonly UserSolutionsRepo userSolutionsRepo;
		private readonly CertificatesRepo certificatesRepo;
		private readonly AdditionalScoresRepo additionalScoresRepo;
		private readonly NotificationsRepo notificationsRepo;
		private readonly SystemAccessesRepo systemAccessesRepo;
		private readonly StyleErrorsRepo styleErrorsRepo;

		public AdminController()
		{
			db = new ULearnDb();

			usersRepo = new UsersRepo(db);
			userRolesRepo = new UserRolesRepo(db);
			commentsRepo = new CommentsRepo(db);
			userManager = new ULearnUserManager(db);
			quizzesRepo = new QuizzesRepo(db);
			coursesRepo = new CoursesRepo(db);
			groupsRepo = new GroupsRepo(db, courseManager);
			slideCheckingsRepo = new SlideCheckingsRepo(db);
			userSolutionsRepo = new UserSolutionsRepo(db, courseManager);
			certificatesRepo = new CertificatesRepo(db, courseManager);
			additionalScoresRepo = new AdditionalScoresRepo(db);
			notificationsRepo = new NotificationsRepo(db);
			systemAccessesRepo = new SystemAccessesRepo(db);
			styleErrorsRepo = new StyleErrorsRepo(db);
		}

		public ActionResult CourseList(string courseCreationLastTry = null)
		{
			var courses = new HashSet<string>(User.GetControllableCoursesId());
			var incorrectChars = new string(CourseManager.GetInvalidCharacters().OrderBy(c => c).Where(c => 32 <= c).ToArray());
			var model = new CourseListViewModel
			{
				Courses = courseManager.GetCourses().Where(course => courses.Contains(course.Id)).Select(course => new CourseViewModel
				{
					Id = course.Id,
					Title = course.Title,
					LastWriteTime = courseManager.GetLastWriteTime(course.Id)
				}).ToList(),
				CourseCreationLastTry = courseCreationLastTry,
				InvalidCharacters = incorrectChars
			};
			return View(model);
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public ActionResult SpellingErrors(Guid versionId)
		{
			var course = courseManager.GetVersion(versionId);
			courseManager.EnsureVersionIsExtracted(versionId);
			return PartialView(course.SpellCheck());
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public ActionResult Units(string courseId)
		{
			var course = courseManager.GetCourse(courseId);
			var appearances = db.UnitAppearances.Where(u => u.CourseId == course.Id).ToList();
			var unitAppearances = course.Units
				.Select(unit => Tuple.Create(unit, appearances.FirstOrDefault(a => a.UnitId == unit.Id)))
				.ToList();
			return View(new UnitsListViewModel(course.Id, course.Title, unitAppearances, DateTime.Now));
		}

		[HttpPost]
		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public async Task<RedirectToRouteResult> SetPublishTime(string courseId, Guid unitId, string publishTime)
		{
			var oldInfo = await db.UnitAppearances.Where(u => u.CourseId == courseId && u.UnitId == unitId).ToListAsync();
			db.UnitAppearances.RemoveRange(oldInfo);
			var unitAppearance = new UnitAppearance
			{
				CourseId = courseId,
				UnitId = unitId,
				UserName = User.Identity.Name,
				PublishTime = DateTime.Parse(publishTime),
			};
			db.UnitAppearances.Add(unitAppearance);
			await db.SaveChangesAsync();
			return RedirectToAction("Units", new { courseId });
		}

		[HttpPost]
		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public async Task<RedirectToRouteResult> RemovePublishTime(string courseId, Guid unitId)
		{
			var unitAppearance = await db.UnitAppearances.FirstOrDefaultAsync(u => u.CourseId == courseId && u.UnitId == unitId);
			if (unitAppearance != null)
			{
				db.UnitAppearances.Remove(unitAppearance);
				await db.SaveChangesAsync();
			}
			return RedirectToAction("Units", new { courseId });
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public ActionResult DownloadPackage(string courseId)
		{
			var packageName = courseManager.GetPackageName(courseId);
			return File(courseManager.GetStagingCoursePath(courseId), "application/zip", packageName);
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public ActionResult DownloadVersion(string courseId, Guid versionId)
		{
			var packageName = courseManager.GetPackageName(courseId);
			return File(courseManager.GetCourseVersionFile(versionId).FullName, "application/zip", packageName);
		}

		private void CreateQuizVersionsForSlides(string courseId, IEnumerable<Slide> slides)
		{
			foreach (var slide in slides.OfType<QuizSlide>())
				quizzesRepo.AddQuizVersionIfNeeded(courseId, slide);
		}

		private async Task NotifyAboutCourseVersion(string courseId, Guid versionId, string userId)
		{
			var notification = new UploadedPackageNotification
			{
				CourseVersionId = versionId,
			};
			await notificationsRepo.AddNotification(courseId, notification, userId);
		}

		[HttpPost]
		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public async Task<ActionResult> UploadCourse(string courseId, HttpPostedFileBase file)
		{
			if (file == null || file.ContentLength <= 0)
				return RedirectToAction("Packages", new { courseId });

			var fileName = Path.GetFileName(file.FileName);
			if (fileName == null || !fileName.ToLower().EndsWith(".zip"))
				return RedirectToAction("Packages", new { courseId });

			var versionId = Guid.NewGuid();

			var destinationFile = courseManager.GetCourseVersionFile(versionId);
			file.SaveAs(destinationFile.FullName);

			try
			{
				/* Load version and put it into LRU-cache */
				courseManager.GetVersion(versionId);
			}
			catch (Exception e)
			{
				var errorMessage = e.Message.ToLowerFirstLetter();
				if (e.InnerException != null)
					errorMessage += $" ({e.InnerException.Message})";
				return RedirectToAction("Packages", new { courseId, error=errorMessage });
			}
			
			var userId = User.Identity.GetUserId();
			await coursesRepo.AddCourseVersion(courseId, versionId, userId);
			await NotifyAboutCourseVersion(courseId, versionId, userId);

			return RedirectToAction("Diagnostics", new { courseId, versionId });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ULearnAuthorize(ShouldBeSysAdmin = true)]
		public ActionResult CreateCourse(string courseId)
		{
			if (!courseManager.TryCreateCourse(courseId))
				return RedirectToAction("CourseList", new { courseCreationLastTry = courseId });
			return RedirectToAction("Users", new { courseId, onlyPrivileged = true });
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public ActionResult Packages(string courseId, string error="")
		{
			var hasPackage = courseManager.HasPackageFor(courseId);
			var lastUpdate = courseManager.GetLastWriteTime(courseId);
			var courseVersions = coursesRepo.GetCourseVersions(courseId).ToList();
			var publishedVersion = coursesRepo.GetPublishedCourseVersion(courseId);
			return View(model: new PackagesViewModel
			{
				CourseId = courseId,
				HasPackage = hasPackage,
				LastUpdate = lastUpdate,
				Versions = courseVersions,
				PublishedVersion = publishedVersion,
				Error = error,
			});
		}

		public ActionResult Comments(string courseId)
		{
			var userId = User.Identity.GetUserId();			
			
			var course = courseManager.GetCourse(courseId);
			var commentsPolicy = commentsRepo.GetCommentsPolicy(courseId);

			var comments = commentsRepo.GetCourseComments(courseId).Where(c => ! c.IsForInstructorsOnly).OrderByDescending(x => x.PublishTime).ToList();
			var commentsLikes = commentsRepo.GetCommentsLikesCounts(comments);
			var commentsLikedByUser = commentsRepo.GetCourseCommentsLikedByUser(courseId, userId);
			var commentsById = comments.ToDictionary(x => x.Id);

			var canViewProfiles = systemAccessesRepo.HasSystemAccess(userId, SystemAccessType.ViewAllProfiles) || User.IsSystemAdministrator();

			return View(new AdminCommentsViewModel
			{
				CourseId = courseId,
				IsCommentsEnabled = commentsPolicy.IsCommentsEnabled,
				ModerationPolicy = commentsPolicy.ModerationPolicy,
				OnlyInstructorsCanReply = commentsPolicy.OnlyInstructorsCanReply,
				Comments = (from c in comments
					let slide = course.FindSlideById(c.SlideId)
					where slide != null
					select
					new CommentViewModel
					{
						Comment = c,
						LikesCount = commentsLikes.GetOrDefault(c.Id),
						IsLikedByUser = commentsLikedByUser.Contains(c.Id),
						Replies = new List<CommentViewModel>(),
						CanEditAndDeleteComment = true,
						CanModerateComment = true,
						IsCommentVisibleForUser = true,
						ShowContextInformation = true,
						ContextSlideTitle = slide.Title,
						ContextParentComment = c.IsTopLevel() ? null : commentsById.ContainsKey(c.ParentCommentId) ? commentsById[c.ParentCommentId].Text : null,
						CanViewAuthorProfile = canViewProfiles,
					}).ToList()
			});
		}

		private ManualCheckingQueueFilterOptions GetManualCheckingFilterOptionsByGroup(string courseId, List<string> groupsIds)
		{
			return ControllerUtils.GetFilterOptionsByGroup<ManualCheckingQueueFilterOptions>(groupsRepo, User, courseId, groupsIds);
		}

		private ActionResult ManualCheckingQueue<T>(string actionName, string viewName, string courseId, bool done, List<string> groupsIds, string userId = "", Guid? slideId = null, string message = "") where T : AbstractManualSlideChecking
		{
			var MaxShownQueueSize = 500;
			var course = courseManager.GetCourse(courseId);

			var filterOptions = GetManualCheckingFilterOptionsByGroup(courseId, groupsIds);
			if (filterOptions.UsersIds == null)
				groupsIds = new List<string> { "all" };

			if (!string.IsNullOrEmpty(userId))
				filterOptions.UsersIds = new List<string> { userId };
			if (slideId.HasValue)
				filterOptions.SlidesIds = new List<Guid> { slideId.Value };

			filterOptions.OnlyChecked = done;
			filterOptions.Count = MaxShownQueueSize + 1;
			var checkings = slideCheckingsRepo.GetManualCheckingQueue<T>(filterOptions).ToList();

			if (!checkings.Any() && !string.IsNullOrEmpty(message))
				return RedirectToAction(actionName, new { courseId, group = string.Join(",", groupsIds) });

			var groups = groupsRepo.GetAvailableForUserGroups(courseId, User);
			var reviews = slideCheckingsRepo.GetExerciseCodeReviewForCheckings(checkings.Select(c => c.Id));
			var submissionsIds = checkings.Select(c => (c as ManualExerciseChecking)?.SubmissionId).Where(s => s.HasValue).Select(s => s.Value);
			var solutions = userSolutionsRepo.GetSolutionsForSubmissions(submissionsIds);

			filterOptions.SlidesIds = null;
			var usedCheckings = slideCheckingsRepo.GetManualCheckingQueue<T>(filterOptions);
			var usedSlidesIds = new HashSet<Guid>(usedCheckings.Select(c => c.SlideId));

			filterOptions = GetManualCheckingFilterOptionsByGroup(courseId, groupsIds);
			filterOptions.OnlyChecked = null;
			var allCheckingsSlidesIds = slideCheckingsRepo.GetManualCheckingQueue<T>(filterOptions).Select(c => c.SlideId).Distinct();

			var emptySlideMock = new Slide(Enumerable.Empty<SlideBlock>(), new SlideInfo(null, null, -1), "", Guid.Empty, meta: null);
			var allCheckingsSlidesTitles = allCheckingsSlidesIds
				.Select(s => new KeyValuePair<Guid, Slide>(s, course.FindSlideById(s)))
				.Where(kvp => kvp.Value != null)
				.Union(new List<KeyValuePair<Guid, Slide>>
				{
					/* Divider between used slides and another ones */
					new KeyValuePair<Guid, Slide>(Guid.Empty, emptySlideMock)
				})
				.OrderBy(s => usedSlidesIds.Contains(s.Key) ? 0 : 1)
				.ThenBy(s => s.Value.Index)
				.Select(s => new KeyValuePair<Guid, string>(s.Key, s.Value.Title))
				.ToList();
			/* Remove divider iff it is first or last */
			if (allCheckingsSlidesTitles.First().Key == Guid.Empty || allCheckingsSlidesTitles.Last().Key == Guid.Empty)
				allCheckingsSlidesTitles.RemoveAll(kvp => kvp.Key == Guid.Empty);

			return View(viewName, new ManualCheckingQueueViewModel
			{
				CourseId = courseId,
				Checkings = checkings.Take(MaxShownQueueSize).Select(c =>
				{
					var slide = course.GetSlideById(c.SlideId);
					return new ManualCheckingQueueItemViewModel
					{
						CheckingQueueItem = c,
						ContextSlideId = slide.Id,
						ContextSlideTitle = slide.Title,
						ContextMaxScore = (slide as ExerciseSlide)?.Exercise.MaxReviewScore ?? slide.MaxScore,
						ContextTimestamp = c.Timestamp,
						ContextReviews = reviews.GetOrDefault(c.Id, new List<ExerciseCodeReview>()),
						ContextExerciseSolution = c is ManualExerciseChecking ?
							solutions.GetOrDefault((c as ManualExerciseChecking).SubmissionId, "") :
							"",
					};
				}).ToList(),
				Groups = groups,
				SelectedGroupsIds = groupsIds,
				Message = message,
				AlreadyChecked = done,
				ExistsMore = checkings.Count > MaxShownQueueSize,
				ShowFilterForm = string.IsNullOrEmpty(userId),
				SlidesTitles = allCheckingsSlidesTitles,
			});
		}

		public ActionResult ManualQuizCheckingQueue(string courseId, bool done = false, string userId = "", Guid? slideId = null, string message = "")
		{
			var groupsIds = Request.GetMultipleValuesFromQueryString("group");
			return ManualCheckingQueue<ManualQuizChecking>("ManualQuizCheckingQueue", "ManualQuizCheckingQueue", courseId, done, groupsIds, userId, slideId, message);
		}

		public ActionResult ManualExerciseCheckingQueue(string courseId, bool done = false, string userId = "", Guid? slideId = null, string message = "")
		{
			var groupsIds = Request.GetMultipleValuesFromQueryString("group");
			return ManualCheckingQueue<ManualExerciseChecking>("ManualExerciseCheckingQueue", "ManualExerciseCheckingQueue", courseId, done, groupsIds, userId, slideId, message);
		}

		private async Task<ActionResult> InternalManualCheck<T>(string courseId, string actionName, int queueItemId, bool ignoreLock = false, List<string> groupsIds = null, bool recheck = false) where T : AbstractManualSlideChecking
		{
			T checking;
			var joinedGroupsIds = string.Join(",", groupsIds ?? new List<string>());
			using (var transaction = db.Database.BeginTransaction())
			{
				checking = slideCheckingsRepo.FindManualCheckingById<T>(queueItemId);
				if (checking == null)
					return RedirectToAction(actionName,
						new
						{
							courseId = courseId,
							group = joinedGroupsIds,
							done = recheck,
							message = "already_checked",
						});

				if (!User.HasAccessFor(checking.CourseId, CourseRole.Instructor))
					return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

				if (checking.IsChecked && !recheck)
					return RedirectToAction(actionName,
						new
						{
							courseId = checking.CourseId,
							group = joinedGroupsIds,
							done = recheck,
							message = "already_checked",
						});

				if (! recheck)
					await slideCheckingsRepo.LockManualChecking(checking, User.Identity.GetUserId());
				transaction.Commit();
			}

			return RedirectToRoute("Course.SlideById", new
			{
				checking.CourseId,
				checking.SlideId,
				CheckQueueItemId = checking.Id,
				Group = joinedGroupsIds,
			});
		}

		private async Task<ActionResult> CheckNextManualCheckingForSlide<T>(string actionName, string courseId, Guid slideId, List<string> groupsIds) where T : AbstractManualSlideChecking
		{
			using (var transaction = db.Database.BeginTransaction())
			{
				var filterOptions = GetManualCheckingFilterOptionsByGroup(courseId, groupsIds);
				if (filterOptions.UsersIds == null)
					groupsIds = new List<string> { "all" };
				filterOptions.SlidesIds = new List<Guid> { slideId };
				var checkings = slideCheckingsRepo.GetManualCheckingQueue<T>(filterOptions).ToList();

				var itemToCheck = checkings.FirstOrDefault(i => !i.IsLocked);
				if (itemToCheck == null)
					return RedirectToAction(actionName, new { courseId, group = string.Join(",", groupsIds), message = "slide_checked" });

				await slideCheckingsRepo.LockManualChecking(itemToCheck, User.Identity.GetUserId());

				transaction.Commit();

				return await InternalManualCheck<T>(courseId, actionName, itemToCheck.Id, true, groupsIds);
			}
		}

		public async Task<ActionResult> CheckQuiz(string courseId, int id, bool recheck = false)
		{
			var groupsIds = Request.GetMultipleValuesFromQueryString("group");
			return await InternalManualCheck<ManualQuizChecking>(courseId, "ManualQuizCheckingQueue", id, false, groupsIds, recheck);
		}

		public async Task<ActionResult> CheckExercise(string courseId, int id, bool recheck = false)
		{
			var groupsIds = Request.GetMultipleValuesFromQueryString("group");
			return await InternalManualCheck<ManualExerciseChecking>(courseId, "ManualExerciseCheckingQueue", id, false, groupsIds, recheck);
		}

		public async Task<ActionResult> CheckNextQuizForSlide(string courseId, Guid slideId)
		{
			var groupsIds = Request.GetMultipleValuesFromQueryString("group");
			return await CheckNextManualCheckingForSlide<ManualQuizChecking>("ManualQuizCheckingQueue", courseId, slideId, groupsIds);
		}

		public async Task<ActionResult> CheckNextExerciseForSlide(string courseId, Guid slideId)
		{
			var groupsIds = Request.GetMultipleValuesFromQueryString("group");
			return await CheckNextManualCheckingForSlide<ManualExerciseChecking>("ManualExerciseCheckingQueue", courseId, slideId, groupsIds);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public async Task<ActionResult> SaveCommentsPolicy(AdminCommentsViewModel model)
		{
			var courseId = model.CourseId;
			var commentsPolicy = new CommentsPolicy
			{
				CourseId = courseId,
				IsCommentsEnabled = model.IsCommentsEnabled,
				ModerationPolicy = model.ModerationPolicy,
				OnlyInstructorsCanReply = model.OnlyInstructorsCanReply
			};
			await commentsRepo.SaveCommentsPolicy(commentsPolicy);
			return RedirectToAction("Comments", new { courseId });
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		public ActionResult Users(UserSearchQueryModel queryModel)
		{
			var isCourseAdmin = User.HasAccessFor(queryModel.CourseId, CourseRole.CourseAdmin);
			var canAddInstructors = coursesRepo.HasCourseAccess(User.Identity.GetUserId(), queryModel.CourseId, CourseAccessType.AddAndRemoveInstructors);
			if (! isCourseAdmin && ! canAddInstructors)
				return HttpNotFound();

			if (string.IsNullOrEmpty(queryModel.CourseId))
				return RedirectToAction("CourseList");
			return View(queryModel);
		}

		[ChildActionOnly]
		public ActionResult UsersPartial(UserSearchQueryModel queryModel)
		{
			var userRoles = usersRepo.FilterUsers(queryModel, userManager);
			var model = GetUserListModel(userRoles, queryModel.CourseId);

			return PartialView("_UserListPartial", model);
		}

		private UserListModel GetUserListModel(IEnumerable<UserRolesInfo> userRoles, string courseId)
		{
			var rolesForUsers = db.UserRoles
				.Where(role => role.CourseId == courseId)
				.GroupBy(role => role.UserId)
				.ToDictionary(
					g => g.Key,
					g => g.Select(role => role.Role).Distinct().ToList()
				);

			var currentUserId = User.Identity.GetUserId();			
			
			var isCourseAdmin = User.HasAccessFor(courseId, CourseRole.CourseAdmin);
			var canAddInstructors = coursesRepo.HasCourseAccess(currentUserId, courseId, CourseAccessType.AddAndRemoveInstructors);
			var model = new UserListModel
			{
				CanToggleRoles = isCourseAdmin || canAddInstructors,
				ShowDangerEntities = false,
				Users = new List<UserModel>(),
				CanViewAndToggleCourseAccesses = isCourseAdmin,
				CanViewAndToogleSystemAccesses = false,
				CanViewProfiles = systemAccessesRepo.HasSystemAccess(currentUserId, SystemAccessType.ViewAllProfiles) || User.IsSystemAdministrator(),
			};

			foreach (var userRolesInfo in userRoles)
			{
				var user = new UserModel(userRolesInfo);

				if (!rolesForUsers.TryGetValue(userRolesInfo.UserId, out List<CourseRole> roles))
					roles = new List<CourseRole>();

				user.CourseRoles = Enum.GetValues(typeof(CourseRole))
					.Cast<CourseRole>()
					.Where(courseRole => courseRole != CourseRole.Student)
					.ToDictionary(
						courseRole => courseRole.ToString(),
						courseRole => (ICoursesRolesListModel)new SingleCourseRolesModel
						{
							HasAccess = roles.Contains(courseRole),
							ToggleUrl = Url.Action("ToggleRole", "Account", new { courseId, userId = user.UserId, role = courseRole })
						});

				var courseAccesses = coursesRepo.GetCourseAccesses(courseId, user.UserId).Select(a => a.AccessType).ToList();
				user.CourseAccesses[courseId] = Enum.GetValues(typeof(CourseAccessType))
					.Cast<CourseAccessType>()
					.ToDictionary(
						a => a,
						a => new CourseAccessModel
						{
							CourseId = courseId,
							HasAccess = courseAccesses.Contains(a),
							ToggleUrl = Url.Action("ToggleCourseAccess", "Admin", new { courseId = courseId, userId = user.UserId, accessType = a })
						}
					);

				model.Users.Add(user);
			}

			model.UsersGroups = groupsRepo.GetUsersGroupsNamesAsStrings(courseId, model.Users.Select(u => u.UserId), User);

			return model;
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public ActionResult Diagnostics(string courseId, Guid? versionId)
		{
			if (versionId == null)
			{
				return View(new DiagnosticsModel
				{
					CourseId = courseId,
				});
			}

			var versionIdGuid = (Guid)versionId;

			var course = courseManager.GetCourse(courseId);
			var version = courseManager.GetVersion(versionIdGuid);

			var courseDiff = new CourseDiff(course, version);

			return View(new DiagnosticsModel
			{
				CourseId = courseId,
				IsDiagnosticsForVersion = true,
				VersionId = versionIdGuid,
				CourseDiff = courseDiff,
			});
		}

		public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
		{
			/* Check that one directory is not a parent of another one */
			if (source.FullName.StartsWith(target.FullName) || target.FullName.StartsWith(source.FullName))
				throw new Exception("Can\'t copy files recursifely from parent to child directory or from child to parent");

			foreach (var subDirectory in source.GetDirectories())
				CopyFilesRecursively(subDirectory, target.CreateSubdirectory(subDirectory.Name));
			foreach (var file in source.GetFiles())
				file.CopyTo(Path.Combine(target.FullName, file.Name), true);
		}

		private async Task NotifyAboutPublishedCourseVersion(string courseId, Guid versionId, string userId)
		{
			var notification = new PublishedPackageNotification
			{
				CourseVersionId = versionId,
			};
			await notificationsRepo.AddNotification(courseId, notification, userId);
		}

		[HttpPost]
		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public async Task<ActionResult> PublishVersion(string courseId, Guid versionId)
		{
			log.Info($"Публикую версию курса {courseId}. ID версии: {versionId}");
			var versionFile = courseManager.GetCourseVersionFile(versionId);
			var courseFile = courseManager.GetStagingCourseFile(courseId);
			var oldCourse = courseManager.GetCourse(courseId);

			/* First, try to load course from LRU-cache or zip file */
			log.Info($"Загружаю версию {versionId}");
			var version = courseManager.GetVersion(versionId);

			/* Copy version's zip file to course's zip archive, overwrite if need */
			log.Info($"Копирую архив с версий в архив с курсом: {versionFile.FullName} → {courseFile.FullName}");
			versionFile.CopyTo(courseFile.FullName, true);
			courseManager.EnsureVersionIsExtracted(versionId);

			/* Replace courseId */
			version.Id = courseId;

			/* and move course from version's directory to courses's directory */
			var extractedVersionDirectory = courseManager.GetExtractedVersionDirectory(versionId);
			var extractedCourseDirectory = courseManager.GetExtractedCourseDirectory(courseId);
			log.Info($"Перемещаю паку с версий в папку с курсом: {extractedVersionDirectory.FullName} → {extractedCourseDirectory.FullName}");
			courseManager.MoveCourse(
				version,
				extractedVersionDirectory,
				extractedCourseDirectory);

			log.Info($"Создаю версии тестов для курса {courseId}");
			CreateQuizVersionsForSlides(courseId, version.Slides);
			log.Info($"Помечаю версию {versionId} как опубликованную версию курса {courseId}");
			await coursesRepo.MarkCourseVersionAsPublished(versionId);
			await NotifyAboutPublishedCourseVersion(courseId, versionId, User.Identity.GetUserId());

			log.Info($"Обновляю курс {courseId} в оперативной памяти");
			courseManager.UpdateCourseVersion(courseId, versionId);

			var courseDiff = new CourseDiff(oldCourse, version);

			return View("Diagnostics", new DiagnosticsModel
			{
				CourseId = courseId,
				IsDiagnosticsForVersion = true,
				IsVersionPublished = true,
				VersionId = versionId,
				CourseDiff = courseDiff,
			});
		}

		[HttpPost]
		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public async Task<ActionResult> DeleteVersion(string courseId, Guid versionId)
		{
			/* Remove information from database */
			await coursesRepo.DeleteCourseVersion(courseId, versionId);

			/* Delete zip-archive from file system */
			courseManager.GetCourseVersionFile(versionId).Delete();

			return RedirectToAction("Packages", new { courseId });
		}

		private static List<ScoringGroup> GetScoringGroupsCanBeSetInSomeUnit(Course course)
		{
			return course.Units
				.SelectMany(u => u.Scoring.Groups.Values)
				.Where(g => g.CanBeSetByInstructor && !g.EnabledForEveryone)
				.DistinctBy(g => g.Id)
				.ToList();
		}

		public ActionResult Groups(string courseId)
		{
			var userId = User.Identity.GetUserId();

			var groups = groupsRepo.GetAvailableForUserGroups(courseId, User, includeArchived: true);
			var groupsIds = groups.Select(g => g.Id).ToList();

			var course = courseManager.GetCourse(courseId);
			var scoringGroupsCanBeSetInSomeUnit = GetScoringGroupsCanBeSetInSomeUnit(course);
			var enabledScoringGroups = groupsRepo.GetEnabledAdditionalScoringGroups(courseId)
				.GroupBy(e => e.GroupId)
				.ToDictionary(g => g.Key, g => g.Select(e => e.ScoringGroupId).ToList());

			var instructors = usersRepo.GetCourseInstructors(courseId, userManager, limit: 100);
			var coursesIds = User.GetControllableCoursesId().ToList();
			var groupsMayBeCopied = groupsRepo.GetAvailableForUserGroups(coursesIds, User).ToList();
			
			var labels = groupsRepo.GetLabels(userId).ToDictionary(l => l.Id, l => l);
			var labelsOnGroups = groupsRepo.GetGroupsLabels(groupsIds);

			var groupAccesses = groupsRepo.GetGroupsAccesses(groupsIds);

			return View("Groups", new GroupsViewModel
			{
				Course = course,
				CourseManualCheckingEnabled = course.Settings.IsManualCheckingEnabled,
				Groups = groups,
				CanModifyGroup = groups.ToDictionary(g => g.Id, CanModifyGroup),
				ScoringGroupsCanBeSetInSomeUnit = scoringGroupsCanBeSetInSomeUnit,
				EnabledScoringGroups = enabledScoringGroups,
				Instructors = instructors,
				GroupsMayBeCopied = groupsMayBeCopied,
				CoursesNames = courseManager.GetCourses().ToDictionary(c => c.Id.ToLower(), c => c.Title),
				Labels = labels,
				LabelsOnGroups = labelsOnGroups,
				GroupAccesses = groupAccesses,
			});
		}

		private async Task NotifyAboutNewGroup(Group group, string initiatedUserId)
		{
			var notification = new CreatedGroupNotification
			{
				Group = group,
			};
			await notificationsRepo.AddNotification(group.CourseId, notification, initiatedUserId);
		}

		[HttpPost]
		public async Task<ActionResult> CreateGroupApi(string courseId, string name)
		{
			var ownerId = User.Identity.GetUserId();
			if (string.IsNullOrEmpty(name))
				return Json(new { status = "error", message = "Название группы не может быть пустым" });

			log.Info($"Создаю группу «{name}» для курса {courseId} (id владельца {ownerId})");

			var group = await groupsRepo.CreateGroup(courseId, name, ownerId, true, true);
			log.Info($"Группа «{name}» (Id = {group.Id}) создана");

			var course = courseManager.GetCourse(courseId);
			await UpdateEnabledScoringGroupsForGroup(course, group.Id);
			await NotifyAboutNewGroup(group, ownerId);

			return Json(new { status = "ok", groupId = group.Id });
		}

		[HttpPost]
		public async Task<ActionResult> CreateGroup(string courseId, string name, bool isPublic, bool manualChecking, bool manualCheckingForOldSolutions, string ownerId)
		{
			var currentUserId = User.Identity.GetUserId();
			if (string.IsNullOrEmpty(ownerId))
				ownerId = currentUserId;

			log.Info($"Создаю группу «{name}» для курса {courseId} (id владельца {ownerId}, публичная = {isPublic})");

			var group = await groupsRepo.CreateGroup(courseId, name, ownerId, manualChecking, manualCheckingForOldSolutions);
			log.Info($"Группа «{name}» (Id = {group.Id}) создана");

			var course = courseManager.GetCourse(courseId);
			await UpdateEnabledScoringGroupsForGroup(course, group.Id);
			await NotifyAboutNewGroup(group, currentUserId);

			return RedirectToAction("Groups", new { courseId });
		}

		[HttpPost]
		public async Task<ActionResult> CopyGroup(string courseId, int groupId, bool changeOwner)
		{
			var group = groupsRepo.FindGroupById(groupId);
			if (!CanSeeGroup(group))
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

			log.Info($"Копирую группу «{group.Name}» (Id = {group.Id}) в курс {courseId} (заменить владельца: {changeOwner})");

			var newOwnerId = changeOwner ? User.Identity.GetUserId() : "";
			var newGroup = await groupsRepo.CopyGroup(group, courseId, newOwnerId);
			log.Info($"Скопировал группу, новый Id = {newGroup.Id}");

			return RedirectToAction("Groups", new { courseId });
		}

		private bool CanSeeGroup(Group group)
		{
			if (group == null)
				return false;
			return CanModifyGroup(group);
		}

		private bool CanModifyGroup(Group group)
		{
			if (group == null)
				return false;

			var courseId = group.CourseId;
			if (groupsRepo.CanUserSeeAllCourseGroups(User, courseId))
				return true;

			var userId = User.Identity.GetUserId();
			if (group.OwnerId == userId)
				return true;

			var accesses = groupsRepo.GetGroupAccesses(group.Id);
			return accesses.Any(a => a.UserId == userId);
		}

		private bool CanAddUsersToGroupManually()
		{
			return User.IsSystemAdministrator();
		}

		[HttpPost]
		public async Task<ActionResult> AddUserToGroup(int groupId, string userId)
		{
			var group = groupsRepo.FindGroupById(groupId);
			if (!CanAddUsersToGroupManually())
				return Json(new { status = "error", message = "Вы не можете добавлять пользователей в группы"});

			var user = usersRepo.FindUserById(userId);
			if (user == null)
				return Json(new { status = "error", message = $"Пользователь с id {userId} не найден" });

			log.Info($"Добавляю пользователя {user.VisibleName} (Id = {userId}) в группу «{group.Name}» (Id = {group.Id})");
			var member = await groupsRepo.AddUserToGroup(groupId, userId);

			if (member == null)
				return Json(new { status = "error", message = $"{user.VisibleName} уже учится в этой группе" });
			return Json(new { status = "ok", html = this.RenderPartialViewToString("_GroupMember", member) });
		}

		[HttpPost]
		public async Task<ActionResult> RemoveUserFromGroup(int groupId, string userId)
		{
			var group = groupsRepo.FindGroupById(groupId);
			if (!CanModifyGroup(group))
				return Json(new { status = "error", message = "Вы не можете удалять пользователей из этой группы" });

			var user = usersRepo.FindUserById(userId);
			if (user == null)
				return Json(new { status = "error", message = $"Не могу найти пользователя {userId}" });

			log.Info($"Удаляю пользователя {user.VisibleName} (Id = {userId}) из группы «{group.Name}» (Id = {group.Id})");

			var member = await groupsRepo.RemoveUserFromGroup(groupId, userId);
			
			var currentUserId = User.Identity.GetUserId();
			if (member != null)
				await notificationsRepo.AddNotification(group.CourseId, new GroupMembersHaveBeenRemovedNotification(groupId, new List<string> { userId }, usersRepo), currentUserId);

			return Json(new { status = "ok", removed = member != null ? "true" : "false" });
		}

		[HttpPost]
		public async Task<ActionResult> RemoveUsersFromGroup(int groupId)
		{
			var group = groupsRepo.FindGroupById(groupId);
			if (!CanModifyGroup(group))
				return Json(new { status = "error", message = "Вы не можете удалять пользователей из этой группы" });

			var userIds = Request.Form.GetMultipleValues("userId");
			log.Info($"Удаляю пользователей {string.Join(", ", userIds)} из группы «{group.Name}» (Id = {group.Id})");

			var members = await groupsRepo.RemoveUsersFromGroup(groupId, userIds);
			
			var currentUserId = User.Identity.GetUserId();
			await notificationsRepo.AddNotification(group.CourseId, new GroupMembersHaveBeenRemovedNotification(group.Id, userIds, usersRepo), currentUserId);
			
			return Json(new { status = "ok", removed_count = members.Count });
		}

		[HttpPost]
		public async Task<ActionResult> CopyUsersFromOneGroupToAnother(int fromGroupId, int toGroupId)
		{
			var fromGroup = groupsRepo.FindGroupById(fromGroupId);
			if (!CanModifyGroup(fromGroup))
				return Json(new { status = "error", message = "Вы не можете копировать пользователей из этой группы" });
			var toGroup = groupsRepo.FindGroupById(toGroupId);
			if (!CanModifyGroup(toGroup))
				return Json(new { status = "error", message = "Вы не можете копировать пользователей в эту группу" });

			var userIds = Request.Form.GetMultipleValues("userId");
			log.Info($"Копирую пользователей {string.Join(", ", userIds)} из группы «{fromGroup.Name}» (Id = {fromGroup.Id}) в группу «{toGroup.Name}» (Id = {toGroup.Id})");

			var members = await groupsRepo.CopyUsersFromOneGroupToAnother(fromGroupId, toGroupId, userIds);	
			
			var currentUserId = User.Identity.GetUserId();
			await notificationsRepo.AddNotification(toGroup.CourseId, new GroupMembersHaveBeenAddedNotification(toGroup.Id, userIds, usersRepo), currentUserId);
			
			return Json(new { status = "ok", copied_count = members.Count });
		}

		[HttpPost]
		public async Task<ActionResult> UpdateGroupApi(string courseId, int groupId, bool manualChecking, bool manualCheckingForOldSolutions, bool defaultProhibitFutherReview, string name=null)
		{
			var group = groupsRepo.FindGroupById(groupId);
			if (!CanModifyGroup(group) || ! group.CourseId.EqualsIgnoreCase(courseId))
				return Json(new { status = "error", message = "Вы не можете редактировать эту группу" });

			if (name == "")
				return Json(new { status = "error", message = "Название группы не может быть пустым" });
			if (name == null)
				name = group.Name;

			log.Info($"Обновляю группу «{group.Name}» → «{name}» (Id = {group.Id}) для курса {courseId}");

			await groupsRepo.ModifyGroup(groupId, name, manualChecking, manualCheckingForOldSolutions, defaultProhibitFutherReview);

			var course = courseManager.GetCourse(group.CourseId);
			await UpdateEnabledScoringGroupsForGroup(course, groupId);

			return Json(new { status = "ok", groupId = groupId });
		}

		[HttpPost]
		public async Task<ActionResult> ChangeGroupOwnerApi(string courseId, int groupId, string newOwnerId)
		{
			var group = groupsRepo.FindGroupById(groupId);
			if (! group.CourseId.EqualsIgnoreCase(courseId) || ! CanChangeGroupOwner(group))
				return Json(new { status = "error", message = "Вы не можете сменить владельца у этой группы" });

			var newOwner = usersRepo.FindUserById(newOwnerId);
			if (newOwner == null)
				return Json(new { status = "error", message = $"Пользователь {newOwnerId} не найден" });

			log.Info($"Меняю владельца группы «{group.Name}» (Id = {group.Id}) на {newOwner.VisibleName} (Id = {newOwnerId})");

			/* Grant full access to previous owner */
			await groupsRepo.GrantAccess(groupId, group.OwnerId, GroupAccessType.FullAccess, group.OwnerId);
			/* Change owner */
			await groupsRepo.ChangeOwner(groupId, newOwnerId);
			/* Revoke access from new owner */
			await groupsRepo.RevokeAccess(groupId, newOwnerId);

			return Json(new { status = "ok", groupId = groupId });
		}

		private bool CanChangeGroupOwner(Group group)
		{
			return group.OwnerId == User.Identity.GetUserId() || User.HasAccessFor(group.CourseId, CourseRole.CourseAdmin);
		}

		[HttpPost]
		public async Task<ActionResult> ArchiveGroupApi(string courseId, int groupId, bool isArchived)
		{
			var group = groupsRepo.FindGroupById(groupId);
			if (!CanModifyGroup(group) || ! group.CourseId.EqualsIgnoreCase(courseId))
				return Json(new { status = "error", message = "Вы не можете редактировать эту группу" });

			log.Info($"Обновляю архивность группы «{group.Name}». Была ли архивна: {group.IsArchived}. Архивна теперь: {isArchived}");

			await groupsRepo.ArchiveGroup(groupId, isArchived);

			return Json(new { status = "ok", groupId = groupId });
		}

		[HttpPost]
		public async Task<ActionResult> ArchiveGroup(int groupId, bool isArchived)
		{
			var group = groupsRepo.FindGroupById(groupId);
			if (!CanModifyGroup(group))
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

			log.Info($"Обновляю архивность группы «{group.Name}». Была ли архивна: {group.IsArchived}. Архивна теперь: {isArchived}");

			await groupsRepo.ArchiveGroup(groupId, isArchived);

			return RedirectToAction("Groups", new { courseId = group.CourseId });
		}

		private async Task UpdateEnabledScoringGroupsForGroup(Course course, int groupId)
		{
			var scoringGroups = GetScoringGroupsCanBeSetInSomeUnit(course);
			var enabledScoringGroupsIds = new List<string>();
			foreach (var scoringGroup in scoringGroups)
			{
				var checkboxName = "scoring-group__" + scoringGroup.Id;
				if (!string.IsNullOrEmpty(Request.Form[checkboxName]) && Request.Form[checkboxName] != "false")
				{
					log.Info($"Включаю группу баллов «{scoringGroup.Name}» ({scoringGroup.Id}) для группы Id = {groupId}");
					enabledScoringGroupsIds.Add(scoringGroup.Id);
				}
			}
			await groupsRepo.EnableAdditionalScoringGroupsForGroup(groupId, enabledScoringGroupsIds);
		}

		[HttpPost]
		public async Task<ActionResult> RemoveGroup(int groupId)
		{
			var group = groupsRepo.FindGroupById(groupId);
			if (!CanModifyGroup(group))
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

			log.Info($"Удаляю группу «{group.Name}» (Id = {groupId})");
			await groupsRepo.RemoveGroup(groupId);

			return RedirectToAction("Groups", new { courseId = group.CourseId });
		}

		[HttpPost]
		public async Task<ActionResult> EnableGroupInviteLink(int groupId, bool isEnabled)
		{
			var group = groupsRepo.FindGroupById(groupId);
			if (!CanModifyGroup(group))
				return Json(new { status = "error", message = "Вы не можете изменять настройки ссылки для этой группы" });

			log.Info($"В{(isEnabled ? "" : "ы")}ключаю инвайт-ссылку для группы «{group.Name}» (Id = {groupId})");
			await groupsRepo.EnableInviteLink(groupId, isEnabled);

			return Json(new { status = "ok", isEnabled = isEnabled });
		}

		[HttpPost]
		public async Task<ActionResult> GrantAccessToGroup(int groupId, string userId)
		{
			var group = groupsRepo.FindGroupById(groupId);
			if (!CanModifyGroup(group))
				return Json(new { status = "error", message = "Вы не можете выдавать права на эту группу" });

			var user = usersRepo.FindUserById(userId);
			if (user == null)
				return Json(new { status = "error", message = $"Пользователь {userId} не найден" });

			if (groupsRepo.GetGroupAccesses(groupId).Select(a => a.UserId).Contains(userId) || group.OwnerId == userId)
				return Json(new { status = "error", message = $"{user.VisibleName} уже является преподавателем этой группы" });

			var currentUserId = User.Identity.GetUserId();
			var access = await groupsRepo.GrantAccess(groupId, userId, GroupAccessType.FullAccess, currentUserId);
			await notificationsRepo.AddNotification(group.CourseId, new GrantedAccessToGroupNotification { AccessId = access.Id }, currentUserId);

			var renderedHtml = this.RenderPartialViewToString("_GroupAccess", new GroupAccessViewModel
			{
				Access = access,
				CanBeRevoked = true,
			});
			return Json(new { status = "ok", html = renderedHtml });
		}

		[HttpPost]
		public async Task<ActionResult> RevokeAccessFromGroup(int groupId, string userId)
		{
			var group = groupsRepo.FindGroupById(groupId);
			if (!CanModifyGroup(group) || ! groupsRepo.CanRevokeAccess(groupId, userId, User))
				return Json(new { status = "error", message = "Вы не можете забрать права на группу у этого пользователя" });

			var currentUserId = User.Identity.GetUserId();
			var accesses = await groupsRepo.RevokeAccess(groupId, userId);
			foreach (var access in accesses)
				await notificationsRepo.AddNotification(group.CourseId, new RevokedAccessToGroupNotification { AccessId = access.Id }, currentUserId);
			return Json(new { status = "ok", userId = userId });
		}

		public ActionResult GroupInfo(int groupId)
		{
			var group = groupsRepo.FindGroupById(groupId);
			if (!CanModifyGroup(group))
				return Json(new { status = "error", message = "Вы не можете редактировать эту группу" });

			var userId = User.Identity.GetUserId();

			var accesses = groupsRepo.GetGroupAccesses(groupId);
			var accessesViewModels = accesses.Select(a => new GroupAccessViewModel
			{
				Access = a,
				CanBeRevoked = group.OwnerId == userId || a.GrantedById == userId || a.UserId == userId || User.HasAccessFor(group.CourseId, CourseRole.CourseAdmin),
			}).ToList();
			accessesViewModels.Insert(0, new GroupAccessViewModel
			{
				Access = new GroupAccess
				{
					User = group.Owner,
					AccessType = GroupAccessType.Owner,
					Group = group,
					GrantTime = group.CreateTime ?? DateTime.MaxValue,
				},
				CanBeRevoked = false,
			});

			var members = groupsRepo.GetGroupMembers(groupId).OrderBy(m => m.User.VisibleNameWithLastNameFirst);

			var enabledScoringGroups = groupsRepo.GetEnabledAdditionalScoringGroupsForGroup(groupId);

			return Json(new
			{
				status = "ok",
				inviteLink = Url.Action("JoinGroup", "Account", new { hash = group.InviteHash }, Request.GetRealScheme()),
				isInviteLinkEnabled = group.IsInviteLinkEnabled,
				accesses = accessesViewModels.Select(model => this.RenderPartialViewToString("_GroupAccess", model)).ToList(),
				group = new
				{
					name = group.Name,
					isManualCheckingEnabled = group.IsManualCheckingEnabled,
					isManualCheckingEnabledForOldSolutions = group.IsManualCheckingEnabledForOldSolutions,
					isInviteLinkEnabled = group.IsInviteLinkEnabled,
					defaultProhibitFutherReview = group.DefaultProhibitFutherReview,
				},
				members = members.Select(model => this.RenderPartialViewToString("_GroupMember", model)).ToList(),
				enabledScoringGroups = enabledScoringGroups.Select(g => g.ScoringGroupId).ToList(),
			}, JsonRequestBehavior.AllowGet);
		}
		
		public class UserSearchResultModel
		{
			public string id { get; set; }
			public string value { get; set; }
		}

		public ActionResult FindUsers(string courseId, string term, bool onlyInstructors=true, bool withGroups=true)
		{
			/* Only sysadmins can search ordinary users */
			// This limitation is disabled now for certificates generation. Waits for futher investigation
			// if (!User.IsSystemAdministrator() && !onlyInstructors)
			//   	return Json(new { status = "error", message = "Вы не можете искать среди всех пользователей" }, JsonRequestBehavior.AllowGet);

			var query = new UserSearchQueryModel { NamePrefix = term };
			if (onlyInstructors)
			{
				query.CourseRole = CourseRole.Instructor;
				query.CourseId = courseId;
				query.IncludeHighCourseRoles = true;
			}

			var users = usersRepo.FilterUsers(query, userManager, 10).ToList();
			var usersList = users.Select(ur => new UserSearchResultModel
			{
				id = ur.UserId,
				value = $"{ur.UserVisibleName} ({ur.UserName})"
			}).ToList();

			if (withGroups)
			{
				var usersIds = users.Select(u => u.UserId);
				var groupsNames = groupsRepo.GetUsersGroupsNamesAsStrings(courseId, usersIds, User);
				foreach (var user in usersList)
					if (groupsNames.ContainsKey(user.id) && !string.IsNullOrEmpty(groupsNames[user.id]))
						user.value += $": {groupsNames[user.id]}";
			}

			return Json(usersList, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Certificates(string courseId)
		{
			var course = courseManager.GetCourse(courseId);
			var certificateTemplates = certificatesRepo.GetTemplates(courseId).ToDictionary(t => t.Id);
			var certificates = certificatesRepo.GetCertificates(courseId);
			var templateParameters = certificateTemplates.ToDictionary(
				kv => kv.Key,
				kv => certificatesRepo.GetTemplateParametersWithoutBuiltins(kv.Value).ToList()
			);

			return View(new CertificatesViewModel
			{
				Course = course,
				Templates = certificateTemplates,
				TemplateParameters = templateParameters,
				Certificates = certificates
			});
		}

		[HttpPost]
		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public async Task<ActionResult> CreateCertificateTemplate(string courseId, string name, HttpPostedFileBase archive)
		{
			if (archive == null || archive.ContentLength <= 0)
			{
				log.Error("Создание шаблона сертификата: ошибка загрузки архива");
				throw new Exception("Ошибка загрузки архива");
			}

			log.Info($"Создаю шаблон сертификата «{name}» для курса {courseId}");
			var archiveName = SaveUploadedTemplate(archive);
			var template = await certificatesRepo.AddTemplate(courseId, name, archiveName);
			log.Info($"Создал шаблон, Id = {template.Id}, путь к архиву {template.ArchiveName}");

			return RedirectToAction("Certificates", new { courseId });
		}

		private string SaveUploadedTemplate(HttpPostedFileBase archive)
		{
			var archiveName = Utils.NewNormalizedGuid();
			var templateArchivePath = certificatesRepo.GetTemplateArchivePath(archiveName);
			try
			{
				archive.SaveAs(templateArchivePath.FullName);
			}
			catch (Exception e)
			{
				log.Error("Создание шаблона сертификата: не могу сохранить архив", e);
				throw;
			}
			return archiveName;
		}

		[HttpPost]
		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public async Task<ActionResult> EditCertificateTemplate(string courseId, Guid templateId, string name, HttpPostedFileBase archive)
		{
			var template = certificatesRepo.FindTemplateById(templateId);
			if (template == null || ! template.CourseId.EqualsIgnoreCase(courseId))
				return HttpNotFound();

			log.Info($"Обновляю шаблон сертификата «{template.Name}» (Id = {template.Id}) для курса {courseId}");

			if (archive != null && archive.ContentLength > 0)
			{
				var archiveName = SaveUploadedTemplate(archive);
				log.Info($"Загружен новый архив в {archiveName}");
				await certificatesRepo.ChangeTemplateArchiveName(templateId, archiveName);
			}

			await certificatesRepo.ChangeTemplateName(templateId, name);

			return RedirectToAction("Certificates", new { courseId });
		}

		[HttpPost]
		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		public async Task<ActionResult> RemoveCertificateTemplate(string courseId, Guid templateId)
		{
			var template = certificatesRepo.FindTemplateById(templateId);
			if (template == null || ! template.CourseId.EqualsIgnoreCase(courseId))
				return HttpNotFound();

			log.Info($"Удаляю шаблон сертификата «{template.Name}» (Id = {template.Id}) для курса {courseId}");
			await certificatesRepo.RemoveTemplate(template);

			return RedirectToAction("Certificates", new { courseId });
		}

		private async Task NotifyAboutCertificate(Certificate certificate)
		{
			var notification = new ReceivedCertificateNotification
			{
				Certificate = certificate,
			};
			var ulearnBotUserId = usersRepo.GetUlearnBotUserId();
			await notificationsRepo.AddNotification(certificate.Template.CourseId, notification, ulearnBotUserId);
		}

		[HttpPost]
		[ValidateInput(false)]
		public async Task<ActionResult> AddCertificate(string courseId, Guid templateId, string userId, bool isPreview = false)
		{
			var template = certificatesRepo.FindTemplateById(templateId);
			if (template == null || ! template.CourseId.EqualsIgnoreCase(courseId))
				return HttpNotFound();

			var certificateParameters = GetCertificateParametersFromRequest(template);
			if (certificateParameters == null)
				return RedirectToAction("Certificates", new { courseId });

			var certificate = await certificatesRepo.AddCertificate(templateId, userId, User.Identity.GetUserId(), certificateParameters, isPreview);

			if (isPreview)
				return RedirectToRoute("Certificate", new { certificateId = certificate.Id });

			await NotifyAboutCertificate(certificate);

			return Redirect(Url.Action("Certificates", new { courseId }) + "#template-" + templateId);
		}

		private Dictionary<string, string> GetCertificateParametersFromRequest(CertificateTemplate template)
		{
			var templateParameters = certificatesRepo.GetTemplateParametersWithoutBuiltins(template);
			var certificateParameters = new Dictionary<string, string>();
			foreach (var parameter in templateParameters)
			{
				if (Request.Unvalidated.Form["parameter-" + parameter] == null)
					return null;
				certificateParameters[parameter] = Request.Unvalidated.Form["parameter-" + parameter];
			}
			return certificateParameters;
		}

		[HttpPost]
		public async Task<ActionResult> RemoveCertificate(string courseId, Guid certificateId)
		{
			var certificate = certificatesRepo.FindCertificateById(certificateId);
			if (certificate == null)
				return HttpNotFound();

			if (!User.HasAccessFor(certificate.Template.CourseId, CourseRole.CourseAdmin) &&
				certificate.InstructorId != User.Identity.GetUserId())
				return HttpNotFound();

			await certificatesRepo.RemoveCertificate(certificate);
			return Json(new { status = "ok" });
		}

		public ActionResult DownloadCertificateTemplate(string courseId, Guid templateId)
		{
			var template = certificatesRepo.FindTemplateById(templateId);
			if (template == null || ! template.CourseId.EqualsIgnoreCase(courseId))
				return HttpNotFound();

			return RedirectPermanent($"/Certificates/{template.ArchiveName}.zip");
		}

		public ActionResult PreviewCertificates(string courseId, Guid templateId, HttpPostedFileBase certificatesData)
		{
			const string namesColumnName = "Фамилия Имя";

			var template = certificatesRepo.FindTemplateById(templateId);
			if (template == null || ! template.CourseId.EqualsIgnoreCase(courseId))
				return HttpNotFound();

			var notBuiltinTemplateParameters = certificatesRepo.GetTemplateParametersWithoutBuiltins(template).ToList();
			var builtinTemplateParameters = certificatesRepo.GetBuiltinTemplateParameters(template).ToList();
			builtinTemplateParameters.Sort();

			var model = new PreviewCertificatesViewModel
			{
				CourseId = courseId,
				Template = template,
				NotBuiltinTemplateParameters = notBuiltinTemplateParameters,
				BuiltinTemplateParameters = builtinTemplateParameters,
				Certificates = new List<PreviewCertificatesCertificateModel>(),
			};

			if (certificatesData == null || certificatesData.ContentLength <= 0)
			{
				return View(model.WithError("Ошибка загрузки файла с данными для сертификатов"));
			}

			var allUsersIds = new HashSet<string>();

			using (var parser = new TextFieldParser(new StreamReader(certificatesData.InputStream)))
			{
				parser.TextFieldType = FieldType.Delimited;
				parser.SetDelimiters(",");
				if (parser.EndOfData)
					return View(model.WithError("Пустой файл? В файле с данными должна присутствовать строка к заголовком"));

				string[] headers;
				try
				{
					headers = parser.ReadFields();
				}
				catch (MalformedLineException e)
				{
					return View(model.WithError($"Ошибка в файле с данными: в строке {parser.ErrorLineNumber}: \"{parser.ErrorLine}\". {e}"));
				}
				var namesColumnIndex = headers.FindIndex(namesColumnName);
				if (namesColumnIndex < 0)
					return View(model.WithError($"Одно из полей должно иметь имя \"{namesColumnName}\", в нём должны содержаться фамилия и имя студента. Смотрите пример файла с данными"));

				var parametersIndeces = new Dictionary<string, int>();
				foreach (var parameter in notBuiltinTemplateParameters)
				{
					parametersIndeces[parameter] = headers.FindIndex(parameter);
					if (parametersIndeces[parameter] < 0)
						return View(model.WithError($"Одно из полей должно иметь имя \"{parameter}\", в нём должна содержаться подстановка для шаблона. Смотрите пример файла с данными"));
				}

				while (!parser.EndOfData)
				{
					string[] fields;
					try
					{
						fields = parser.ReadFields();
					}
					catch (MalformedLineException e)
					{
						return View(model.WithError($"Ошибка в файле с данными: в строке {parser.ErrorLineNumber}: \"{parser.ErrorLine}\". {e}"));
					}
					if (fields.Length != headers.Length)
					{
						return View(model.WithError($"Ошибка в файле с данными: в строке {parser.ErrorLineNumber}: \"{parser.ErrorLine}\". Количество ячеек в строке не совпадает с количеством столбцов в заголовке"));
					}

					var userNames = fields[namesColumnIndex];

					var query = new UserSearchQueryModel { NamePrefix = userNames };
					var users = usersRepo.FilterUsers(query, userManager).Take(10).ToList();

					var exportedParameters = parametersIndeces.ToDictionary(kv => kv.Key, kv => fields[kv.Value]);
					var certificateModel = new PreviewCertificatesCertificateModel
					{
						UserNames = userNames,
						Users = users,
						Parameters = exportedParameters,
					};
					allUsersIds.AddAll(users.Select(u => u.UserId));
					model.Certificates.Add(certificateModel);
				}
			}

			model.GroupsNames = groupsRepo.GetUsersGroupsNamesAsStrings(courseId, allUsersIds, User);

			return View(model);
		}

		public async Task<ActionResult> GenerateCertificates(string courseId, Guid templateId, int maxCertificateId)
		{
			var template = certificatesRepo.FindTemplateById(templateId);
			if (template == null || ! template.CourseId.EqualsIgnoreCase(courseId))
				return HttpNotFound();

			var templateParameters = certificatesRepo.GetTemplateParametersWithoutBuiltins(template).ToList();
			var certificateRequests = new List<CertificateRequest>();

			for (var certificateIndex = 0; certificateIndex < maxCertificateId; certificateIndex++)
			{
				var userId = Request.Form.Get($"user-{certificateIndex}");
				if (userId == null)
					continue;
				if (string.IsNullOrEmpty(userId))
				{
					return View("GenerateCertificatesError", (object)"Не все пользователи выбраны");
				}

				var parameters = new Dictionary<string, string>();
				foreach (var parameterName in templateParameters)
				{
					var parameterValue = Request.Unvalidated.Form.Get($"parameter-{certificateIndex}-{parameterName}");
					parameters[parameterName] = parameterValue;
				}

				certificateRequests.Add(new CertificateRequest
				{
					UserId = userId,
					Parameters = parameters,
				});
			}

			foreach (var certificateRequest in certificateRequests)
			{
				var certificate = await certificatesRepo.AddCertificate(templateId, certificateRequest.UserId, User.Identity.GetUserId(), certificateRequest.Parameters);
				await NotifyAboutCertificate(certificate);
			}

			return Redirect(Url.Action("Certificates", new { courseId }) + "#template-" + templateId);
		}

		public async Task<ActionResult> GetBuiltinCertificateParametersForUser(string courseId, Guid templateId, string userId)
		{
			var template = certificatesRepo.FindTemplateById(templateId);
			if (template == null || ! template.CourseId.EqualsIgnoreCase(courseId))
				return HttpNotFound();

			var user = await userManager.FindByIdAsync(userId);
			var instructor = await userManager.FindByIdAsync(User.Identity.GetUserId());
			var course = courseManager.GetCourse(courseId);

			var builtinParameters = certificatesRepo.GetBuiltinTemplateParameters(template);
			var builtinParametersValues = builtinParameters.ToDictionary(
				p => p,
				p => certificatesRepo.GetTemplateBuiltinParameterForUser(template, course, user, instructor, p)
			);

			return Json(builtinParametersValues, JsonRequestBehavior.AllowGet);
		}

		private async Task NotifyAboutAdditionalScore(AdditionalScore score)
		{
			var notification = new ReceivedAdditionalScoreNotification
			{
				Score = score
			};
			await notificationsRepo.AddNotification(score.CourseId, notification, score.InstructorId);
		}

		[HttpPost]
		public async Task<ActionResult> SetAdditionalScore(string courseId, Guid unitId, string userId, string scoringGroupId, string score)
		{
			var course = courseManager.GetCourse(courseId);
			if (!course.Settings.Scoring.Groups.ContainsKey(scoringGroupId))
				return HttpNotFound();
			var unit = course.Units.FirstOrDefault(u => u.Id == unitId);
			if (unit == null)
				return HttpNotFound();

			var scoringGroup = unit.Scoring.Groups[scoringGroupId];
			if (string.IsNullOrEmpty(score))
			{
				await additionalScoresRepo.RemoveAdditionalScores(courseId, unitId, userId, scoringGroupId);
				return Json(new { status = "ok", score = "" });
			}

			if (!int.TryParse(score, out int scoreInt))
				return Json(new { status = "error", error = "Введите целое число" });
			if (scoreInt < 0 || scoreInt > scoringGroup.MaxAdditionalScore)
				return Json(new { status = "error", error = $"Баллы должны быть от 0 до {scoringGroup.MaxAdditionalScore}" });

			var additionalScore = await additionalScoresRepo.SetAdditionalScore(courseId, unitId, userId, scoringGroupId, scoreInt, User.Identity.GetUserId());
			await NotifyAboutAdditionalScore(additionalScore);

			return Json(new { status = "ok", score = scoreInt });
		}
		
		[HttpPost]
		public async Task<ActionResult> AddLabelToGroup(int groupId, int labelId)
		{
			var label = groupsRepo.FindLabelById(labelId);
			if (label == null || label.OwnerId != User.Identity.GetUserId())
				return Json(new { status = "error", message = "Label not found or not owned by you" });

			await groupsRepo.AddLabelToGroup(groupId, labelId);
			return Json(new { status = "ok" });
		}
		
		[HttpPost]
		public async Task<ActionResult> RemoveLabelFromGroup(int groupId, int labelId)
		{
			var label = groupsRepo.FindLabelById(labelId);
			if (label == null || label.OwnerId != User.Identity.GetUserId())
				return Json(new { status = "error", message = "Label not found or not owned by you" });

			await groupsRepo.RemoveLabelFromGroup(groupId, labelId);
			return Json(new { status = "ok" });
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
		[HttpPost]
		public async Task<ActionResult> ToggleCourseAccess(string courseId, string userId, CourseAccessType accessType, bool isEnabled)
		{
			var currentUserId = User.Identity.GetUserId();

			var userRoles = userRolesRepo.GetRoles(userId);
			var errorMessage = "Выдавать дополнительные права можно только преподавателям. Сначала назначьте пользователя администратором курса или преподавателем";
			if (!userRoles.ContainsKey(courseId))
				return Json(new { status = "error", message = errorMessage });
			if (userRoles[courseId] > CourseRole.Instructor)
				return Json(new { status = "error", message = errorMessage });

			if (isEnabled)
				await coursesRepo.GrantAccess(courseId, userId, accessType, currentUserId);
			else
				await coursesRepo.RevokeAccess(courseId, userId, accessType);

			return Json(new { status = "ok" });
		}

		[SysAdminsOnly]
		public async Task<ActionResult> StyleValidations()
		{
			return View(await styleErrorsRepo.GetStyleErrorSettingsAsync());
		}

		[SysAdminsOnly]
		[HttpPost]
		public async Task<ActionResult> EnableStyleValidation(StyleErrorType errorType, bool isEnabled)
		{
			await styleErrorsRepo.EnableStyleErrorAsync(errorType, isEnabled);
			return Json(new { status = "ok" });
		}
	}

	public class CertificateRequest
	{
		public string UserId;
		public Dictionary<string, string> Parameters;
	}

	public class PreviewCertificatesCertificateModel
	{
		public string UserNames { get; set; }
		public List<UserRolesInfo> Users { get; set; }
		public Dictionary<string, string> Parameters { get; set; }
	}

	public class PreviewCertificatesViewModel
	{
		public string Error { get; set; }

		public string CourseId { get; set; }
		public CertificateTemplate Template { get; set; }
		public List<PreviewCertificatesCertificateModel> Certificates { get; set; }
		public List<string> NotBuiltinTemplateParameters { get; set; }
		public List<string> BuiltinTemplateParameters { get; set; }
		public Dictionary<string, string> GroupsNames { get; set; }

		public PreviewCertificatesViewModel WithError(string error)
		{
			return new PreviewCertificatesViewModel
			{
				CourseId = CourseId,
				Template = Template,
				Error = error,
			};
		}
	}

	public class CertificatesViewModel
	{
		public Course Course { get; set; }

		public Dictionary<Guid, CertificateTemplate> Templates { get; set; }
		public Dictionary<Guid, List<Certificate>> Certificates { get; set; }
		public Dictionary<Guid, List<string>> TemplateParameters { get; set; }
	}

	public class UnitsListViewModel
	{
		public string CourseId;
		public string CourseTitle;
		public DateTime CurrentDateTime;
		public List<Tuple<Unit, UnitAppearance>> Units;

		public UnitsListViewModel(string courseId, string courseTitle, List<Tuple<Unit, UnitAppearance>> units,
			DateTime currentDateTime)
		{
			CourseId = courseId;
			CourseTitle = courseTitle;
			Units = units;
			CurrentDateTime = currentDateTime;
		}
	}

	public class CourseListViewModel
	{
		public List<CourseViewModel> Courses;
		public string CourseCreationLastTry { get; set; }
		public string InvalidCharacters { get; set; }
	}

	public class CourseViewModel
	{
		public string Title { get; set; }
		public string Id { get; set; }
		public DateTime LastWriteTime { get; set; }
	}

	public class PackagesViewModel
	{
		public string CourseId { get; set; }
		public bool HasPackage { get; set; }
		public DateTime LastUpdate { get; set; }
		public List<CourseVersion> Versions { get; set; }
		public CourseVersion PublishedVersion { get; set; }
		public string Error { get; set; }
	}

	public class AdminCommentsViewModel
	{
		public string CourseId { get; set; }
		public bool IsCommentsEnabled { get; set; }
		public CommentModerationPolicy ModerationPolicy { get; set; }
		public bool OnlyInstructorsCanReply { get; set; }
		public List<CommentViewModel> Comments { get; set; }
	}

	public class ManualCheckingQueueViewModel
	{
		public string CourseId { get; set; }
		public List<ManualCheckingQueueItemViewModel> Checkings { get; set; }
		public string Message { get; set; }
		public List<Group> Groups { get; set; }
		public List<string> SelectedGroupsIds { get; set; }
		public string SelectedGroupsIdsJoined => string.Join(",", SelectedGroupsIds);
		public bool AlreadyChecked { get; set; }
		public bool ExistsMore { get; set; }
		public bool ShowFilterForm { get; set; }
		public List<KeyValuePair<Guid, string>> SlidesTitles { get; set; }
	}

	public class ManualCheckingQueueItemViewModel
	{
		public AbstractManualSlideChecking CheckingQueueItem { get; set; }

		public Guid ContextSlideId { get; set; }
		public string ContextSlideTitle { get; set; }
		public int ContextMaxScore { get; set; }
		public List<ExerciseCodeReview> ContextReviews { get; set; }
		public string ContextExerciseSolution { get; set; }
		public DateTime ContextTimestamp { get; set; }
	}

	public class DiagnosticsModel
	{
		public string CourseId { get; set; }

		public bool IsDiagnosticsForVersion { get; set; }
		public bool IsVersionPublished { get; set; }
		public Guid VersionId { get; set; }
		public CourseDiff CourseDiff { get; set; }
	}

	public class GroupsViewModel
	{
		public Course Course { get; set; }
		public bool CourseManualCheckingEnabled { get; set; }

		public List<Group> Groups { get; set; }
		public Dictionary<int, bool> CanModifyGroup { get; set; }
		public List<ScoringGroup> ScoringGroupsCanBeSetInSomeUnit { get; set; }
		public Dictionary<int, List<string>> EnabledScoringGroups { get; set; }

		public List<UserRolesInfo> Instructors { get; set; }
		public List<Group> GroupsMayBeCopied { get; set; }
		public Dictionary<string, string> CoursesNames { get; set; }

		public Dictionary<int, GroupLabel> Labels { get; set; }
		public DefaultDictionary<int, List<int>> LabelsOnGroups { get; set; }

		public DefaultDictionary<int, List<GroupAccess>> GroupAccesses { get; set; }
	}

	public class GroupAccessViewModel
	{
		public GroupAccess Access { get; set; }

		public bool CanBeRevoked { get; set; }
	}
}