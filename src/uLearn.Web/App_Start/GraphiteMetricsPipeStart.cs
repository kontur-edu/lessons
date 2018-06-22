using System;
using Graphite.Web;
using System.Web.Configuration;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using uLearn.Web;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(GraphiteMetricsPipeStart), "PreStart")]

namespace uLearn.Web
{
	public class GraphiteMetricsPipeStart
	{
		private static bool IsGraphiteSendingEnabled => !string.IsNullOrEmpty(WebConfigurationManager.ConnectionStrings["statsd"]?.ConnectionString);
		private static readonly string machineName = Environment.MachineName.Replace(".", "_").ToLower();

		public static void PreStart()
		{
			if (!IsGraphiteSendingEnabled)
				return;

			// Make sure MetricsPipe handles BeginRequest and EndRequest
			DynamicModuleUtility.RegisterModule(typeof(MetricsPipeStartupModule));

			MetricsPipeStartupModule.Settings.ReportRequestTime = true;
			MetricsPipeStartupModule.Settings.RequestTimePrefix = $"web.{machineName}.request.time";
		}
	}
}