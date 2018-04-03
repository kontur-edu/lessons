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
	public class CourseRoleRequirement : IAuthorizationRequirement
	{
		public readonly CourseRole MinCourseRole;

		public CourseRoleRequirement(CourseRole minCourseRole)
		{
			MinCourseRole = minCourseRole;
		}
	}
	
	public class CourseRoleAuthorizationHandler : AuthorizationHandler<CourseRoleRequirement>
	{
		private readonly UserRolesRepo userRolesRepo;
		private readonly ILogger logger;

		public CourseRoleAuthorizationHandler(UserRolesRepo userRolesRepo, ILogger logger)
		{
			this.userRolesRepo = userRolesRepo;
			this.logger = logger;
		}

		protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CourseRoleRequirement requirement)
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
				logger.Error("Can't find `courseId` parameter in route data for checking course role requirement.");
				context.Fail();
				return;
			}

			if (context.User.IsSystemAdministrator())
			{
				context.Succeed(requirement);
				return;
			}

			var userId = context.User.GetUserId();

			if (await userRolesRepo.HasUserAccessToCourseAsync(userId, courseId, requirement.MinCourseRole))
				context.Succeed(requirement);
			else
				context.Fail();
		}
	}
}