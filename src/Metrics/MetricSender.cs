using System;
using System.Configuration;
using System.Linq;
using log4net;
using StatsdClient;

namespace Metrics
{
	public class MetricSender
	{
		private readonly ILog log = LogManager.GetLogger(typeof(MetricSender));

		private readonly string prefix;
		private readonly string service;
		private static string MachineName { get; } = Environment.MachineName.Replace(".", "_").ToLower();
		private readonly Statsd statsd;
		private bool IsEnabled => statsd != null;

		public MetricSender(string service)
		{
			var connectionString = ConfigurationManager.ConnectionStrings["statsd"]?.ConnectionString;
			if (string.IsNullOrEmpty(connectionString))
				return;

			var config = StatsdConfiguration.CreateFrom(connectionString);
			prefix = config.Prefix;
			this.service = service;

			statsd = CreateStatsd(config);
		}

		private static Statsd CreateStatsd(StatsdConfiguration config)
		{
			var client = config.IsTCP
				? (IStatsdClient)new StatsdTCPClient(config.Address, config.Port)
				: new StatsdUDPClient(config.Address, config.Port);
			return new Statsd(client, new RandomGenerator(), new StopWatchFactory());
		}

		/* Builds key "{prefix}.{service}.{machine_name}.{key}" */
		private string BuildKey(string key)
		{
			var parts = new[] { prefix, service, MachineName, key }
				.Where(s => !string.IsNullOrEmpty(s))
				.ToArray();
			return string.Join(".", parts);
		}

		public void SendCount(string key, int value = 1)
		{
			if (!IsEnabled)
				return;

			var builtKey = BuildKey(key);
			log.Info($"Send count metric {builtKey}, value {value}");
			try
			{
				statsd.Send<Statsd.Counting>(builtKey, value);
			}
			catch (Exception e)
			{
				log.Warn($"Can't send count metric {builtKey}, value {value}", e);
			}
		}

		public void SendTiming(string key, int value)
		{
			if (!IsEnabled)
				return;

			var builtKey = BuildKey(key);
			log.Info($"Send timing metric {builtKey}, value {value}");
			try
			{
				statsd.Send<Statsd.Timing>(builtKey, value);
			}
			catch (Exception e)
			{
				log.Warn($"Can't send timing metric {builtKey}, value {value}", e);
			}
		}

		public void SendGauge(string key, double value)
		{
			if (!IsEnabled)
				return;

			var builtKey = BuildKey(key);
			log.Info($"Send gauge metric {builtKey}, value {value}");
			try
			{
				statsd.Send<Statsd.Gauge>(builtKey, value);
			}
			catch (Exception e)
			{
				log.Warn($"Can't send gauge metric {builtKey}, value {value}", e);
			}
		}
	}
}