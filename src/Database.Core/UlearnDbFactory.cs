using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Database
{
	public class UlearnDbFactory : IDesignTimeDbContextFactory<UlearnDb>
	{
		public UlearnDb CreateDbContext(string[] args)
		{
			var configuration = GetConfiguration();

			var optionsBuilder = new DbContextOptionsBuilder<UlearnDb>();
			optionsBuilder.UseLazyLoadingProxies();
			optionsBuilder.UseSqlServer(configuration.GetValue<string>("database"));
			
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