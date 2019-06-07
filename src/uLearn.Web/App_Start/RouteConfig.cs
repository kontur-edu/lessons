using System.Web.Mvc;
using System.Web.Routing;

namespace uLearn.Web
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				name: "Course.Slide",
				url: "Course/{courseId}/{slideIndex}",
				defaults: new { controller = "Course", action = "Slide", slideIndex = -1 },
				constraints: new { slideIndex = @"-?\d+|" }
			);
			routes.MapRoute(
				name: "Course.SlideById",
				url: "Course/{courseId}/{slideId}",
				defaults: new { controller = "Course", action = "SlideById", slideId = "" },
				constraints: new { slideId = @"(.*_)?[{|\(]?[0-9A-F]{8}[-]?([0-9A-F]{4}[-]?){3}[0-9A-F]{12}[\)|}]?" }
			);
			routes.MapRoute(
				name: "Slide",
				url: "Slide/{slideId}",
				defaults: new { controller = "Course", action = "SlideById" }
			);
			routes.MapRoute(
				name: "Course",
				url: "Course/{courseId}/{action}/{slideIndex}",
				defaults: new { controller = "Course", action = "Slide", slideIndex = -1 }
			);
			routes.MapRoute(
				name: "Certificates",
				url: "CertificatesList",
				defaults: new { controller = "Certificates", action = "Index" }
			);
			routes.MapRoute(
				name: "Certificate",
				url: "Certificate/{certificateId}",
				defaults: new { controller = "Certificates", action = "CertificateById" }
			);
			routes.MapRoute(
				name: "CertificateFile",
				url: "Certificate/{certificateId}/{*path}",
				defaults: new { controller = "Certificates", action = "CertificateFile" }
			);
			
			/* For react application which is not able to proxy root url (/) in webpack devserver */
			routes.MapRoute(
				name: "CourseList",
				url: "CourseList",
				defaults: new { controller = "Home", action = "Index" }
			);
			
			/* We should enumerate all controllers here.
			   Otherwise all new, react-based routes (i.e. /basicprogramming/groups) will be routed to unknown controller ("basicprogrammingcontroller"),
			   not to SpaController (next route below). */
			routes.MapRoute(
				name: "Default",
				url: "{controller}/{action}",
				defaults: new { controller = "Home", action = "Index" },
				constraints: new { controller = @"^(Account|Admin|Analytics|AntiPlagiarism|Certificates|Comments|Course|Errors|Exercise|Feed|Grader|Hint|Home|Login|Notifications|Questions|Quiz|RestorePassword|Runner|Sandbox|SlideNavigation|Stepik|Telegram|Visits)$" }
			);
			
			/* After all your routes */
			routes.MapRoute(
				name: "NotFound",
				url: "{*pathInfo}",
				defaults: new { controller = "Spa", action = "IndexHtml" }
			);
		}
	}
}