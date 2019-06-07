using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Filters;
using Ulearn.Common.Api;
using Ulearn.Core.Configuration;
using Ulearn.VideoAnnotations.Web.Annotations;
using Ulearn.VideoAnnotations.Web.Configuration;
using Vostok.Hosting;
using ILogger = Serilog.ILogger;

namespace Ulearn.VideoAnnotations.Web
{
    public class WebApplication : BaseApiWebApplication
    {
		private readonly VideoAnnotationsConfiguration configuration;

		public WebApplication()
		{
			configuration = ApplicationConfiguration.Read<VideoAnnotationsConfiguration>();
		}

		protected override void ConfigureServices(IServiceCollection services, IVostokHostingEnvironment hostingEnvironment, ILogger logger)
		{
			base.ConfigureServices(services, hostingEnvironment, logger);
			
			services.Configure<VideoAnnotationsConfiguration>(options => hostingEnvironment.Configuration.Bind(options));
			
			services.AddSwaggerExamplesFromAssemblyOf<WebApplication>();
		}

		public override void ConfigureDi(IServiceCollection services, ILogger logger)
		{
			base.ConfigureDi(services, logger);

			services.AddSingleton<IAnnotationsParser, AnnotationsParser>();
			services.AddSingleton<IGoogleDocApiClient, GoogleDocApiClient>();
			services.AddSingleton<IAnnotationsCache, AnnotationsCache>();
		}
	}
}