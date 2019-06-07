using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.CourseRoles;
using Database.Repos.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;
using Ulearn.Common;
using Ulearn.Web.Api;
using Ulearn.Web.Api.Controllers;
using Web.Api.Configuration;
using Z.EntityFramework.Plus;

namespace Web.Api.Tests.Controllers
{
	public class BaseControllerTests
	{
		private WebApplication application;		
		
		protected Logger logger;
		protected UlearnDb db;
		protected IServiceProvider serviceProvider;

		private readonly WebApiConfiguration FakeWebApiConfiguration = new WebApiConfiguration
		{
			Web = new UlearnWebConfiguration()
			{
				Authentication = new AuthenticationConfiguration
				{
					Jwt = new JwtConfiguration
					{
						Audience = "ulearn.me",
						Issuer = "ulearn.me",
						IssuerSigningKey = "issuer-signing-key",
						LifeTimeHours = 24
					}
				},
				CookieKeyRingDirectory = "."
			}
		};

		public BaseControllerTests()
		{
			application = new WebApplication();
		}

		public async Task SetupTestInfrastructureAsync(Action<IServiceCollection> addServices=null)
		{
			logger = new LoggerConfiguration()
				.MinimumLevel.Information()
				.WriteTo.NUnitOutput()
				.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.ffff } {Level}] {Message:lj}{NewLine}{Exception}")
				.CreateLogger();

			var loggerFactory = new LoggerFactory(new List<ILoggerProvider> { new SerilogLoggerProvider(logger) });
			db = CreateDbContext(loggerFactory);

			serviceProvider = ConfigureServices(addServices);
			
			await CreateInitialDataInDatabaseAsync().ConfigureAwait(false);
			await CreateTestUsersAsync().ConfigureAwait(false);
			
			/* Configuring Z.EntityFramework.Plus for working with In-Memory database
			   See https://entityframework-plus.net/batch-delete for details */
			BatchDeleteManager.InMemoryDbContextFactory = () => CreateDbContext(loggerFactory);
			
			/* Cache Manager is not working with In-Memory database.
			   See https://github.com/zzzprojects/EntityFramework-Plus/issues/391 for details. */
			QueryCacheManager.IsEnabled = false;
		}

		private async Task CreateInitialDataInDatabaseAsync()
		{
			var initialDataCreator = serviceProvider.GetService<InitialDataCreator>();
			await initialDataCreator.CreateRolesAsync().ConfigureAwait(false);
			await initialDataCreator.CreateUlearnBotUserAsync().ConfigureAwait(false);
		}

		private static UlearnDb CreateDbContext(ILoggerFactory loggerFactory)
		{
			var optionsBuilder = new DbContextOptionsBuilder<UlearnDb>();
			optionsBuilder.UseLazyLoadingProxies();
			optionsBuilder.UseInMemoryDatabase("ulearn_test_database");
			if (loggerFactory != null)
				optionsBuilder.UseLoggerFactory(loggerFactory);
			
			return new UlearnDb(optionsBuilder.Options);
		}

		private IServiceProvider ConfigureServices(Action<IServiceCollection> addServices=null)
		{
			var services = new ServiceCollection();
			
			services.AddSingleton(db);
			services.AddLogging(builder => builder.AddSerilog(logger));
			application.ConfigureDi(services, logger);
			application.ConfigureAuthServices(services, FakeWebApiConfiguration);
			application.ConfigureMvc(services);

			addServices?.Invoke(services);

			return services.BuildServiceProvider();
		}

		private async Task CreateTestUsersAsync(UlearnUserManager userManager)
		{
			var result = await userManager.CreateAsync(TestUsers.Admin, TestUsers.AdminPassword).ConfigureAwait(false);
			if (! result.Succeeded)
				throw new InvalidOperationException($"Can't create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
			
			TestUsers.Admin = await userManager.FindByNameAsync(TestUsers.Admin.UserName).ConfigureAwait(false);
			logger.Information($"Created user {TestUsers.Admin.UserName} with password {TestUsers.AdminPassword}, id = {TestUsers.Admin.Id}");
			await userManager.AddToRoleAsync(TestUsers.Admin, LmsRoleType.SysAdmin.ToString()).ConfigureAwait(false);
		}

		private Task CreateTestUsersAsync()
		{
			var userManager = serviceProvider.GetService<UlearnUserManager>();
			return CreateTestUsersAsync(userManager);
		}

		protected async Task AuthenticateUserInControllerAsync(BaseController controller, ApplicationUser user)
		{
			var factory = serviceProvider.GetService<IUserClaimsPrincipalFactory<ApplicationUser>>();
			var admin = await factory.CreateAsync(user).ConfigureAwait(false);
			controller.ControllerContext.HttpContext.User = admin;
		}

		protected void ConfigureControllerForTests(BaseController controller)
		{
			var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
			var urlHelper = new Mock<IUrlHelper>();
			controller.Url = urlHelper.Object;
			controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
		}

		protected T GetController<T>() where T : BaseController
		{
			var controller = serviceProvider.GetService<T>();
			ConfigureControllerForTests(controller);
			return controller;
		}
	}

	public static class TestUsers
	{
		public static ApplicationUser Admin = new ApplicationUser
		{
			UserName = "admin",
			FirstName = "Super",
			LastName = "Administrator",
			Email = "admin@ulearn.me",
			Gender = Gender.Male,
			Registered = DateTime.Now,
		};

		public const string AdminPassword = "AdminPassword123!";
	}
}