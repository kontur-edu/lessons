using System;
using System.Web;
using Serilog;
using SerilogWeb.Classic;
using Ulearn.Core.Configuration;
using Ulearn.Core.Logging;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Serilog;

namespace uLearn.Web
{
	public static class UlearnLogger
	{
		public static void ConfigureLogging()
		{
			var configuration = ApplicationConfiguration.Read<UlearnConfiguration>();
			var log = LoggerSetup.Setup(configuration.HostLog, configuration.GraphiteServiceName, false)
				.WithProperty("user", () => new HttpContextUserNameProvider().ToString())
				.WithProperty("address", () => new HttpContextAddressProvider().ToString());
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Sink(new VostokSink(log))
				.CreateLogger();
			SerilogWebClassic.Configure(cfg => cfg.UseLogger(Log.Logger));
			LogProvider.Configure(log);
		}
	}

	public class HttpContextUserNameProvider
	{
		public override string ToString()
		{
			var context = HttpContext.Current;
			if (context?.User != null && context.User.Identity.IsAuthenticated)
				return context.User.Identity.Name;
			return "(unknown user)";
		}
	}

	public class HttpContextAddressProvider
	{
		private const string xRealIpHeaderName = "X-Real-IP";

		public override string ToString()
		{
			var context = HttpContext.Current;
			try
			{
				var realIp = context?.Request.Headers[xRealIpHeaderName];
				if (!string.IsNullOrEmpty(realIp))
					return realIp;
				return context?.Request.UserHostAddress ?? "";
			}
			catch (HttpException ex) // context?.Request throw if no Request (in App Start) 
			{
				return "";
			}
		}
	}
}