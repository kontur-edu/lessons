using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CourseToolHotReloader.Dtos;
using CourseToolHotReloader.Exceptions;

using JetBrains.Annotations;

namespace CourseToolHotReloader.ApiClient
{
	public interface IHttpMethods
	{
		Task<TokenResponseDto> GetJwtToken(LoginPasswordParameters parameters);
		Task<TempCourseUpdateResponse> UploadCourse(MemoryStream memoryStream, string id);
		Task<TempCourseUpdateResponse> UploadFullCourse(MemoryStream memoryStream, string id);
		Task<TempCourseUpdateResponse> CreateCourse(string id);
		Task<CoursesListResponse> GetCoursesList();
		Task<TokenResponseDto> RenewToken();
		Task<ShortUserInfo> GetUserInfo();
	}

	public class HttpMethods : IHttpMethods
	{
		private readonly IConfig config;

		public HttpMethods(IConfig config)
		{
			this.config = config;
		}

		[ItemCanBeNull]
		public async Task<TokenResponseDto> GetJwtToken(LoginPasswordParameters parameters)
		{
			var url = $"{config.ApiUrl}/account/login";
			var json = JsonSerializer.Serialize(parameters);
			var data = new StringContent(json, Encoding.UTF8, "application/json");
			using var client = new HttpClient();
			var response = await client.PostAsync(url, data);
			try
			{
				ThrowExceptionIfBadCode(response);
			}
			catch (UnauthorizedException)
			{
				return null;
			}
			catch (ForbiddenException)
			{
				return null;
			}
			var result = response.Content.ReadAsStringAsync().Result;
			return JsonSerializer.Deserialize<TokenResponseDto>(result);
		}

		[ItemCanBeNull]
		public async Task<TokenResponseDto> RenewToken()
		{
			var url = $"{config.ApiUrl}/account/api-token?days=3";
			using var client = HttpClient();
			var response = await client.PostAsync(url, null);
			try
			{
				ThrowExceptionIfBadCode(response);
			}
			catch (UnauthorizedException)
			{
				return null;
			}
			catch (ForbiddenException)
			{
				return null;
			}
			var result = response.Content.ReadAsStringAsync().Result;
			return JsonSerializer.Deserialize<TokenResponseDto>(result);
		}

		public async Task<TempCourseUpdateResponse> UploadCourse(MemoryStream memoryStream, string id)
		{
			var url = $"{config.ApiUrl}/tempCourses/{id}";
			return await UpdateTempCourse(memoryStream, url, HttpMethod.Patch);
		}

		public async Task<TempCourseUpdateResponse> UploadFullCourse(MemoryStream memoryStream, string id)
		{
			var url = $"{config.ApiUrl}/tempCourses/{id}";
			return await UpdateTempCourse(memoryStream, url, HttpMethod.Put);
		}

		public async Task<TempCourseUpdateResponse> CreateCourse(string id)
		{
			var url = $"{config.ApiUrl}/tempCourses/{id}";
			using var client = HttpClient();
			var response = await client.PostAsync(url, null);
			ThrowExceptionIfBadCode(response);
			return DeserializeResponseContent<TempCourseUpdateResponse>(response);
		}

		public async Task<CoursesListResponse> GetCoursesList()
		{
			var url = $"{config.ApiUrl}/courses";
			using var client = HttpClient();
			var response = await client.GetAsync(url);
			ThrowExceptionIfBadCode(response);
			return response.StatusCode != HttpStatusCode.OK
				? null
				: DeserializeResponseContent<CoursesListResponse>(response);
		}

		public async Task<ShortUserInfo> GetUserInfo()
		{
			var url = $"{config.ApiUrl}/account";
			using var client = HttpClient();
			var response = await client.GetAsync(url);
			ThrowExceptionIfBadCode(response);
			return DeserializeResponseContent<AccountResponse>(response).User;
		}

		private static T DeserializeResponseContent<T>(HttpResponseMessage response)
		{
			var result = response.Content.ReadAsStringAsync().Result;
			return JsonSerializer.Deserialize<T>(result);
		}

		private async Task<TempCourseUpdateResponse> UpdateTempCourse(MemoryStream memoryStream, string url, HttpMethod httpMethod)
		{
			using var client = HttpClient();
			memoryStream.Position = 0;
			var fileContent = new ByteArrayContent(memoryStream.ToArray());
			var multiContent = new MultipartFormDataContent { { fileContent, "files", "course.zip" } };
			var response = httpMethod == HttpMethod.Patch ? await client.PatchAsync(url, multiContent) : await client.PutAsync(url, multiContent);
			ThrowExceptionIfBadCode(response);
			return DeserializeResponseContent<TempCourseUpdateResponse>(response);
		}

		private void ThrowExceptionIfBadCode(HttpResponseMessage response)
		{
			switch (response.StatusCode)
			{
				case HttpStatusCode.OK:
					return;
				case HttpStatusCode.Unauthorized:
					throw new UnauthorizedException();
				case HttpStatusCode.Forbidden:
					throw new ForbiddenException();
				case HttpStatusCode.InternalServerError:
					string message = null;
					try
					{
						message = DeserializeResponseContent<ServerErrorDto>(response).Message;
					}
					catch (Exception)
					{
						// ignore
					}
					throw new InternalServerErrorException(message);
				default:
					throw new StatusCodeException(response.StatusCode);
			}
		}

		private HttpClient HttpClient()
		{
			var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Bearer", config.JwtToken);
			return client;
		}
	}
}