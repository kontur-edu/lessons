using System;
using System.Linq;
using System.Threading.Tasks;
using AntiPlagiarism.UpdateDb.Configuration;
using AntiPlagiarism.Web.CodeAnalyzing;
using AntiPlagiarism.Web.Configuration;
using AntiPlagiarism.Web.Database;
using AntiPlagiarism.Web.Database.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vostok.Logging.Abstractions;
using Ulearn.Core.Configuration;
using Ulearn.Core.Logging;
using Vostok.Logging.Microsoft;

namespace AntiPlagiarism.UpdateDb
{
	public class Program
	{
		public static void Main(string[] args)
		{
			new Program().RunAsync(args).Wait();
		}

		public async Task RunAsync(string[] args)
		{
			Console.WriteLine(
				@"This tool will help you to update antiplagiarism database. " +
				@"I.e. in case when new logic for code units extraction have been added."
			);

			var firstSubmissionId = 0;
			if (args.Contains("--start"))
			{
				var startArgIndex = args.FindIndex("--start");
				if (startArgIndex + 1 >= args.Length || !int.TryParse(args[startArgIndex + 1], out firstSubmissionId))
					firstSubmissionId = 0;
			}

			var updateOnlyTokensCount = args.Contains("--update-tokens-count");

			var provider = GetServiceProvider();
			var updater = provider.GetService<AntiPlagiarismSnippetsUpdater>();
			await updater.UpdateAsync(firstSubmissionId, updateOnlyTokensCount).ConfigureAwait(false);
		}

		private ServiceProvider GetServiceProvider()
		{
			var configuration = ApplicationConfiguration.Read<AntiPlagiarismUpdateDbConfiguration>();

			var services = new ServiceCollection();

			services.AddOptions();

			services.Configure<AntiPlagiarismUpdateDbConfiguration>(ApplicationConfiguration.GetConfiguration());
			services.Configure<AntiPlagiarismConfiguration>(ApplicationConfiguration.GetConfiguration());

			LoggerSetup.Setup(configuration.HostLog, configuration.GraphiteServiceName);
			services.AddScoped(_ => GetDatabase(configuration));
			services.AddScoped<AntiPlagiarismSnippetsUpdater>();
			services.AddScoped<ISnippetsRepo, SnippetsRepo>();
			services.AddScoped<ISubmissionsRepo, SubmissionsRepo>();
			services.AddSingleton<CSharpCodeUnitsExtractor>();
			services.AddSingleton<CodeUnitsExtractor>();
			services.AddSingleton<TokensExtractor>();
			services.AddSingleton<SnippetsExtractor>();
			services.AddSingleton<SubmissionSnippetsExtractor>();

			return services.BuildServiceProvider();
		}

		private AntiPlagiarismDb GetDatabase(AntiPlagiarismUpdateDbConfiguration configuration)
		{
			var optionsBuilder = new DbContextOptionsBuilder<AntiPlagiarismDb>();
			optionsBuilder.UseNpgsql(configuration.Database, o => o.SetPostgresVersion(13, 2));
			if (configuration.HostLog.EnableEntityFrameworkLogging)
				optionsBuilder.UseLoggerFactory(new LoggerFactory().AddVostok(LogProvider.Get()));

			return new AntiPlagiarismDb(optionsBuilder.Options);
		}
	}
}