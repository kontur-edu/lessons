using System;
using Ulearn.Core.Configuration;

namespace Ulearn.VideoAnnotations.Web.Configuration
{
	public class VideoAnnotationsConfiguration : UlearnConfiguration
	{
		public VideoAnnotationsServiceConfiguration VideoAnnotations { get; set; }
	}

	public class VideoAnnotationsServiceConfiguration
	{
		public string GoogleDocsApiKey { get; set; }
		
		public TimeSpan Timeout { get; set; }
		
		public CacheConfiguration Cache { get; set; } = new CacheConfiguration();
	}

	public class CacheConfiguration
	{
		public int Capacity { get; set; } = 50;

		public TimeSpan MaxLifeTime { get; set; } = TimeSpan.FromHours(1);
	}
}