using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace uLearn.Web
{
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			UlearnLogger.ConfigureLogging();
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			/* Disable identity checks for CSRF tokens.
			 * See http://stackoverflow.com/questions/14970102/anti-forgery-token-is-meant-for-user-but-the-current-user-is-username for details
			 */
			AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;

			/* Disable X-AspNetMvc-Version header
			 * See https://stackoverflow.com/questions/3418557/how-to-remove-asp-net-mvc-default-http-headers for details
			 */
			MvcHandler.DisableMvcResponseHeader = true;
		}
	}
}
 