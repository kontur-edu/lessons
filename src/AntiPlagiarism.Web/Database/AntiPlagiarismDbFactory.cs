using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Ulearn.Core.Configuration;

namespace AntiPlagiarism.Web.Database
{
	public class AntiPlagiarismDbFactory : IDesignTimeDbContextFactory<AntiPlagiarismDb>
	{
		public AntiPlagiarismDb CreateDbContext(string[] args)
		{
			var configuration = ApplicationConfiguration.GetConfiguration();
			var optionsBuilder = new DbContextOptionsBuilder<AntiPlagiarismDb>();
			optionsBuilder.UseNpgsql(configuration["database"]);

			return new AntiPlagiarismDb(optionsBuilder.Options);
		}
	}
}