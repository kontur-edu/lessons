using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Ulearn.Core.Configuration
{
	public static class ApplicationConfiguration
	{
		public static T Read<T>(IDictionary<string, string> initialData, bool isAppsettingsJsonOptional=false) where T : AbstractConfiguration
		{
			var configuration = GetConfiguration(initialData, isAppsettingsJsonOptional);
			return configuration.Get<T>();
		}
		
		public static T Read<T>(bool isAppsettingsJsonOptional=false) where T : AbstractConfiguration
		{
			return Read<T>(new Dictionary<string, string>(), isAppsettingsJsonOptional);
		}

		public static IConfiguration GetConfiguration(IDictionary<string, string> initialData, bool isAppsettingsJsonOptional=false)
		{
			var applicationPath = string.IsNullOrEmpty(Utils.WebApplicationPhysicalPath)
				? Directory.GetCurrentDirectory()
				: Utils.WebApplicationPhysicalPath;
			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(initialData)
				.SetBasePath(applicationPath)
				.AddJsonFile("appsettings.json", optional: isAppsettingsJsonOptional, reloadOnChange: true)
				.Build();

			return configuration;
		}
		
		public static IConfiguration GetConfiguration(bool isAppsettingsJsonOptional=false)
		{
			return GetConfiguration(new Dictionary<string, string>(), isAppsettingsJsonOptional);
		}
		
	}
	
	public abstract class AbstractConfiguration
	{
		 
	}
	
	public class HostLogConfiguration
	{
		public bool Console { get; set; }
		
		public string PathFormat { get; set; }
		
		public string MinimumLevel { get; set; }
		
		public bool EnableEntityFrameworkLogging { get; set; }
	}
	
	public class UlearnConfiguration : AbstractConfiguration
	{
		public TelegramConfiguration Telegram { get; set; }
		
		public string BaseUrl { get; set; }
		
		public string CoursesDirectory { get; set; }
		
		public bool BuildExerciseStudentZips { get; set; }
		
		public string ExerciseStudentZipsDirectory { get; set; }
		
		public CertificateConfiguration Certificates { get; set; }
		
		public string GraphiteServiceName { get; set; }
		
		public GitConfiguration Git { get; set; }
	}

	public class TelegramConfiguration
	{
		public string BotToken { get; set; }
		
		public ErrorsTelegramConfiguration Errors { get; set; }
	}

	public class ErrorsTelegramConfiguration
	{
		public string Channel { get; set; }
	}
	
	public class CertificateConfiguration
	{
		public string Directory { get; set; }
	}

	public class GitConfiguration
	{
		public GitWebhookConfiguration Webhook { get; set; }
	}

	public class GitWebhookConfiguration
	{
		public string Secret { get; set; }
	}
}