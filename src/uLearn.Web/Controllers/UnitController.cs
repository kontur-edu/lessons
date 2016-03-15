using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using uLearn.SlideIdChecking;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using uLearn.Web.DataContexts;
using uLearn.Web.FilterAttributes;
using uLearn.Web.Models;

namespace uLearn.Web.Controllers
{
	[ULearnAuthorize(MinAccessLevel = CourseRole.CourseAdmin)]
	public class UnitController : Controller
	{
		private readonly CourseManager courseManager;
		private readonly ULearnDb db;
		private readonly UsersRepo usersRepo;
		private readonly CommentsRepo commentsRepo;
		private readonly UserManager<ApplicationUser> userManager;

		public UnitController()
		{
			db = new ULearnDb();
			courseManager = WebCourseManager.Instance;
			usersRepo = new UsersRepo(db);
			commentsRepo = new CommentsRepo(db);
			userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ULearnDb()));
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

		[HttpPost]
		public ActionResult ReloadCourse(string courseId, string returnUrl = null)
		{
			courseManager.ReloadCourse(courseId);
			if (returnUrl != null)
				return Redirect(returnUrl);
			return RedirectToAction("CourseList", new { courseId });
		}

		public ActionResult SpellingErrors(string courseId)
		{
			var course = courseManager.GetCourse(courseId);
			return PartialView(course.SpellCheck());
		}

		public ActionResult List(string courseId)
		{
			var course = courseManager.GetCourse(courseId);
			var appearances = db.Units.Where(u => u.CourseId == course.Id).ToList();
			var unitAppearances =
				course.Slides
					.Select(s => s.Info.UnitName)
					.Distinct()
					.Select(unitName => Tuple.Create(unitName, appearances.FirstOrDefault(a => a.UnitName.RemoveBom() == unitName)))
					.ToList();
			return View(new UnitsListViewModel(course.Id, course.Title, unitAppearances, DateTime.Now));
		}

		[HttpPost]
		public async Task<RedirectToRouteResult> SetPublishTime(string courseId, string unitName, string publishTime)
		{

			var oldInfo = await db.Units.Where(u => u.CourseId == courseId && u.UnitName == unitName).ToListAsync();
			db.Units.RemoveRange(oldInfo);
			var unitAppearance = new UnitAppearance
			{
				CourseId = courseId,
				UnitName = unitName,
				UserName = User.Identity.Name,
				PublishTime = DateTime.Parse(publishTime),
			};
			db.Units.Add(unitAppearance);
			await db.SaveChangesAsync();
			return RedirectToAction("List", new { courseId });
		}

		[HttpPost]
		public async Task<RedirectToRouteResult> RemovePublishTime(string courseId, string unitName)
		{
			var unitAppearance = await db.Units.FirstOrDefaultAsync(u => u.CourseId == courseId && u.UnitName == unitName);
			if (unitAppearance != null)
			{
				db.Units.Remove(unitAppearance);
				await db.SaveChangesAsync();
			}
			return RedirectToAction("List", new { courseId });
		}

		public ActionResult DownloadPackage(string courseId)
		{
			var packageName = courseManager.GetPackageName(courseId);
			return File(courseManager.GetStagingCoursePath(courseId), "application/zip", packageName);
		}

		[HttpPost]
		public ActionResult UploadCourse(string courseId, HttpPostedFileBase file)
		{
			if (file == null || file.ContentLength <= 0)
				return RedirectToAction("Packages", new { courseId });

			var fileName = Path.GetFileName(file.FileName);
			if (fileName == null || !fileName.ToLower().EndsWith(".zip"))
				return RedirectToAction("Packages", new { courseId });

			var packageName = courseManager.GetPackageName(courseId);
			var destinationFile = courseManager.StagedDirectory.GetFile(packageName);
			file.SaveAs(destinationFile.FullName);
			courseManager.ReloadCourse(courseId);
			return RedirectToAction("Diagnostics", new { courseId });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ULearnAuthorize(Roles = LmsRoles.SysAdmin)]
		public ActionResult CreateCourse(string courseId)
		{
			if (!courseManager.TryCreateCourse(courseId))
				return RedirectToAction("CourseList", new { courseCreationLastTry = courseId });
			return RedirectToAction("Users", new { courseId, onlyPrivileged = true });
		}

		public ActionResult ManageMenu(string courseId)
		{
			var course = courseManager.GetCourse(courseId);
			return PartialView(new ManageMenuViewModel
			{
				CourseId = courseId,
				Title = course.Title
			});
		}

		public ActionResult Packages(string courseId)
		{
			var hasPackage = courseManager.HasPackageFor(courseId);
			var lastUpdate = courseManager.GetLastWriteTime(courseId);
			return View(model: new PackagesViewModel
			{
				CourseId = courseId,
				HasPackage = hasPackage,
				LastUpdate = lastUpdate
			});
		}

		public ActionResult Comments(string courseId)
		{
			var course = courseManager.GetCourse(courseId);
			var commentsPolicy = commentsRepo.GetCommentsPolicy(courseId);
			
			var comments = commentsRepo.GetCourseComments(courseId).OrderByDescending(x => x.PublishTime).ToList();
			var commentsLikes = commentsRepo.GetCommentsLikesCounts(comments);
			var commentsLikedByUser = commentsRepo.GetCourseCommentsLikedByUser(courseId, User.Identity.GetUserId());
			var commentsById = comments.ToDictionary(x => x.Id);

			return View(new AdminCommentsViewModel
			{
				CourseId = courseId,
				IsCommentsEnabled = commentsPolicy.IsCommentsEnabled,
				ModerationPolicy = commentsPolicy.ModerationPolicy,
				OnlyInstructorsCanReply = commentsPolicy.OnlyInstructorsCanReply,
				Comments = comments.Select(c => new CommentViewModel
				{
					Comment = c,
					LikesCount = commentsLikes.Get(c.Id, 0),
					IsLikedByUser = commentsLikedByUser.Contains(c.Id),
					Replies = new List<CommentViewModel>(),
					CanEditAndDeleteComment = true,
					CanModerateComment = true,
					IsCommentVisibleForUser = true,
					ShowContextInformation = true,
					ContextSlideTitle = course.GetSlideById(c.SlideId).Title,
					ContextParentComment = c.IsTopLevel() ? null : commentsById[c.ParentCommentId].Text,
				}).ToList()
			});
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
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

		public ActionResult Users(UserSearchQueryModel queryModel)
		{
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

			var model = new UserListModel
			{
				IsCourseAdmin = true,
				ShowDangerEntities = false,
				Users = new List<UserModel>()
			};

			foreach (var userRolesInfo in userRoles)
			{
				var user = new UserModel(userRolesInfo);

				List<CourseRole> roles;
				if (!rolesForUsers.TryGetValue(userRolesInfo.UserId, out roles))
					roles = new List<CourseRole>();

				user.CoursesAccess = Enum.GetValues(typeof(CourseRole))
					.Cast<CourseRole>()
					.Where(courseRole => courseRole != CourseRole.Student)
					.ToDictionary(
						courseRole => courseRole.ToString(),
						courseRole => (ICoursesAccessListModel)new SingleCourseAccessModel
						{
							HasAccess = roles.Contains(courseRole),
							ToggleUrl = Url.Action("ToggleRole", "Account", new { courseId, userId = user.UserId, role = courseRole })
						});

				model.Users.Add(user);
			}

			return model;
		}

		public ActionResult CheckSlideIds(string courseId)
		{
			var course = courseManager.GetCourse(courseId);
			var model = new CheckSlideIdsViewModel
			{
				InnerErrors = course.FindDuplicatedIds(),
				OuterErrors = courseManager.FindGlobalDuplicatedIdsFor(courseId)
			};
			return PartialView(model);
		}

		public ActionResult Diagnostics(string courseId)
		{
			return View(model: courseId);
		}
	}

	public class UnitsListViewModel
	{
		public string CourseId;
		public string CourseTitle;
		public DateTime CurrentDateTime;
		public List<Tuple<string, UnitAppearance>> Units;

		public UnitsListViewModel(string courseId, string courseTitle, List<Tuple<string, UnitAppearance>> units,
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

	public class ManageMenuViewModel
	{
		public string CourseId { get; set; }
		public string Title { get; set; }
	}

	public class PackagesViewModel
	{
		public string CourseId { get; set; }
		public bool HasPackage { get; set; }
		public DateTime LastUpdate { get; set; }
	}

	public class CheckSlideIdsViewModel
	{
		public List<LocalSlideIdErrorList> InnerErrors { get; set; }
		public List<GlobalSlideIdErrorList> OuterErrors { get; set; }
	}

	public class AdminCommentsViewModel
	{
		public string CourseId { get; set; }
		public bool IsCommentsEnabled { get; set; }
		public CommentModerationPolicy ModerationPolicy { get; set; }
		public bool OnlyInstructorsCanReply { get; set; }
		public List<CommentViewModel> Comments { get; set; }
	}
}