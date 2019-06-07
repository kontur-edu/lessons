using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using log4net;
using RunCsJob.Api;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core;

namespace RunCsJob
{
	internal class Client
	{
		private readonly string token;
		private readonly HttpClient httpClient;
		private const string instanceIdEnvironmentVariableName = "WEBSITE_INSTANCE_ID";
		private const string arrAffinityCookieName = "ARRAffinity";
		private static readonly ILog log = LogManager.GetLogger(typeof(Client));
		private readonly string agentName;

		public Client(string address, string token, string agentName)
		{
			this.token = token;

			var cookieContainer = new CookieContainer();
			var httpClientHandler = new HttpClientHandler { CookieContainer = cookieContainer };
			var baseAddress = new Uri(address + "/");

			httpClient = new HttpClient(httpClientHandler) { BaseAddress = baseAddress };
			httpClient.DefaultRequestHeaders.Accept.Clear();
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			/* Select instanceId for multiple azure instances */
			var instanceId = Environment.GetEnvironmentVariable(instanceIdEnvironmentVariableName);
			if (!string.IsNullOrEmpty(instanceId))
				cookieContainer.Add(baseAddress, new Cookie(arrAffinityCookieName, instanceId));

			this.agentName = agentName;
		}

		public async Task<List<RunnerSubmission>> TryGetSubmission()
		{
			var uri = GetUri("GetSubmissions", new [] {"language", Language.CSharp.GetName()});
			try
			{
				log.Info($"Отправляю запрос на {uri}");
				var response = await httpClient.GetAsync(uri).ConfigureAwait(false);
				log.Info($"Получил ответ, код {(int) response.StatusCode} {response.StatusCode}, читаю содержимое");
				if (response.IsSuccessStatusCode)
					return await response.Content.ReadAsJsonAsync<List<RunnerSubmission>>(JsonConfig.GetSettings()).ConfigureAwait(false);
				else
				{
					var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
					log.Error($"StatusCode {response.StatusCode}\n{text}");
				}
			}
			catch (Exception e)
			{
				log.Error($"Не могу подключиться к {httpClient.BaseAddress}{uri}", e);
				if (e.InnerException != null)
					log.Error(e.InnerException.Message);
			}
			return new List<RunnerSubmission>();
		}

		public async void SendResults(List<RunningResults> results)
		{
			var uri = GetUri("PostResults");
			try
			{
				var response = await TrySendResults(results, uri).ConfigureAwait(false);

				if (response.IsSuccessStatusCode)
					return;

				log.Error($"Не могу отправить результаты проверки (они ниже) на сервер: {response}\n{await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
			}
			catch (Exception e)
			{
				log.Error($"Не могу отправить результаты проверки (они ниже) на сервер. Произошла ошибка {e.Message}");
			}
			
			foreach (var result in results)
			{
				log.Info($"Результат: {result}");
			}
		}

		private Task<HttpResponseMessage> TrySendResults(List<RunningResults> results, string uri)
		{
			return FuncUtils.TrySeveralTimesAsync(async () => await httpClient.PostAsJsonAsync(uri, results).ConfigureAwait(false), 3);
		}

		private string GetUri(string path, params string[][] parameters)
		{
			var query = HttpUtility.ParseQueryString(string.Empty);
			query["token"] = token;
			query["agent"] = agentName;
			foreach (var parameter in parameters)
			{
				query[parameter[0]] = parameter[1];
			}
			return $"{path}/?{query}";
		}
	}
}