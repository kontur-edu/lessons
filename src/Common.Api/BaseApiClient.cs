using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vostok.Logging.Abstractions;
using Ulearn.Common.Api.Models.Parameters;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Common.Extensions;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Transport;
using Newtonsoft.Json;
using Vostok.Clusterclient.Core.Criteria;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Logging.Tracing;
using Vostok.Telemetry.Kontur;

namespace Ulearn.Common.Api
{
	public class BaseApiClient
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(BaseApiClient)).WithTracingProperties(KonturTracerProvider.Get());
		private readonly ApiClientSettings settings;
		private readonly ClusterClient clusterClient;

		protected BaseApiClient(ApiClientSettings settings)
		{
			this.settings = settings;

			clusterClient = new ClusterClient(log, config =>
			{
				config.SetupUniversalTransport();
				config.DefaultTimeout = settings.DefaultTimeout;
				config.SetupDistributedKonturTracing();
				config.ClusterProvider = new FixedClusterProvider(settings.EndpointUrl);
				config.TargetServiceName = settings.ServiceName;
				config.DefaultRequestStrategy = Strategy.SingleReplica;
				config.SetupResponseCriteria(
					//new AcceptNonRetriableCriterion(), // Пока у нас одна реплика, используем результат, какой есть
					//new RejectNetworkErrorsCriterion(),
					//new RejectServerErrorsCriterion(),
					//new RejectThrottlingErrorsCriterion(),
					//new RejectUnknownErrorsCriterion(),
					//new RejectStreamingErrorsCriterion(),
					new AlwaysAcceptCriterion()
				);
			});
		}

		protected async Task<TResult> MakeRequestAsync<TParameters, TResult>(HttpMethod method, string url, TParameters parameters)
			where TParameters : ApiParameters
			where TResult : ApiResponse
		{
			ClusterResult response;
			if (settings.LogRequestsAndResponses)
				log.Info("Send {method} request to {serviceName} ({url}) with parameters: {parameters}", method.Method, settings.ServiceName, url, parameters.ToString());

			try
			{
				if (method == HttpMethod.Get)
				{
					var request = Request.Get(BuildUrl(settings.EndpointUrl + url));
					var parametersNameValueCollection = parameters.ToNameValueCollection();
					foreach (var key in parametersNameValueCollection.AllKeys)
						request = request.WithAdditionalQueryParameter(key, parametersNameValueCollection[key]);
					response = await clusterClient.SendAsync(request).ConfigureAwait(false);
				}
				else if (method == HttpMethod.Post)
				{
					var serializedPayload = JsonConvert.SerializeObject(parameters, Formatting.Indented);
					var request = Request
						.Post(BuildUrl(settings.EndpointUrl + url))
						.WithContent(serializedPayload, Encoding.UTF8)
						.WithContentTypeHeader("application/json");
					response = await clusterClient.SendAsync(request).ConfigureAwait(false);
				}
				else
					throw new ApiClientException($"Internal error: unsupported http method: {method.Method}");
			}
			catch (Exception e)
			{
				log.Error(e, "Can't send request to {serviceName}: {message}", settings.ServiceName, e.Message);
				throw new ApiClientException($"Can't send request to {settings.ServiceName}: {e.Message}", e);
			}
			
			if (response.Status != ClusterResultStatus.Success)
			{
				log.Error("Bad response status from {serviceName}: {status}", settings.ServiceName, response.Status.ToString());
				throw new ApiClientException($"Bad response status from {settings.ServiceName}: {response.Status}");
			}

			if (!response.Response.IsSuccessful)
			{
				log.Error("Bad response code from {serviceName}: {statusCode} {statusCodeDescrption}", settings.ServiceName, (int)response.Response.Code, response.Response.Code.ToString());
				throw new ApiClientException($"Bad response code from {settings.ServiceName}: {(int)response.Response.Code} {response.Response.Code}");
			}

			TResult result;
			string jsonResult = null;
			try
			{
				if (response.Response.HasStream)
				{
					var reader = new StreamReader(response.Response.Stream, Encoding.UTF8);
					jsonResult = await reader.ReadToEndAsync();
				}
				else if (response.Response.HasContent)
				{
					var content = response.Response.Content;
					jsonResult = Encoding.UTF8.GetString(content.Buffer, content.Offset, content.Length);
				}
				result = JsonConvert.DeserializeObject<TResult>(jsonResult);
			}
			catch (Exception e)
			{
				log.Error(e, "Can't parse response from {serviceName}: {message}", settings.ServiceName, e.Message);
				throw new ApiClientException($"Can't parse response from {settings.ServiceName}: {e.Message}", e);
			}

			if (settings.LogRequestsAndResponses)
			{
				var shortened = false;
				var logResult = jsonResult;
				if (jsonResult.Length > 8 * 1024)
				{
					logResult = result.GetShortLogString();
					shortened = true;
				}

				log.Info($"Received response from \"{settings.ServiceName}\"{(shortened ? " (сокращенный)" : "")}: {logResult}");
			}

			return result;
		}

		private Uri BuildUrl(string url, NameValueCollection parameters = null)
		{
			if (parameters == null)
				parameters = new NameValueCollection();

			var builder = new UriBuilder(url);
			var queryString = WebUtils.ParseQueryString(builder.Query);
			foreach (var parameterName in parameters.AllKeys)
				queryString[parameterName] = parameters[parameterName];

			builder.Query = queryString.ToQueryString();
			return AddCustomParametersToUrl(builder.Uri);
		}

		protected virtual Uri AddCustomParametersToUrl(Uri url)
		{
			return url;
		}
	}

	public class ApiClientSettings
	{
		public ApiClientSettings(string endpointUrl)
		{
			endpointUrl = endpointUrl.Replace("localhost", Environment.MachineName.ToLowerInvariant());
			EndpointUrl = new Uri(endpointUrl);
		}

		public readonly Uri EndpointUrl;

		public TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

		public string ServiceName = "ulearn.service";

		public bool LogRequestsAndResponses = true;
	}
}