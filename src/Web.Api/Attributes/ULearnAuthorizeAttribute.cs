using System.Threading.Tasks;
using Database.Extensions;
using Database.Models;
using Database.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using Ulearn.Common.Extensions;

namespace Ulearn.Web.Api.Attributes
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
	/*public class UlearnAuthorizeAttribute : TypeFilterAttribute
	{
		public CourseRole MinAccessLevel = CourseRole.Student;

		public UlearnAuthorizeAttribute(Type type) : base(type)
		{
		}

		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
		{
			var request = filterContext.HttpContext.Request;
			if (request.HttpMethod == "POST" && request.UrlReferrer != null)
			{
				filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { action = "Index", controller = "Login", returnUrl = request.UrlReferrer.PathAndQuery }));
				return;
			}
			base.HandleUnauthorizedRequest(filterContext);
		}

		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			if (httpContext == null)
				throw new ArgumentNullException(nameof(httpContext));

			var user = httpContext.User;
			if (!user.Identity.IsAuthenticated)
				return false;

			if (MinAccessLevel == CourseRole.Student && ! ShouldBeSysAdmin)
				return true;

			if (ShouldBeSysAdmin && user.IsSystemAdministrator())
				return true;

			if (MinAccessLevel == CourseRole.Student)
				return false;

			var courseIds = httpContext.Request.Unvalidated.QueryString.GetValues("courseId");
			if (courseIds == null)
				return user.HasAccess(MinAccessLevel);

			if (courseIds.Length != 1)
				return false;

			return user.HasAccessFor(courseIds[0], MinAccessLevel);
		}

		public bool ShouldBeSysAdmin { get; set; }

		public new string Users
		{
			set { throw new NotSupportedException(); }
		}

		private static readonly char[] delims = { ',' };

		private static string[] SplitString(string original)
		{
			if (String.IsNullOrEmpty(original))
			{
				return new string[0];
			}

			return original
				.Split(delims)
				.Select(piece => piece.Trim())
				.Where(s => !String.IsNullOrEmpty(s))
				.ToArray();
		}
	}*/
}