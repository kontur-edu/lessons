using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Mvc;
using LtiLibrary.Core.Extensions;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Configuration;
using Web.Api.Configuration;

namespace uLearn.Web.Controllers
{
	public class ErrorsController : Controller
	{
	
		private static string IndexHtmlPath => WebConfigurationManager.AppSettings["ulearn.react.index.html"];
		private static DirectoryInfo AppDirectory => new DirectoryInfo(Utils.GetAppPath());
		
		private readonly List<string> excludedPrefixes;
		private readonly byte[] content;

		
		public ErrorsController()
			:this(excludedPrefixes: new List<string>
			{
				"/elmah/",
				"/Certificate/",
				"/Analytics/ExportCourseStatisticsAs",
				"/Exercise/StudentZip",
				"/Content/"
			})
		{}
		
		public ErrorsController(List<string> excludedPrefixes)
		{
			var file = AppDirectory.GetFile(IndexHtmlPath);
			
			this.excludedPrefixes = excludedPrefixes;
			content = System.IO.File.ReadAllBytes(file.FullName);
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
		
		public ActionResult Error404()
		{
			var httpContext = HttpContext;
			
			foreach (var prefix in excludedPrefixes)
				if (httpContext.Request.Url != null && httpContext.Request.Url.LocalPath.StartsWith(prefix))
					return Real404();
			
			var acceptHeader = httpContext.Request.Headers["Accept"] ?? "";
			var cspHeader = WebConfigurationManager.AppSettings["ulearn.web.cspHeader"] ?? "";
			if (acceptHeader.Contains("text/html") && httpContext.Request.HttpMethod == "GET")
			{
				httpContext.Response.Headers.Add("Content-Security-Policy-Report-Only", cspHeader);
				return new FileContentResult(content, "text/html");
			}

			return Real404();
		}

		private ActionResult Real404()
		{
			return View("404");
		}
		
		public ActionResult Error500()
		{
			return View("Error");
		}
	}
}