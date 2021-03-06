﻿using System;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;
using StatsdClient;
using Ulearn.Core.Configuration;

namespace Ulearn.Core.Metrics
{
	public class MetricSender
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(MetricSender));

		private readonly string prefix;
		private readonly string service;
		private static string MachineName { get; } = Environment.MachineName.Replace(".", "_").ToLower();
		private readonly Statsd statsd;
		private bool IsEnabled => statsd != null;

		public MetricSender([CanBeNull] string service)
		{
			var connectionString = ApplicationConfiguration.Read<UlearnConfiguration>().StatsdConnectionString;
			if (string.IsNullOrEmpty(connectionString))
				return;

			var config = StatsdConfiguration.CreateFrom(connectionString);
			prefix = config.Prefix;
			this.service = service ?? System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToLower();

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
		public static string BuildKey(string prefix, string service, string key)
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

			var builtKey = BuildKey(prefix, service, key);
			log.Info($"Send count metric {builtKey}, value {value}");
			try
			{
				statsd.Send<Statsd.Counting>(builtKey, value);
			}
			catch (Exception e)
			{
				log.Warn(e, $"Can't send count metric {builtKey}, value {value}");
			}
		}

		public void SendTiming(string key, int value)
		{
			if (!IsEnabled)
				return;

			var builtKey = BuildKey(prefix, service, key);
			log.Info($"Send timing metric {builtKey}, value {value}");
			try
			{
				statsd.Send<Statsd.Timing>(builtKey, value);
			}
			catch (Exception e)
			{
				log.Warn(e, $"Can't send timing metric {builtKey}, value {value}");
			}
		}

		public void SendGauge(string key, double value)
		{
			if (!IsEnabled)
				return;

			var builtKey = BuildKey(prefix, service, key);
			log.Info($"Send gauge metric {builtKey}, value {value}");
			try
			{
				statsd.Send<Statsd.Gauge>(builtKey, value);
			}
			catch (Exception e)
			{
				log.Warn(e, $"Can't send gauge metric {builtKey}, value {value}");
			}
		}
	}
}