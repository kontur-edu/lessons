using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Database;
using Database.Di;
using Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Ulearn.Common.Api;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses;
using Ulearn.Web.Api.Authorization;
using Ulearn.Web.Api.Controllers.Notifications;
using Ulearn.Web.Api.Models;
using Ulearn.Web.Api.Models.Binders;
using Ulearn.Web.Api.Swagger;
using Vostok.Commons.Extensions.UnitConvertions;
using Vostok.Hosting;
using Vostok.Metrics;
using Web.Api.Configuration;
using Enum = System.Enum;
using ILogger = Serilog.ILogger;

namespace Ulearn.Web.Api
{
    public class WebApplication : BaseApiWebApplication
    {
		private readonly WebApiConfiguration configuration;

		public WebApplication()
		{
			configuration = ApplicationConfiguration.Read<WebApiConfiguration>();
		}
		
		protected override void OnStarted(IVostokHostingEnvironment hostingEnvironment)
        {
            hostingEnvironment.MetricScope.SystemMetrics(1.Minutes());
			
#if DEBUG
			/* Initialize EntityFramework Profiler. See https://www.hibernatingrhinos.com/products/efprof/learn for details */
			HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();
#endif
        }

		protected override IApplicationBuilder ConfigureCors(IApplicationBuilder app)
		{
			return app.UseCors(builder =>
			{
				builder
					.WithOrigins(configuration.Web.Cors.AllowOrigins)
					.AllowAnyMethod()
					.WithHeaders("Authorization", "Content-Type", "Json-Naming-Strategy")
					.WithExposedHeaders("Location")
					.AllowCredentials();
			});
		}

		protected override IApplicationBuilder ConfigureWebApplication(IApplicationBuilder app)
		{
			var database = app.ApplicationServices.GetService<UlearnDb>();
			database.MigrateToLatestVersion();
			var initialDataCreator = app.ApplicationServices.GetService<InitialDataCreator>();
			database.CreateInitialDataAsync(initialDataCreator);
			
			return app;
		}

		protected override void ConfigureServices(IServiceCollection services, IVostokHostingEnvironment hostingEnvironment, ILogger logger)
		{
			base.ConfigureServices(services, hostingEnvironment, logger);
			
			/* TODO (andgein): use UlearnDbFactory here */
			services.AddDbContextPool<UlearnDb>(
				options => options
					.UseLazyLoadingProxies()
					.UseSqlServer(hostingEnvironment.Configuration["database"])
			);
			
			services.Configure<WebApiConfiguration>(options => hostingEnvironment.Configuration.Bind(options));
			
			/* Add CORS */
			services.AddCors();

			ConfigureAuthServices(services, configuration);

			services.AddSwaggerExamplesFromAssemblyOf<WebApplication>();
		}

		public override void ConfigureMvc(IServiceCollection services)
		{
			/* Asp.NET Core MVC */
			services.AddMvc(options =>
				{
					/* Add binder for passing Course object to actions */
					options.ModelBinderProviders.Insert(0, new CourseBinderProvider());

					/* Disable model checking because in other case stack overflow raised on course model binding.
					   See https://github.com/aspnet/Mvc/issues/7357 for details */
					options.ModelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(Course)));
					options.ModelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(ICourse)));
				}
			).AddApplicationPart(GetType().Assembly)
				.AddControllersAsServices()
				.AddXmlSerializerFormatters()
				.AddJsonOptions(opt => 
					opt.SerializerSettings.ContractResolver = new ApiHeaderJsonContractResolver(new ApiHeaderJsonNamingStrategyOptions
					{
						DefaultStrategy = new CamelCaseNamingStrategy(),
						HeaderName = "Json-Naming-Strategy",
						HttpContextAccessorProvider = services.BuildServiceProvider().GetService<IHttpContextAccessor>,
						NamingStrategies = new Dictionary<string, NamingStrategy>
						{
							{"camelcase", new CamelCaseNamingStrategy() },
							{"snakecase", new SnakeCaseNamingStrategy() }
						}
					}, services.BuildServiceProvider().GetService<IMemoryCache>)
				);
		}

		protected override void ConfigureSwaggerDocumentationGeneration(SwaggerGenOptions c)
		{
			c.OperationFilter<RemoveCourseParameterOperationFilter>();
		}

		public override void ConfigureDi(IServiceCollection services, ILogger logger)
		{
			base.ConfigureDi(services, logger);
			
			services.AddScoped<IAuthorizationHandler, CourseRoleAuthorizationHandler>();
			services.AddScoped<IAuthorizationHandler, CourseAccessAuthorizationHandler>();
			services.AddScoped<INotificationDataPreloader, NotificationDataPreloader>();

			services.AddDatabaseServices(logger);
		}

		public void ConfigureAuthServices(IServiceCollection services, WebApiConfiguration configuration)
		{
			/* Configure sharing cookies between application.
			   See https://docs.microsoft.com/en-us/aspnet/core/security/cookie-sharing?tabs=aspnetcore2x for details */
			services.AddDataProtection()
				.PersistKeysToFileSystem(new DirectoryInfo(configuration.Web.CookieKeyRingDirectory))
				.SetApplicationName("ulearn");

			services.ConfigureApplicationCookie(options =>
			{
				options.Cookie.Name = configuration.Web.CookieName;
				options.Cookie.Expiration = TimeSpan.FromDays(14);
				options.Cookie.Domain = configuration.Web.CookieDomain;
				options.LoginPath = "/users/login";
				options.LogoutPath = "/users/logout";
				options.Events.OnRedirectToLogin = context =>
				{
					/* Replace standard redirecting to LoginPath */
					context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
					return Task.CompletedTask;
				};
				options.Events.OnRedirectToAccessDenied = context =>
				{
					/* Replace standard redirecting to AccessDenied */
					context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
					return Task.CompletedTask;
				};
			});

			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					
					ValidIssuer = configuration.Web.Authentication.Jwt.Issuer,
					ValidAudience = configuration.Web.Authentication.Jwt.Audience,
					IssuerSigningKey = JwtBearerHelpers.CreateSymmetricSecurityKey(configuration.Web.Authentication.Jwt.IssuerSigningKey)
				};
			});

			services.AddAuthorization(options =>
			{
				options.AddPolicy("Instructors", policy => policy.Requirements.Add(new CourseRoleRequirement(CourseRoleType.Instructor)));
				options.AddPolicy("CourseAdmins", policy => policy.Requirements.Add(new CourseRoleRequirement(CourseRoleType.CourseAdmin)));
				options.AddPolicy("SysAdmins", policy => policy.RequireRole(new List<string> { LmsRoleType.SysAdmin.GetDisplayName() }));

				foreach (var courseAccessType in Enum.GetValues(typeof(CourseAccessType)).Cast<CourseAccessType>())
				{
					var policyName = courseAccessType.GetAuthorizationPolicyName();
					options.AddPolicy(policyName, policy => policy.Requirements.Add(new CourseAccessRequirement(courseAccessType)));
				}
			});
		}
	}
}