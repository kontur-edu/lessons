using System;
using System.Web.Configuration;
using Serilog;
using Ulearn.VideoAnnotations.Api.Client;

namespace uLearn.Web.Clients
{
	public static class UlearnVideoAnnotationsClient
	{
		public static readonly VideoAnnotationsClient Instance = 
			string.IsNullOrEmpty(WebConfigurationManager.AppSettings["ulearn.videoAnnotations.endpoint"])
				? null
				: new VideoAnnotationsClient(
					new LoggerConfiguration().WriteTo.Log4Net().CreateLogger(),
					new Uri(WebConfigurationManager.AppSettings["ulearn.videoAnnotations.endpoint"])
				);
	}
}