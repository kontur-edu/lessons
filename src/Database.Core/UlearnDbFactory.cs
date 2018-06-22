using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Database
{
	public class UlearnDbFactory : IDesignTimeDbContextFactory<UlearnDb>
	{
		public UlearnDb CreateDbContext(string[] args)
		{
			return CreateDbContext(args, null);
		}
		
		public UlearnDb CreateDbContext(string[] args, ILoggerFactory loggerFactory)
		{
			var configuration = GetConfiguration();

			var optionsBuilder = new DbContextOptionsBuilder<UlearnDb>();
			optionsBuilder.UseLazyLoadingProxies();
			optionsBuilder.UseSqlServer(configuration.GetValue<string>("database"));
			if (loggerFactory != null)
				optionsBuilder.UseLoggerFactory(loggerFactory);
			
			return new UlearnDb(optionsBuilder.Options);
		}

		private static IConfigurationRoot GetConfiguration()
		{
			return new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: false)
				.Build();
		}
	}
}