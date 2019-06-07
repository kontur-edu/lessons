using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using Database.DataContexts;
using log4net;
using LtiLibrary.Core.Extensions;
using Microsoft.AspNet.Identity;
using NUnit.Framework.Internal;
using uLearn.Web.Kontur.Passport;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Configuration;
using Web.Api.Configuration;

namespace uLearn.Web
{
	public static class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
			
			/* Next filter serves built index.html from ../Frontend/build/ (appSettings/ulearn.react.index.html).
			   Before running this code build Frontend project via `yarn build` or `npm run build` */
			var indexHtmlPath = WebConfigurationManager.AppSettings["ulearn.react.index.html"];
			var appDirectory = new DirectoryInfo(Utils.GetAppPath());
			filters.Add(new ServeStaticFileForEveryNonAjaxRequest(appDirectory.GetFile(indexHtmlPath), excludedPrefixes: new List<string>
			{
				"/elmah/",
				"/Certificate/",
				"/Analytics/ExportCourseStatisticsAs",
				"/Exercise/StudentZip",
				"/Content/"
			}));
			
			var requireHttps = Convert.ToBoolean(WebConfigurationManager.AppSettings["ulearn.requireHttps"] ?? "true");
			if (requireHttps)
				filters.Add(new RequireHttpsForCloudFlareAttribute());
			filters.Add(new AntiForgeryTokenFilter());
			filters.Add(new KonturPassportRequiredFilter());
		}
	}

	public class AntiForgeryTokenFilter : FilterAttribute, IExceptionFilter
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(AntiForgeryTokenFilter));
		public void OnException(ExceptionContext filterContext)
		{
			if (!(filterContext.Exception is HttpAntiForgeryException))
				return;

			if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
				filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			else
				filterContext.Result = new RedirectResult("/");

			log.Info($"{nameof(AntiForgeryTokenFilter)} did his job");
			filterContext.ExceptionHandled = true;
		}
	}
	
	public class HandleHttpAntiForgeryException : ActionFilterAttribute, IExceptionFilter
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(HandleHttpAntiForgeryException));
		public void OnException(ExceptionContext filterContext)
		{
			if (!(filterContext.Exception is HttpAntiForgeryException))
				return;

			if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
				filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			else
				filterContext.Result = new RedirectResult("/");

			log.Info($"{nameof(HandleHttpAntiForgeryException)} did his job");
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

		public static int GetRealPort(this HttpRequestBase request)
		{
			if (request.Url?.Scheme == "http" && request.Url.Port == 80 && request.GetRealScheme() == "https")
				return 443;
			return request.Url?.Port ?? 80;
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
			{
				filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden, "SSL required"); // 403.4 - SSL required
				return;
			}
			var url = "https://" + filterContext.HttpContext.Request.Url?.Host + filterContext.HttpContext.Request.RawUrl;
			filterContext.Result = new RedirectResult(url);
		}
	}

	public class ServeStaticFileForEveryNonAjaxRequest : ActionFilterAttribute
	{
		private readonly List<string> excludedPrefixes;
		private readonly byte[] content;
		
		public ServeStaticFileForEveryNonAjaxRequest(FileInfo file, List<string> excludedPrefixes)
		{
			this.excludedPrefixes = excludedPrefixes;
			content = File.ReadAllBytes(file.FullName);
			content = InsertFrontendConfiguration(content);
		}

		private static byte[] InsertFrontendConfiguration(byte[] content)
		{
			var configuration = ApplicationConfiguration.Read<WebApiConfiguration>();
			var frontendConfigJson = configuration.Frontend.ToJsonString();
			var decodedContent = Encoding.UTF8.GetString(content);
			var regex = new Regex(@"(window.config\s*=\s*)(\{\})");
			var contentWithConfig = regex.Replace(decodedContent, "$1" + frontendConfigJson);

			return Encoding.UTF8.GetBytes(contentWithConfig);
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var httpContext = filterContext.RequestContext.HttpContext;
			
			foreach (var prefix in excludedPrefixes)
				if (httpContext.Request.Url != null && httpContext.Request.Url.LocalPath.StartsWith(prefix))
					return;
			
			var acceptHeader = httpContext.Request.Headers["Accept"] ?? "";
			var cspHeader = WebConfigurationManager.AppSettings["ulearn.web.cspHeader"] ?? "";
			if (acceptHeader.Contains("text/html") && httpContext.Request.HttpMethod == "GET")
			{
				filterContext.HttpContext.Response.Headers.Add("Content-Security-Policy-Report-Only", cspHeader);
				filterContext.Result = new FileContentResult(content, "text/html");
			}
		}

		public override void OnResultExecuting(ResultExecutingContext filterContext)
		{
			/* Add no-cache headers for correct working of react application (otherwise clicking on `back` button in browsers loads cached not-reacted version) */			
			var cache = filterContext.HttpContext.Response.Cache;
			cache.SetExpires(DateTime.UtcNow.AddDays(-1));
			cache.SetValidUntilExpires(false);
			cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
			cache.SetCacheability(HttpCacheability.NoCache);
			cache.SetNoStore();
			
			base.OnResultExecuting(filterContext);
		}
	}

	public class KonturPassportRequiredFilter : ActionFilterAttribute
	{
		/* If query string contains &kontur=true then we need to check kontur.passport login */
		private const string queryStringParameterName = "kontur";

		private readonly ULearnUserManager userManager;

		public KonturPassportRequiredFilter(ULearnUserManager userManager)
		{
			this.userManager = userManager;
		}

		public KonturPassportRequiredFilter()
			: this(new ULearnUserManager(new ULearnDb()))
		{
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var httpContext = filterContext.RequestContext.HttpContext;

			var queryString = httpContext.Request.QueryString ?? new NameValueCollection();
			var konturPassportRequired = Convert.ToBoolean(queryString.Get(queryStringParameterName) ?? "false");

			if (!konturPassportRequired)
				return;

			var originalUrl = "";
			var requestUrl = httpContext.Request.Url;
			if (requestUrl != null)
			{
				/* Substitute http(s) scheme with real scheme from header */
				var realScheme = filterContext.RequestContext.HttpContext.Request.GetRealScheme();
				var realPort = filterContext.RequestContext.HttpContext.Request.GetRealPort();
				requestUrl = new UriBuilder(requestUrl) { Scheme = realScheme, Port = realPort }.Uri;

				originalUrl = requestUrl.ToString().RemoveQueryParameter(queryStringParameterName) ?? "";
			}

			var isAuthenticated = httpContext.User.Identity.IsAuthenticated;
			if (isAuthenticated)
			{
				var userId = httpContext.User.Identity.GetUserId();
				var user = userManager.FindById(userId);
				var hasKonturPassportLogin = user.Logins.Any(l => l.LoginProvider == KonturPassportConstants.AuthenticationType);
				if (hasKonturPassportLogin)
				{
					filterContext.Result = new RedirectResult(originalUrl);
					return;
				}

				/* Try to link Kontur.Passport account to current user */
				filterContext.Result = RedirectToAction("LinkLogin", "Login", new
				{
					provider = KonturPassportConstants.AuthenticationType,
					returnUrl = originalUrl,
				});
			}
			else
			{
				filterContext.Result = RedirectToAction("EnsureKonturProfileLogin", "Login", new
				{
					returnUrl = originalUrl
				});
			}
		}

		private ActionResult RedirectToAction(string actionName, string controllerName, RouteValueDictionary values)
		{
			if (string.IsNullOrEmpty(actionName))
				throw new ArgumentNullException(nameof(actionName));

			if (string.IsNullOrEmpty(controllerName))
				throw new ArgumentNullException(nameof(controllerName));

			values.Add("action", actionName);
			values.Add("controller", controllerName);
			return new RedirectToRouteResult(values);
		}

		private ActionResult RedirectToAction(string actionName, string controllerName, Dictionary<string, string> values)
		{
			var routeValues = new RouteValueDictionary();
			foreach (var kpv in values)
				routeValues.Add(kpv.Key, kpv.Value);
			return RedirectToAction(actionName, controllerName, routeValues);
		}

		private ActionResult RedirectToAction(string actionName, string controllerName, object values)
		{
			return RedirectToAction(actionName, controllerName, HtmlHelper.AnonymousObjectToHtmlAttributes(values));
		}

		private ActionResult RedirectToAction(string actionName, string controllerName)
		{
			return RedirectToAction(actionName, controllerName, new Dictionary<string, string>());
		}
	}
}