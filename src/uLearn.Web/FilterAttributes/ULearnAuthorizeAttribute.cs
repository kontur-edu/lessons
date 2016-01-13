using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using uLearn.Web.Models;

namespace uLearn.Web.FilterAttributes
{
	public class ULearnAuthorizeAttribute : AuthorizeAttribute
	{
		public CourseRoles MinAccessLevel = CourseRoles.Student;

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
				throw new ArgumentNullException("httpContext");

			var user = httpContext.User;
			if (!user.Identity.IsAuthenticated)
				return false;

			if (MinAccessLevel == CourseRoles.Student && rolesSplit.Length == 0)
				return true;

			if (rolesSplit.Length > 0 && rolesSplit.Any(user.IsInRole))
				return true;

			if (MinAccessLevel == CourseRoles.Student)
				return false;

			var courseIds = httpContext.Request.Params.Get("courseId");
			if (courseIds == null)
				return user.HasAccess(MinAccessLevel);

			var courseId = courseIds.Split(delims).SingleOrDefault();
			return user.HasAccessFor(courseId, MinAccessLevel);
		}

		private string[] rolesSplit = new string[0];

		public new string Roles
		{
			get { return string.Join(",", rolesSplit); }
			set { rolesSplit = SplitString(value); }
		}

		public new string Users
		{
			set { throw new NotImplementedException(); }
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
	}
}