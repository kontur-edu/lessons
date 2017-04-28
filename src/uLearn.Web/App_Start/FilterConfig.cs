using System;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace uLearn.Web
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
			var requireHttps = Convert.ToBoolean(WebConfigurationManager.AppSettings["ulearn.requireHttps"] ?? "true");
			if (requireHttps)
				filters.Add(new RequireHttpsForCloudFlareAttribute());
			filters.Add(new AntiForgeryTokenFilter());
		}
	}

	public class AntiForgeryTokenFilter : FilterAttribute, IExceptionFilter
	{
		public void OnException(ExceptionContext filterContext)
		{
			if (!(filterContext.Exception is HttpAntiForgeryException))
				return;

			if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
				filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			else
				filterContext.Result = new RedirectResult("/");

			filterContext.ExceptionHandled = true;
		}
	}

	public static class HttpRequestExtensions
	{
		private static readonly string xSchemeHeaderName = "X-Scheme";

		/* Return scheme from request of from header X-Scheme if request has been proxied by cloudflare or nginx or ... */
		public static string GetRealScheme(this HttpRequestBase request)
		{
			return request.Headers[xSchemeHeaderName] ?? request.Url?.Scheme;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class RequireHttpsForCloudFlareAttribute : RequireHttpsAttribute
	{
		/* Additionally view real scheme from headers. If it equals to "HTTPS", continue work */
		protected override void HandleNonHttpsRequest(AuthorizationContext filterContext)
		{
			if (string.Equals(filterContext.HttpContext.Request.GetRealScheme(), "HTTPS", StringComparison.OrdinalIgnoreCase))
				return;
			if (!string.Equals(filterContext.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase) && 
				!string.Equals(filterContext.HttpContext.Request.HttpMethod, "HEAD", StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException("Require HTTPS");
			var url = "https://" + filterContext.HttpContext.Request.Url?.Host + filterContext.HttpContext.Request.RawUrl;
			filterContext.Result = new RedirectResult(url);
		}
	}
}