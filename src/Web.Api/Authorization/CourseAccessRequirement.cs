using System.Threading.Tasks;
using Database.Extensions;
using Database.Models;
using Database.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using Ulearn.Common.Extensions;

namespace Ulearn.Web.Api.Authorization
{
	public class CourseAccessRequirement: IAuthorizationRequirement
	{
		public readonly CourseAccessType CourseAccessType;

		public CourseAccessRequirement(CourseAccessType courseAccessType)
		{
			CourseAccessType = courseAccessType;
		}
	}
	
	/* TODO (andgein): extract common logic to BaseCourseHandler */
	public class CourseAccessAuthorizationHandler : AuthorizationHandler<CourseAccessRequirement>
	{
		private readonly ILogger logger;
		private readonly CoursesRepo coursesRepo;
		private readonly UserRolesRepo userRolesRepo;

		public CourseAccessAuthorizationHandler(CoursesRepo coursesRepo, UserRolesRepo userRolesRepo, ILogger logger)
		{
			this.coursesRepo = coursesRepo;
			this.userRolesRepo = userRolesRepo;
			this.logger = logger;
		}

		protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CourseAccessRequirement requirement)
		{
			/* Get MVC context. See https://docs.microsoft.com/en-US/aspnet/core/security/authorization/policies#accessing-mvc-request-context-in-handlers */
			if (!(context.Resource is AuthorizationFilterContext mvcContext))
			{
				logger.Error("Can't get MVC context in CourseRoleAuthenticationHandler");
				context.Fail();
				return;
			}
			
			var routeData = mvcContext.RouteData;
			if (!(routeData.Values["courseId"] is string courseId))
			{
				logger.Error("Can't find `courseId` parameter in route data for checking course access requirement.");
				context.Fail();
				return;
			}

			if (context.User.IsSystemAdministrator())
			{
				context.Succeed(requirement);
				return;
			}

			var userId = context.User.GetUserId();

			var isCourseAdmin = await userRolesRepo.HasUserAccessToCourseAsync(userId, courseId, CourseRole.CourseAdmin);
			if (isCourseAdmin || await coursesRepo.HasCourseAccessAsync(userId, courseId, requirement.CourseAccessType))
				context.Succeed(requirement);
			else
				context.Fail();
		}
	}

	public class CourseAccessAuthorizeAttribute : AuthorizeAttribute
	{
		public CourseAccessAuthorizeAttribute(CourseAccessType accessType)
			: base(accessType.GetAuthorizationPolicyName())
		{
		}
	}
}