using System;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Models.Comments;
using Database.Repos.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Units;
using Ulearn.Web.Api.Models.Common;
using Ulearn.Web.Api.Models.Responses.Notifications;

namespace Ulearn.Web.Api.Controllers
{
	/* See https://docs.microsoft.com/en-us/aspnet/core/web-api/index?view=aspnetcore-2.1#annotate-class-with-apicontrollerattribute
	   for detailed description of ApiControllerAttribute */
	[ApiController]
	[Produces("application/json")]
	public class BaseController : Controller
	{
		protected readonly ILogger logger;
		protected readonly IWebCourseManager courseManager;
		protected readonly UlearnDb db;
		protected readonly IUsersRepo usersRepo;

		protected string UserId => User.GetUserId();
		protected bool IsAuthenticated => User.Identity.IsAuthenticated;

		public BaseController(ILogger logger, IWebCourseManager courseManager, UlearnDb db, IUsersRepo usersRepo)
		{
			this.logger = logger ?? throw new ArgumentException(nameof(logger));
			this.courseManager = courseManager ?? throw new ArgumentException(nameof(courseManager));
			this.db = db ?? throw new ArgumentException(nameof(db));
			this.usersRepo = usersRepo ?? throw new ArgumentException(nameof(usersRepo));
		}

		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			DisableEfChangesTrackingForGetRequests(context);
			
			await next().ConfigureAwait(false);
		}

		private void DisableEfChangesTrackingForGetRequests(ActionContext context)
		{
			/* Disable change tracking in EF Core for GET requests due to performance issues */
			/* TODO (andgein): we need a way to enable change tracking for some GET requests in future */
			var isRequestSafe = context.HttpContext.Request.Method == "GET"; // Maybe for HEAD and OPTION requests too?
			db.ChangeTracker.AutoDetectChangesEnabled = !isRequestSafe;

			if (isRequestSafe)
			{
				logger.Information("Выключаю автоматическое отслеживание изменений в EF Core: db.ChangeTracker.AutoDetectChangesEnabled = false");
			}
		}

		protected async Task<bool> IsSystemAdministratorAsync()
		{
			var user = await usersRepo.FindUserByIdAsync(UserId).ConfigureAwait(false);
			return usersRepo.IsSystemAdministrator(user);
		}

		protected UnitInfo BuildUnitInfo(string courseId, Unit unit)
		{
			return new UnitInfo
			{
				Id = unit.Id,
				Title = unit.Title,
				Slides = unit.Slides.Select(slide => BuildSlideInfo(courseId, slide)).ToList()
			};
		}

		protected ShortSlideInfo BuildSlideInfo(string courseId, Slide slide)
		{
			return new ShortSlideInfo
			{
				Id = slide.Id,
				Title = slide.Title,
				Slug = slide.Url,
				ApiUrl = Url.Action(nameof(SlidesController.SlideInfo), "Slides", new { courseId = courseId, slideId = slide.Id }),
				MaxScore = slide.MaxScore,
				Type = GetSlideType(slide)
			};
		}

		protected static SlideType GetSlideType(Slide slide)
		{
			switch (slide)
			{
				case ExerciseSlide _:
					return SlideType.Exercise;
				case QuizSlide _:
					return SlideType.Quiz;
				default:
					return SlideType.Lesson;
			}
		}

		protected ShortUserInfo BuildShortUserInfo(ApplicationUser user, bool discloseLogin=false, bool discloseEmail=false)
		{
			return new ShortUserInfo
			{
				Id = user.Id,
				Login = discloseLogin ? user.UserName : null,
				Email = discloseEmail ? user.Email : null,
				FirstName = user.FirstName ?? "",
				LastName = user.LastName ?? "",
				VisibleName = user.VisibleName,
				AvatarUrl = user.AvatarUrl,
				Gender = user.Gender,
			};
		}		

		protected NotificationCommentInfo BuildNotificationCommentInfo(Comment comment)
		{
			if (comment == null)
				return null;
			
			return new NotificationCommentInfo
			{
				Id = comment.Id,
				CourseId = comment.CourseId,
				SlideId = comment.SlideId,
				PublishTime = comment.PublishTime,
				Author = BuildShortUserInfo(comment.Author),
				Text = comment.Text,
			};
		}
	}
}