using AntiPlagiarism.Web.CodeAnalyzing;
using AntiPlagiarism.Web.Configuration;
using AntiPlagiarism.Web.Database;
using AntiPlagiarism.Web.Database.Repos;
using AntiPlagiarism.Web.Workers;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Filters;
using Ulearn.Common.Api;
using Ulearn.Core.Configuration;
using Vostok.Hosting.Abstractions;

namespace AntiPlagiarism.Web
{
	public class WebApplication : BaseApiWebApplication
	{
		private AddNewSubmissionWorker addNewSubmissionWorker; // держит ссылку на воркеры
		private UpdateOldSubmissionsFromStatisticsWorker updateOldSubmissionsFromStatisticsWorker; // держит ссылку на воркеры
		
		protected override IApplicationBuilder ConfigureWebApplication(IApplicationBuilder app)
		{
			var database = app.ApplicationServices.GetService<AntiPlagiarismDb>();
			database.MigrateToLatestVersion();
			addNewSubmissionWorker = app.ApplicationServices.GetService<AddNewSubmissionWorker>();
			updateOldSubmissionsFromStatisticsWorker = app.ApplicationServices.GetService<UpdateOldSubmissionsFromStatisticsWorker>();
			return app;
		}

		protected override void ConfigureServices(IServiceCollection services, IVostokHostingEnvironment hostingEnvironment)
		{
			base.ConfigureServices(services, hostingEnvironment);

			var configuration = hostingEnvironment.SecretConfigurationProvider.Get<UlearnConfiguration>(hostingEnvironment.SecretConfigurationSource);

			services.AddDbContext<AntiPlagiarismDb>(
				options => options.UseNpgsql(configuration.Database, o => o.SetPostgresVersion(13, 2))
			);

			services.Configure<AntiPlagiarismConfiguration>(options =>
				options.SetFrom(hostingEnvironment.SecretConfigurationProvider.Get<AntiPlagiarismConfiguration>(hostingEnvironment.SecretConfigurationSource)));

			services.AddSwaggerExamplesFromAssemblyOf<WebApplication>();
		}

		public override void ConfigureDi(IServiceCollection services)
		{
			base.ConfigureDi(services);

			/* Database repositories */
			/* TODO (andgein): make auto-discovering of repositories */
			services.AddScoped<IClientsRepo, ClientsRepo>();
			services.AddScoped<ISubmissionsRepo, SubmissionsRepo>();
			services.AddScoped<ISnippetsRepo, SnippetsRepo>();
			services.AddScoped<ITasksRepo, TasksRepo>();
			services.AddScoped<IWorkQueueRepo, WorkQueueRepo>();
			services.AddScoped<IMostSimilarSubmissionsRepo, MostSimilarSubmissionsRepo>();
			services.AddScoped<IManualSuspicionLevelsRepo, ManualSuspicionLevelsRepo>();

			/* Other services */
			services.AddScoped<PlagiarismDetector>();
			services.AddScoped<StatisticsParametersFinder>();
			services.AddSingleton<CSharpCodeUnitsExtractor>();
			services.AddScoped<SnippetsExtractor>();
			services.AddScoped<SubmissionSnippetsExtractor>();
			services.AddScoped<NewSubmissionHandler>();
			services.AddSingleton<AddNewSubmissionWorker>();
			services.AddSingleton<UpdateOldSubmissionsFromStatisticsWorker>();
			services.AddSingleton<TokensExtractor>();
			services.AddSingleton<CodeUnitsExtractor>();
		}
	}
}