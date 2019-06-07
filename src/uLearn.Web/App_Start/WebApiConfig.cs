using System.Linq;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Elmah.Contrib.WebApi;
using Microsoft.Owin.Security.OAuth;
using Ulearn.Core;

namespace uLearn.Web
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			config.SuppressDefaultHostAuthentication();
			config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
			config.MapHttpAttributeRoutes();

			config.Formatters.Where(f => f != config.Formatters.JsonFormatter).ToList()
				.ForEach(f => config.Formatters.Remove(f));
			config.Formatters.JsonFormatter.SerializerSettings = JsonConfig.GetSettings();

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);

			/* Log all errors via ELMAH */
			config.Services.Add(typeof(IExceptionLogger), new ElmahExceptionLogger());
		}
	}
}