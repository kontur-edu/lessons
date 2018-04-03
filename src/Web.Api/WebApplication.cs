using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using uLearn;
using uLearn.Configuration;
using Ulearn.Common.Extensions;
using Ulearn.Web.Api.Authorization;
using Ulearn.Web.Api.Models.Binders;
using Vostok.Commons.Extensions.UnitConvertions;
using Vostok.Hosting;
using Vostok.Instrumentation.AspNetCore;
using Vostok.Logging.Serilog;
using Vostok.Logging.Serilog.Enrichers;
using Vostok.Metrics;
using Web.Api.Configuration;

namespace Ulearn.Web.Api
{
    public class WebApplication : AspNetCoreVostokApplication
    {
        protected override void OnStarted(IVostokHostingEnvironment hostingEnvironment)
        {
            hostingEnvironment.MetricScope.SystemMetrics(1.Minutes());
        }

        protected override IWebHost BuildWebHost(IVostokHostingEnvironment hostingEnvironment)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.With<ThreadEnricher>()
                .Enrich.With<FlowContextEnricher>()
                .MinimumLevel.Debug()
                .WriteTo.Airlock(LogEventLevel.Information);
			
            if (hostingEnvironment.Log != null)
                loggerConfiguration = loggerConfiguration.WriteTo.VostokLog(hostingEnvironment.Log);
            var logger = loggerConfiguration.CreateLogger();
			
            return new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://*:{hostingEnvironment.Configuration["port"]}/")
                .AddVostokServices()
				.ConfigureServices(s => ConfigureServices(s, hostingEnvironment, logger))
                .UseSerilog(logger)
                .Configure(app =>
                {
                    var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
                    app.UseVostok();
                    if (env.IsDevelopment())
                        app.UseDeveloperExceptionPage();
					
					app.UseAuthentication();
					app.UseMvc();
					
					var database = app.ApplicationServices.GetService<UlearnDb>();
					database.MigrateToLatestVersion();
					var initialDataCreator = app.ApplicationServices.GetService<InitialDataCreator>();
					database.CreateInitialDataAsync(initialDataCreator);
                })
                .Build();
        }

		private void ConfigureServices(IServiceCollection services, IVostokHostingEnvironment hostingEnvironment, Logger logger)
		{
			services.AddDbContextPool<UlearnDb>(
				options => options.UseSqlServer(hostingEnvironment.Configuration["database"])
			);
			
			var configuration = ApplicationConfiguration.Read<WebApiConfiguration>();
			
			/* DI */
			services.AddSingleton<ILogger>(logger);
			services.AddSingleton<ULearnUserManager>();
			services.AddSingleton<InitialDataCreator>();
			services.AddSingleton<WebCourseManager>();
			services.AddSingleton(configuration);
			services.AddScoped<IAuthorizationHandler, CourseRoleAuthorizationHandler>();
			services.AddScoped<IAuthorizationHandler, CourseAccessAuthorizationHandler>();
			
			/* DI for database repos */
			services.AddScoped<UsersRepo>();
			services.AddScoped<CommentsRepo>();
			services.AddScoped<UserRolesRepo>();
			services.AddScoped<CoursesRepo>();
			services.AddScoped<SlideCheckingsRepo>();
			services.AddScoped<GroupsRepo>();
			services.AddScoped<UserSolutionsRepo>();
			services.AddScoped<UserQuizzesRepo>();
			services.AddScoped<VisitsRepo>();
			services.AddScoped<TextsRepo>();
			
			/* Asp.NET Core MVC */
			services.AddMvc(options =>
				{
					/* Add binder for passing Course object to actions */
					options.ModelBinderProviders.Insert(0, new CourseBinderProvider());
						
					/* Disable model checking because in other case stack overflow raised on course model binding.
					   See https://github.com/aspnet/Mvc/issues/7357 for details
					*/
					options.ModelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(Course)));
				}
			);

			ConfigureAuthServices(services, configuration, logger);
		}

		private void ConfigureAuthServices(IServiceCollection services, WebApiConfiguration configuration, Logger logger)
		{
			services.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<UlearnDb>()
				.AddDefaultTokenProviders();

			/* Configure sharing cookies between application.
			   See https://docs.microsoft.com/en-us/aspnet/core/security/cookie-sharing?tabs=aspnetcore2x for details */
			services.AddDataProtection()
				.PersistKeysToFileSystem(new DirectoryInfo(configuration.Web.CookieKeyRingDirectory))
				.SetApplicationName("ulearn");

			services.ConfigureApplicationCookie(options =>
			{
				options.Cookie.Name = "ulearn.auth";
				options.Cookie.Expiration = TimeSpan.FromDays(14);
				options.LoginPath = "/users/login";
				options.LogoutPath = "/users/logout";
				options.Events.OnRedirectToLogin = context =>
				{
					/* Replace standart redirecting to LoginPath */
					context.Response.StatusCode = (int)HttpStatusCode.NotFound;
					return Task.CompletedTask;
				};
				options.Events.OnRedirectToAccessDenied = context =>
				{
					/* Replace standart redirecting to AccessDenied */
					context.Response.StatusCode = (int)HttpStatusCode.NotFound;
					return Task.CompletedTask;
				};
			});

			services.AddAuthorization(options =>
			{
				options.AddPolicy("Instructors", policy => policy.Requirements.Add(new CourseRoleRequirement(CourseRole.Instructor)));
				options.AddPolicy("CourseAdmins", policy => policy.Requirements.Add(new CourseRoleRequirement(CourseRole.CourseAdmin)));
				options.AddPolicy("SysAdmins", policy => policy.RequireRole(new List<string> { LmsRoles.SysAdmin.GetDisplayName() }));

				foreach (var courseAccessType in Enum.GetValues(typeof(CourseAccessType)).Cast<CourseAccessType>())
				{
					var policyName = courseAccessType.GetAuthorizationPolicyName();
					options.AddPolicy(policyName, policy => policy.Requirements.Add(new CourseAccessRequirement(courseAccessType)));
				}
			});
		}
	}
}