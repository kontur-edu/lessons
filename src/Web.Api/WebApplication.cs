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
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Ulearn.Common.Api;
using Ulearn.Common.Api.Swagger;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Courses;
using Ulearn.Core.Metrics;
using Ulearn.Core.RunCheckerJobApi;
using Ulearn.Core.Telegram;
using Ulearn.Web.Api.Authorization;
using Ulearn.Web.Api.Clients;
using Ulearn.Web.Api.Controllers.Notifications;
using Ulearn.Web.Api.Controllers.Runner;
using Ulearn.Web.Api.Controllers.Slides;
using Ulearn.Web.Api.Controllers.Websockets;
using Ulearn.Web.Api.Models;
using Ulearn.Web.Api.Models.Binders;
using Ulearn.Web.Api.Models.Responses.SlideBlocks;
using Ulearn.Web.Api.Swagger;
using Vostok.Hosting.Abstractions;
using Web.Api.Configuration;
using Enum = System.Enum;

namespace Ulearn.Web.Api
{
	public class WebApplication : BaseApiWebApplication
	{
		private WebApiConfiguration configuration;
		private Type[] polymorphismBaseTypes = { typeof(IApiSlideBlock), typeof(RunnerSubmission) };

		public override Task WarmupAsync(IVostokHostingEnvironment environment, IServiceProvider provider)
		{
#if DEBUG
			/* Initialize EntityFramework Profiler. See https://www.hibernatingrhinos.com/products/efprof/learn for details */
			HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();
#endif
			provider.GetService<WebsocketsEventSender>();
			return Task.CompletedTask;
		}

		protected override IApplicationBuilder ConfigureCors(IApplicationBuilder app)
		{
			return app.UseCors(builder =>
			{
				builder
					.WithOrigins(configuration.Web.Cors.AllowOrigins)
					.AllowAnyMethod()
					.WithHeaders("Authorization", "Content-Type", "Json-Naming-Strategy",
						"X-Requested-With", "x-signalr-user-agent" // signalR
					)
					.WithExposedHeaders("Location")
					.AllowCredentials();
			});
		}

		protected override IApplicationBuilder ConfigureWebApplication(IApplicationBuilder app)
		{
			// MigrateAndCreateInitialData(app); // NOTE: Миграции в Api отключены пока выполняются в Web
			ConfigureWebsockets(app);
			return app;
		}

		private static void MigrateAndCreateInitialData(IApplicationBuilder app)
		{
			var database = app.ApplicationServices.GetService<UlearnDb>();
			database.MigrateToLatestVersion();
			var initialDataCreator = app.ApplicationServices.GetService<InitialDataCreator>();
			database.CreateInitialDataAsync(initialDataCreator);
		}

		private const string websocketsPath = "/ws";
		private static void ConfigureWebsockets(IApplicationBuilder app)
		{
			
			app.Map(
				new PathString(websocketsPath), // Map применяет middleware только при обработке запросов по указанному префиксу пути
				a =>
				{
					app.UseWebSockets();
					app.UseRouting(); // Включает обработку UseEndpoints
					app.UseEndpoints(endpoints =>
					{
						endpoints.MapHub<WebsocketsHub>(websocketsPath, configureOptions =>
						{
							configureOptions.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
						});
					});
				});
		}

		protected override void ConfigureServices(IServiceCollection services, IVostokHostingEnvironment hostingEnvironment)
		{
			configuration = hostingEnvironment.SecretConfigurationProvider.Get<WebApiConfiguration>(hostingEnvironment.SecretConfigurationSource);

			base.ConfigureServices(services, hostingEnvironment);

			/* TODO (andgein): use UlearnDbFactory here */
			services.AddDbContext<UlearnDb>( // AddDbContextPool: DbContext Pooling does not dispose LazyLoader https://github.com/dotnet/efcore/issues/11308
				options => options
					.UseLazyLoadingProxies()
					.UseNpgsql(configuration.Database, o => o.SetPostgresVersion(13, 2))
			);

			services.Configure<WebApiConfiguration>(options =>
				options.SetFrom(hostingEnvironment.SecretConfigurationProvider.Get<WebApiConfiguration>(hostingEnvironment.SecretConfigurationSource)));

			/* Add CORS */
			services.AddCors();

			ConfigureAuthServices(services, configuration);

			services.AddSignalR();

			services.AddSwaggerExamplesFromAssemblyOf<WebApplication>();
		}

		public override void ConfigureMvc(IServiceCollection services)
		{
			var jsonSerializerSettings = JsonConfig.GetSettings(polymorphismBaseTypes);

			/* Asp.NET Core MVC https://www.strathweb.com/2020/02/asp-net-core-mvc-3-x-addmvc-addmvccore-addcontrollers-and-other-bootstrapping-approaches/ */
			services.AddMvc(options =>
					{
						options.EnableEndpointRouting = false;

						/* Add binder for passing Course object to actions */
						options.ModelBinderProviders.Insert(0, new CourseBinderProvider());

						/* Disable model checking because in other case stack overflow raised on course model binding.
							See https://github.com/aspnet/Mvc/issues/7357 for details */
						options.ModelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(Course)));
						options.ModelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(ICourse)));
					}
				).AddApplicationPart(GetType().Assembly)
				.AddControllersAsServices()
				.AddNewtonsoftJson(opt =>
					{
						opt.SerializerSettings.ContractResolver = new ApiHeaderJsonContractResolver(new ApiHeaderJsonNamingStrategyOptions
						{
							DefaultStrategy = new CamelCaseNamingStrategy(),
							HeaderName = "Json-Naming-Strategy",
							HttpContextAccessorProvider = services.BuildServiceProvider().GetService<IHttpContextAccessor>,
							NamingStrategies = new Dictionary<string, NamingStrategy>
							{
								{ "camelcase", new CamelCaseNamingStrategy() },
								{ "snakecase", new SnakeCaseNamingStrategy() }
							}
						}, services.BuildServiceProvider().GetService<IMemoryCache>);
						opt.SerializerSettings.TypeNameHandling = jsonSerializerSettings.TypeNameHandling;
						opt.SerializerSettings.SerializationBinder = jsonSerializerSettings.SerializationBinder;
						opt.SerializerSettings.Converters = opt.SerializerSettings.Converters.Concat(jsonSerializerSettings.Converters).ToList();
					}
				);
		}

		protected override void ConfigureSwaggerDocumentationGeneration(SwaggerGenOptions c)
		{
			c.OperationFilter<RemoveCourseParameterOperationFilter>();
			foreach (var polymorphismBaseType in polymorphismBaseTypes)
			{
				c.DocumentFilterDescriptors.Add(new FilterDescriptor { Type = typeof(PolymorphismDocumentFilter<>).MakeGenericType(polymorphismBaseType), Arguments = new object[0] });
				c.SchemaFilterDescriptors.Add(new FilterDescriptor { Type = typeof(PolymorphismSchemaFilter<>).MakeGenericType(polymorphismBaseType), Arguments = new object[0] });
			}
		}

		public override void ConfigureDi(IServiceCollection services)
		{
			base.ConfigureDi(services);

			services.AddScoped<IAuthorizationHandler, CourseRoleAuthorizationHandler>();
			services.AddScoped<IAuthorizationHandler, CourseAccessAuthorizationHandler>();
			services.AddScoped<INotificationDataPreloader, NotificationDataPreloader>();
			services.AddSingleton<IUlearnVideoAnnotationsClient, UlearnVideoAnnotationsClient>();
			services.AddScoped<SlideRenderer, SlideRenderer>();
			services.AddSingleton<WebsocketsEventSender, WebsocketsEventSender>();
			services.AddScoped(sp => new MetricSender(
				((IOptions<WebApiConfiguration>)sp.GetService(typeof(IOptions<WebApiConfiguration>))).Value.GraphiteServiceName));
			services.AddScoped(sp => new ErrorsBot(
				((IOptions<WebApiConfiguration>)sp.GetService(typeof(IOptions<WebApiConfiguration>))).Value,
				(MetricSender)sp.GetService(typeof(MetricSender))));
			services.AddScoped<XQueueResultObserver>();
			services.AddScoped<SandboxErrorsResultObserver>();
			services.AddScoped<AntiPlagiarismResultObserver>();
			services.AddScoped<StyleErrorsResultObserver>();
			services.AddScoped<LtiResultObserver>();

			services.AddDatabaseServices();
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
				options.ExpireTimeSpan = TimeSpan.FromDays(14);
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
				options.Events = new JwtBearerEvents // Jwt-токен в websocket передается через query string
				{
					OnMessageReceived = context =>
					{
						var path = context.HttpContext.Request.Path;
						if (path.StartsWithSegments(websocketsPath))
						{
							var accessToken = context.Request.Query["access_token"];
							if(!string.IsNullOrEmpty(accessToken))
								context.Token = accessToken;
						}
						return Task.CompletedTask;
					}
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

			services.Configure<PasswordHasherOptions>(options =>
			{
				options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2;
			});
		}
	}
}