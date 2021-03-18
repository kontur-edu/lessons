using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseToolHotReloader.DirectoryWorkers;
using CourseToolHotReloader.Dtos;
using JetBrains.Annotations;

namespace CourseToolHotReloader.ApiClient
{
	public interface IUlearnApiClient
	{
		Task<TempCourseUpdateResponse> SendCourseUpdates(string path, IList<ICourseUpdate> update, IList<ICourseUpdate> deletedFiles, string courseId, List<string> excludeCriterias);
		Task<TempCourseUpdateResponse> SendFullCourse(string path, string courseId, List<string> excludeCriterias);
		Task<TempCourseUpdateResponse> CreateCourse(string courseId);
		Task<string> Login(string login, string password);
		Task<bool> HasCourse(string courseId);
		Task<string> RenewToken();
		Task<ShortUserInfo> GetShortUserInfo();
	}

	internal class UlearnApiClient : IUlearnApiClient
	{
		private readonly IHttpMethods httpMethods;

		public UlearnApiClient(IHttpMethods httpMethods)
		{
			this.httpMethods = httpMethods;
		}

		public async Task<TempCourseUpdateResponse> SendCourseUpdates(string path, IList<ICourseUpdate> updates, IList<ICourseUpdate> deletedFiles, string courseId, List<string> excludeCriterias)
		{
			using (var ms = ZipUpdater.CreateZipByUpdates(path, updates, deletedFiles, excludeCriterias))
				return await httpMethods.UploadCourse(ms, courseId);
		}

		[ItemCanBeNull]
		public async Task<TempCourseUpdateResponse> SendFullCourse(string path, string courseId, List<string> excludeCriterias)
		{
			using (var ms = ZipUpdater.CreateZipByFolder(path, excludeCriterias))
				return await httpMethods.UploadFullCourse(ms, courseId);
		}
		
		public async Task<TempCourseUpdateResponse> CreateCourse(string courseId)
		{
			return await httpMethods.CreateCourse(courseId);
		}

		public async Task<bool> HasCourse(string courseId)
		{
			var coursesList = await httpMethods.GetCoursesList();
			return coursesList?.Courses.Any(c => string.Compare(c.Id, courseId, StringComparison.OrdinalIgnoreCase) == 0) ?? false;
		}

		[ItemCanBeNull]
		public async Task<string> Login(string login, string password)
		{
			var loginPasswordParameters = new LoginPasswordParameters
			{
				Login = login,
				Password = password
			};

			var accountTokenResponseDto = await httpMethods.GetJwtToken(loginPasswordParameters);
			return accountTokenResponseDto?.Token;
		}
		
		[ItemCanBeNull]
		public async Task<string> RenewToken()
		{
			var accountTokenResponseDto = await httpMethods.RenewToken();
			return accountTokenResponseDto?.Token;
		}

		public async Task<ShortUserInfo> GetShortUserInfo()
		{
			return await httpMethods.GetUserInfo();
		}
	}
}