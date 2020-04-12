﻿using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CourseToolHotReloader.Dtos;

namespace CourseToolHotReloader.ApiClient
{
	public class HttpMethods
	{
		public static void TestGetJWTToken()
		{
			var task = GetJwtToken(new LoginPasswordParameters { Login = "admin", Password = "fullcontrol" });
			task.Wait();
			var accountTokenResponseDto = task.Result;
			var token = accountTokenResponseDto.Token;
			Console.WriteLine(token);
		}

		public static void TestCreateCourse()
		{
			var task = GetJwtToken(new LoginPasswordParameters { Login = "admin", Password = "fullcontrol" });
			task.Wait();
			var accountTokenResponseDto = task.Result;
			var token = accountTokenResponseDto.Token;

			const string path = "C:/Projects/ulearn/Ulearn/courses/Help/Help.zip";

			var task2 = UploadCourse(path, token);
			task2.Wait();
		}

		private static async Task<AccountTokenResponseDto> GetJwtToken(LoginPasswordParameters parameters)
		{
			const string baseUrl = "http://localhost:8000";
			var url = $"{baseUrl}/account/login";

			var json = JsonSerializer.Serialize(parameters);
			var data = new StringContent(json, Encoding.UTF8, "application/json");

			using var client = new HttpClient();

			var response = await client.PostAsync(url, data);

			var result = response.Content.ReadAsStringAsync().Result;
			return JsonSerializer.Deserialize<AccountTokenResponseDto>(result);
		}

		private static async Task UploadCourse(string path, string token)
		{
			const string baseUrl = "http://localhost:8000";
			var url = $"{baseUrl}/tempCourses/uploadCourse/123";
			
			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Bearer", token);

			var multiForm = new MultipartFormDataContent();
			var fileStream = File.OpenRead(path);
			var streamContent = new StreamContent(fileStream);
			multiForm.Add(streamContent, "files", Path.GetFileName(path));
			var response = await client.PostAsync(url, multiForm);

			var result = response.Content.ReadAsStringAsync().Result;
		}

	}
}