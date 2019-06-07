using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Ulearn.Web.Api.Authorization
{
	public class BaseCourseAuthorizationHandler<T> : AuthorizationHandler<T> where T : IAuthorizationRequirement
	{
		protected readonly ILogger logger;

		public BaseCourseAuthorizationHandler(ILogger logger)
		{
			this.logger = logger;
		}

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, T requirement)
		{
			throw new System.NotImplementedException();
		}
		
		/* Find `course_id` arguments in request. Try to get course_id in following order:
		 * route data (/groups/<course_id>/)
		 * query string (/groups/?course_id=<course_id>)
		 * NOTE: not supported JSON request body})
		 */
		protected string GetCourseIdFromRequestAsync(AuthorizationFilterContext mvcContext)
		{
			/* 1. Route data */
			var routeData = mvcContext.RouteData;
			if (routeData.Values["courseId"] is string courseIdFromRoute)
				return courseIdFromRoute;

			var courseIdFromQuery = mvcContext.HttpContext.Request.Query["course_id"].FirstOrDefault();
			if (!courseIdFromQuery.IsNullOrEmpty())
				return courseIdFromQuery;

			logger.Error("Can't find `courseId` parameter in request for checking course role requirement. You should inherit your parameters models from ICourseAuthorizationParameters.");
			return null;
		}
	}
}