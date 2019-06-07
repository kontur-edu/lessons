using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using Community.AspNetCore.ExceptionHandling;
using Community.AspNetCore.ExceptionHandling.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Common.Api.Swagger;
using Vostok.Commons.Extensions.UnitConvertions;
using Vostok.Hosting;
using Vostok.Instrumentation.AspNetCore;
using Vostok.Logging.Serilog;
using Vostok.Logging.Serilog.Enrichers;
using Vostok.Metrics;
using ILogger = Serilog.ILogger;

namespace Ulearn.Common.Api
{
	public class BaseApiWebApplication : AspNetCoreVostokApplication
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
				loggerConfiguration = loggerConfiguration.WriteTo.VostokLog(hostingEnvironment.Log, LogEventLevel.Information);
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

					/* Add CORS. Should be before app.UseMvc() */
					ConfigureCors(app);

					/* Add exception handling policy.
					   See https://github.com/IharYakimush/asp-net-core-exception-handling */
					app.UseExceptionHandlingPolicies();

					app.UseAuthentication();
					app.UseMvc();

					/* Configure swagger documentation. Now it's available at /swagger/v1/swagger.json.
					 * See https://github.com/domaindrivendev/Swashbuckle.AspNetCore for details */
					app.UseSwagger(c => { c.RouteTemplate = "documentation/{documentName}/swagger.json"; });
					/* And add swagger UI, available at /documentation */
					app.UseSwaggerUI(c =>
					{
						c.SwaggerEndpoint("/documentation/v1/swagger.json", "Ulearn API");
						c.DocumentTitle = "UlearnApi";
						c.RoutePrefix = "documentation";
					});

					ConfigureWebApplication(app);
				})
				.Build();
		}

		protected virtual IApplicationBuilder ConfigureCors(IApplicationBuilder app)
		{
			return app;
		}

		protected virtual IApplicationBuilder ConfigureWebApplication(IApplicationBuilder app)
		{
			return app;
		}

		protected virtual void ConfigureServices(IServiceCollection services, IVostokHostingEnvironment hostingEnvironment, ILogger logger)
		{
			ConfigureDi(services, logger);
			ConfigureMvc(services);
			ConfigureSwaggerDocumentation(services);
			ConfigureExceptionPolicy(services, logger);
		}

		public virtual void ConfigureMvc(IServiceCollection services)
		{
			/* Asp.NET Core MVC */
			services
				.AddMvc()
				.AddApplicationPart(GetType().Assembly)
				.AddControllersAsServices();
		}

		private void ConfigureSwaggerDocumentation(IServiceCollection services)
		{
			/* Swagger API documentation generator. See https://github.com/domaindrivendev/Swashbuckle.AspNetCore for details */
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Info
				{
					Title = "Ulearn API",
					Version = "v1",
					Description = "An API for ulearn.me",
					Contact = new Contact
					{
						Name = "Ulearn support",
						Email = "support@ulearn.me"
					}
				});

				/* See https://github.com/mattfrear/Swashbuckle.AspNetCore.Filters#installation for manual about swagger request and response examples */
				c.ExampleFilters();

				c.OperationFilter<BadRequestResponseOperationFilter>();
				c.OperationFilter<AuthResponsesOperationFilter>();

				c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

				c.AddSecurityDefinition("Bearer", new ApiKeyScheme
				{
					In = "header",
					Description = "Please insert JWT with Bearer into field. Example: \"Bearer {token}\"",
					Name = "Authorization",
					Type = "apiKey"
				});
				c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
				{
					{ "Bearer", new string[] { } }
				});

				/* See https://docs.microsoft.com/ru-ru/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-2.1&tabs=visual-studio%2Cvisual-studio-xml for details */
				var xmlFile = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				if (new FileInfo(xmlPath).Exists)
					c.IncludeXmlComments(xmlPath);
				
				ConfigureSwaggerDocumentationGeneration(c);
			});
		}

		protected virtual void ConfigureSwaggerDocumentationGeneration(SwaggerGenOptions c)
		{
		}

		private void ConfigureExceptionPolicy(IServiceCollection services, ILogger logger)
		{
			/* See https://github.com/IharYakimush/asp-net-core-exception-handling for details */
			services.AddExceptionHandlingPolicies(options =>
			{
				options.For<StatusCodeException>()
					.Response(exception => exception.Code)
					.WithObjectResult((r, exception) => new ErrorResponse(exception.Message))
					.Handled();

				/* Ensure that all exception types are handled by adding handler for generic exception at the end. */
				options.For<Exception>()
					.Log(lo => { lo.Level = (context, exception) => LogLevel.Error; })
					.Response(exception => (int)HttpStatusCode.InternalServerError, ResponseAlreadyStartedBehaviour.GoToNextHandler)
					.ClearCacheHeaders()
					.WithObjectResult((r, exception) => new ErrorResponse("Internal error occured"
#if DEBUG
																		+ $". {exception.GetType().FullName}: {exception.Message}\n{exception.StackTrace}"
#endif
					))
					.Handled();
			});
		}

		public virtual void ConfigureDi(IServiceCollection services, ILogger logger)
		{
			services.AddSingleton(logger);
		}
	}
}