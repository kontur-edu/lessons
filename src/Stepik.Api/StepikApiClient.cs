using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json.Linq;
using Ulearn.Common.Extensions;

namespace Stepik.Api
{
	/* This is REST API client for stepik.org. 
	 * Documentation is available at https://docs.google.com/document/d/1PIToDEI_ESD4rMU2HIIXrzMdAO-9Os5qLm1ZRwaXjao/edit
	 * API endpoints are listed at https://stepik.org/api/docs/
	 */
    public class StepikApiClient
    {
	    private const string apiBaseUrl = "https://stepik.org/api/";
	    private const string tokenUrl = "https://stepik.org/oauth2/token/";
	    private const string tokenGrantType = "authorization_code";

	    private readonly ILog log = LogManager.GetLogger(typeof(StepikApiClient));
		private readonly StepikApiOptions options;
		private readonly HttpClient httpClient;
		
		public string AccessToken { get; private set; }

	    public StepikApiClient(StepikApiOptions options)
	    {
		    this.options = options;
		    httpClient = new HttpClient
		    {
			    BaseAddress = new Uri(apiBaseUrl),
		    };

			if (!string.IsNullOrEmpty(options.AccessToken))
		    {
			    AccessToken = options.AccessToken;
			    httpClient.DefaultRequestHeaders.Authorization = GetAutorizationHeaderWithToken(AccessToken);
			    isAuthorizationHeaderSet = true;
		    }
	    }

		private bool isAuthorizationHeaderSet = false;
	    public async Task RetrieveAccessTokenFromAuthorizationCode()
	    {
		    if (isAuthorizationHeaderSet)
			    return;

			httpClient.DefaultRequestHeaders.Authorization = await GetAutorizationHeaderWithToken();
			isAuthorizationHeaderSet = true;
	    }

		private async Task<AuthenticationHeaderValue> GetAutorizationHeaderWithToken()
		{
			try
			{
				AccessToken = await GetAccessTokenByAuthorizationCode();
			}
			catch (StepikApiException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new StepikApiException($"Can't get access token by authorization code: {e.Message}", e);
			}
			return GetAutorizationHeaderWithToken(AccessToken);
		}

	    private static AuthenticationHeaderValue GetAutorizationHeaderWithToken(string accessToken)
	    {
		    return new AuthenticationHeaderValue("Bearer", accessToken);
	    }

		private async Task<string> GetAccessTokenByAuthorizationCode()
	    {
		    var client = new HttpClient();
		    var formContent = new Dictionary<string, string> { { "grant_type", tokenGrantType }, { "code", options.AuthorizationCode }, { "redirect_uri", options.RedirectUri } };
		    client.DefaultRequestHeaders.Authorization = GetBasicAuthorizationHeader(options.ClientId, options.ClientSecret);

		    log.Info($"Try to get access token for code \"{options.AuthorizationCode.MaskAsSecret()}\". Sending request to {tokenUrl}");
			var response = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(formContent));
		    if (!response.IsSuccessStatusCode)
			    throw new StepikApiException($"Can't get access token for code \"{options.AuthorizationCode.MaskAsSecret()}\". HTTP API response status code is {(int) response.StatusCode} {response.StatusCode}");

		    var responseContent = await response.Content.ReadAsStringAsync();
			log.Debug($"HTTP API status code: {(int) response.StatusCode} {response.StatusCode}. Response: {responseContent}");
		    var parsedResponse = responseContent.DeserializeJson<StepikApiAccessToken>();
		    return parsedResponse.AccessToken;
	    }

	    private AuthenticationHeaderValue GetBasicAuthorizationHeader(string username, string password)
	    {
		    var authBytes = Encoding.ASCII.GetBytes($"{username}:{password}");
		    return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
	    }

	    private async Task<string> MakeRequest(HttpMethod method, string url, object content=null)
	    {
			log.Info($"Making {method} request to Stepik API: {url}");

			if (content != null)
				log.Debug($"Request content: {content.JsonSerialize()}");

		    HttpResponseMessage response;
		    if (method == HttpMethod.Get)
				response = await httpClient.GetAsync(url);
			else if (method == HttpMethod.Delete)
			    response = await httpClient.DeleteAsync(url);
			else if (method == HttpMethod.Post)
		    {
				/* If content is not a HttpContent, pack them in JSON */
			    if (content != null && content.GetType().IsSubclassOf(typeof(HttpContent)))
				    response = await httpClient.PostAsync(url, (HttpContent) content);
			    else
				    response = await httpClient.PostAsJsonAsync(url, content);
		    }
		    else if (method == HttpMethod.Put)
		    {
			    if (content != null && content.GetType().IsSubclassOf(typeof(HttpContent)))
				    response = await httpClient.PutAsync(url, (HttpContent)content);
			    else
				    response = await httpClient.PutAsJsonAsync(url, content);
		    }
		    else
			    throw new StepikApiException($"Invalid HTTP method ({method}) for making request to {url}");

		    var responseContent = await response.Content.ReadAsStringAsync();
		    log.Debug($"Response: {responseContent}");
		    if (!response.IsSuccessStatusCode)
			    throw new StepikApiException($"Can't get response from Stepik API. Url: {response.RequestMessage.RequestUri}. HTTP status code: {(int)response.StatusCode} {response.StatusCode}. HTTP content: {responseContent}");

		    return responseContent;
		}

	    /* I.e. Get<StepikApiUser>(123456) or Get<StepikApiSubmission>(123456, "file") */
	    private async Task<TObject> Get<TObject>(int? objectPrimaryKey=null, string additional="")
		where TObject : StepikApiObject
		{
			var listQueryAdditional = objectPrimaryKey.HasValue ? $"{objectPrimaryKey}" : "";
			if (!string.IsNullOrEmpty(additional))
				listQueryAdditional += "/" + additional;

				
			var valuesList = await GetList<TObject>(additional: listQueryAdditional);
			if (valuesList.Count == 0)
				throw new StepikApiException($"Stepik API: object {typeof(TObject).Name} #{objectPrimaryKey} not found");

			return valuesList[0];
		}

		/* TODO (andgein): process `meta` dictionary in response, process paginated response */
	    private async Task<List<TObject>> GetList<TObject>(IDictionary<string, string> parameters=null, string additional = "") where TObject : StepikApiObject
	    {
		    var apiEndpoint = typeof(TObject).GetApiEndpoint();

		    var url = string.IsNullOrEmpty(additional) ? apiEndpoint : $"{apiEndpoint}/{additional}";
		    if (parameters != null)
		    {
			    var builder = new UriBuilder(apiBaseUrl + url) { Query = parameters.ToQueryString() };
			    url = builder.ToString();
		    }
			
		    var response = await MakeRequest(HttpMethod.Get, url);
			/* JSON in response always contains key with the same name as an API endpoint*/
		    return ExtractOneJsonField<List<TObject>>(response, apiEndpoint);
	    }

	    private static TValue ExtractOneJsonField<TValue>(string json, string key)
	    {
		    var jObject = JObject.Parse(json);
			if (! jObject.TryGetValue(key, out JToken value))
				throw new KeyNotFoundException($"Key {key} not found in JSON");

		    var valueJson = value.ToString();
		    return valueJson.DeserializeJson<TValue>();
	    }

	    private async Task<TResponseObject> MakeUpdateRequest<TObject, TResponseObject>(HttpMethod method, TObject obj)
			where TObject : StepikApiObject
			where TResponseObject : StepikApiObject
	    {
		    if (method != HttpMethod.Post && method != HttpMethod.Put)
			    throw new StepikApiException($"Invalid method ({method}) for updating entity: {obj.JsonSerialize()}");

		    var apiEndpoint = obj.GetApiEndpoint();
		    var url = apiEndpoint;
		    if (obj.Id.HasValue)
		        url += $"/{obj.Id}";

			/* We should send JSON with following structure (i.e. for lesson): 
			 * {
			 *	  'lesson': {
			 *	     ... (StepikApiLesson) ...
			 *	  }
			 * }
			 */
		    var request = new Dictionary<string, TObject> { [obj.GetApiPutJsonKeyName()] = obj };
		    var response = await MakeRequest(method, url, request);
			
			if (typeof(TResponseObject).IsSubclassOf(typeof(IEnumerable)))
				return ExtractOneJsonField<TResponseObject>(response, apiEndpoint);

		    return GetFirstListItemFromResponse<TResponseObject>(response);
	    }

	    private static TResponseObject GetFirstListItemFromResponse<TResponseObject>(string response, string jsonKeyName="") where TResponseObject : StepikApiObject
	    {
		    if (string.IsNullOrEmpty(jsonKeyName))
			    jsonKeyName = typeof(TResponseObject).GetApiEndpoint();

		    var valuesList = ExtractOneJsonField<List<TResponseObject>>(response, jsonKeyName);
		    if (valuesList.Count == 0)
			    throw new StepikApiException($"Can't fetch object {jsonKeyName} from response: {response}");

		    return valuesList[0];
	    }

	    private Task<TResponseObject> Put<TObject, TResponseObject>(TObject obj)
			where TObject : StepikApiObject
			where TResponseObject : StepikApiObject
	    {
		    return MakeUpdateRequest<TObject, TResponseObject>(HttpMethod.Put, obj);
	    }

	    private Task<TResponseObject> Post<TObject, TResponseObject>(TObject obj)
		    where TObject : StepikApiObject
		    where TResponseObject : StepikApiObject
	    {
		    return MakeUpdateRequest<TObject, TResponseObject>(HttpMethod.Post, obj);
	    }

	    private Task<TObject> Post<TObject>(TObject obj)
		    where TObject : StepikApiObject
	    {
		    return Post<TObject, TObject>(obj);
	    }

	    private async Task<TResponse> PostForm<TResponse>(IDictionary<string, string> formData) where TResponse : StepikApiObject
	    {
		    var url = typeof(TResponse).GetApiEndpoint();
		    var response = await MakeRequest(HttpMethod.Post, url, new FormUrlEncodedContent(formData));
		    return GetFirstListItemFromResponse<TResponse>(response);
	    }

		private Task Delete(string apiEndpoint, int objectPrimaryKey)
	    {
		    var url = $"{apiEndpoint}/{objectPrimaryKey}";
		    return MakeRequest(HttpMethod.Delete, url);
	    }

	    private Task Delete<TObject>(int objectPrimaryKey)
		    where TObject : StepikApiObject
		{
			return Delete(typeof(TObject).GetApiEndpoint(), objectPrimaryKey);
		}

		private Task Delete<TObject>(TObject obj)
		    where TObject : StepikApiObject
	    {
		    if (! obj.Id.HasValue)
			    throw new StepikApiException($"Can't delete entity without Id: {obj.JsonSerialize()}");

			return Delete(obj.GetApiEndpoint(), obj.Id.Value);
	    }

		private int? stepikUserId;

		public async Task<int> GetCurrentStepikUserId()
	    {
			if (stepikUserId.HasValue)
				return stepikUserId.Value;
			var stepic = await Get<StepikApiStepic>(1);
		    stepikUserId = stepic.UserId;
		    return stepic.UserId;
	    }

	    public async Task<bool> IsAuthenticated()
	    {
		    var user = await GetUserInfo();
		    return ! user.IsGuest;
	    }

		/* Methods implemented concrete API requests */

	    public async Task<StepikApiUser> GetUserInfo()
	    {
		    var userId = await GetCurrentStepikUserId();
		    return await Get<StepikApiUser>(userId);
	    }

	    public async Task<List<StepikApiCourse>> GetMyCourses()
	    {
		    var userId = await GetCurrentStepikUserId();
		    var parameters = new Dictionary<string, string> { {"teacher", userId.ToString()}};
		    return await GetList<StepikApiCourse>(parameters);
	    }

	    public Task<StepikApiCourse> GetCourse(int courseId)
	    {
		    return Get<StepikApiCourse>(courseId);
	    }

	    public Task<StepikApiSection> GetSection(int sectionId)
	    {
		    return Get<StepikApiSection>(sectionId);
	    }

	    public Task DeleteSection(int sectionId)
	    {
		    return Delete<StepikApiSection>(sectionId);
	    }

	    public Task<StepikApiSection> UploadSection(StepikApiSection section)
	    {
		    return Post(section);
	    }

	    public Task<StepikApiUnit> GetUnit(int unitId)
	    {
		    return Get<StepikApiUnit>(unitId);
	    }

	    public Task<StepikApiUnit> UploadUnit(StepikApiUnit unit)
	    {
		    return Post(unit);
	    }

		public Task<StepikApiLesson> GetLesson(int lessonId)
	    {
		    return Get<StepikApiLesson>(lessonId);
	    }

	    public Task<StepikApiLesson> UploadLesson(StepikApiLesson lesson)
	    {
		    return Post(lesson);
	    }

		public Task<StepikApiStep> GetStep(int stepId)
	    {
		    return Get<StepikApiStep>(stepId);
	    }

	    public Task<StepikApiStepSource> UploadStep(StepikApiStepSource stepSource)
	    {
		    return Post(stepSource);
	    }

	    public Task DeleteStep(int stepId)
	    {
			return Delete<StepikApiStepSource>(stepId);
		}

	    public async Task MoveStep(int stepId, int position)
	    {
		    var stepSource = await Get<StepikApiStepSource>(stepId);
		    stepSource.Position = position;
		    await Put<StepikApiStepSource, StepikApiStepSource>(stepSource);
	    }

		public Task<StepikApiVideo> UploadVideo(string sourceUrl, int lessonId)
	    {
		    var formData = new Dictionary<string, string>
		    {
			    { "source_url", sourceUrl },
			    { "lesson", lessonId.ToString() }
		    };
		    return PostForm<StepikApiVideo>(formData);
	    }
    }

	public class StepikApiException : Exception
	{
		public StepikApiException()
		{
		}

		public StepikApiException(string message)
			: base(message)
		{
		}

		public StepikApiException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected StepikApiException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

	public class StepikApiOptions
	{
		public string ClientId { get; set; }

		public string ClientSecret { get; set; }

		public string AccessToken { get; set; }

		public string AuthorizationCode { get; set; }

		public string RedirectUri { get; set; }
	}
}
