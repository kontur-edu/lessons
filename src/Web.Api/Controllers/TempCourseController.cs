using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.Users;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Extensions;
using Ulearn.Web.Api.Models.Responses.TempCourses;
using Ionic.Zip;
using Vostok.Logging.Abstractions;


namespace Ulearn.Web.Api.Controllers
{
	[Route("/tempCourses")]
	public class TempCourseController : BaseController
	{
		private readonly ITempCoursesRepo tempCoursesRepo;
		private readonly ICourseRolesRepo courseRolesRepo;
		public bool DontCheckBaseCourseExistsOnCreate = false; // Для тестрирования
		private static ILog log => LogProvider.Get().ForContext(typeof(TempCourseController));

		public TempCourseController(IWebCourseManager courseManager, UlearnDb db, [CanBeNull] IUsersRepo usersRepo, ITempCoursesRepo tempCoursesRepo, ICourseRolesRepo courseRolesRepo)
			: base(courseManager, db, usersRepo)
		{
			this.tempCoursesRepo = tempCoursesRepo;
			this.courseRolesRepo = courseRolesRepo;
		}

		/// <summary>
		/// Создать временный курс для базового курса с courseId
		/// </summary>
		[Authorize]
		[HttpPost("{courseId}")]
		public async Task<ActionResult<TempCourseUpdateResponse>> CreateCourse([FromRoute] string courseId)
		{
			var tmpCourseId = GetTmpCourseId(courseId, UserId);

			if (!DontCheckBaseCourseExistsOnCreate && !await courseManager.HasCourseAsync(courseId))
			{
				return new TempCourseUpdateResponse
				{
					ErrorType = ErrorType.NotFound,
					Message = $"Не существует курса {courseId}"
				};
			}

			if (!await courseRolesRepo.HasUserAccessToCourseAsync(UserId, courseId, CourseRoleType.CourseAdmin))
			{
				return new TempCourseUpdateResponse
				{
					ErrorType = ErrorType.Forbidden,
					Message = $"Необходимо быть администратором курса {courseId}"
				};
			}

			var tmpCourse = await tempCoursesRepo.FindAsync(tmpCourseId);
			if (tmpCourse != null)
			{
				return new TempCourseUpdateResponse
				{
					ErrorType = ErrorType.Conflict,
					Message = $"Ваша временная версия курса {courseId} уже существует с id {tmpCourseId}."
				};
			}

			var versionId = Guid.NewGuid();
			var courseTitle = "Заготовка временного курса";
			if (!courseManager.TryCreateTempCourse(tmpCourseId, courseTitle, versionId))
				throw new Exception();

			var result = await tempCoursesRepo.AddTempCourseAsync(tmpCourseId, UserId);
			var loadingTime = result.LoadingTime;
			await courseRolesRepo.ToggleRoleAsync(tmpCourseId, UserId, CourseRoleType.CourseAdmin, UserId, "Создал временный курс");
			return new TempCourseUpdateResponse
			{
				Message = $"Временный курс с id {tmpCourseId} успешно создан.",
				LastUploadTime = loadingTime
			};
		}

		/// <summary>
		/// Ошибки при загрузке временного курса c courseId
		/// </summary>
		[Authorize(Policy = "Instructors")]
		[HttpGet("{courseId}/errors")]
		public async Task<ActionResult<TempCourseErrorsResponse>> GetError([FromRoute] string courseId)
		{
			var tmpCourseError = await tempCoursesRepo.GetCourseErrorAsync(courseId);
			return new TempCourseErrorsResponse
			{
				TempCourseError = tmpCourseError?.Error
			};
		}

		/// <summary>
		/// Загрузить изменения временного курса для базового курса с courseId
		/// </summary>
		[Authorize]
		[HttpPatch("{courseId}")]
		public async Task<ActionResult<TempCourseUpdateResponse>> UploadCourse([FromRoute] string courseId, List<IFormFile> files)
		{
			return await UploadCourse(courseId, files, false);
		}

		/// <summary>
		/// Загрузить целиком временный курс для базового курса с courseId
		/// </summary>
		[HttpPut("{courseId}")]
		[Authorize]
		public async Task<ActionResult<TempCourseUpdateResponse>> UploadFullCourse([FromRoute] string courseId, List<IFormFile> files)
		{
			return await UploadCourse(courseId, files, true);
		}

		private async Task<TempCourseUpdateResponse> UploadCourse(string courseId, List<IFormFile> files, bool isFull)
		{
			var tmpCourseId = GetTmpCourseId(courseId, UserId);
			var tmpCourse = await tempCoursesRepo.FindAsync(tmpCourseId);
			if (tmpCourse is null)
			{
				return new TempCourseUpdateResponse
				{
					ErrorType = ErrorType.NotFound,
					Message = $"Вашей временной версии курса {courseId} не существует. Для создания испрользуйте метод Create"
				};
			}

			if (!await courseRolesRepo.HasUserAccessToCourseAsync(UserId, courseId, CourseRoleType.CourseAdmin))
			{
				return new TempCourseUpdateResponse
				{
					ErrorType = ErrorType.Forbidden,
					Message = $"Необходимо быть администратором курса {tmpCourseId}"
				};
			}

			if (files.Count != 1)
			{
				throw new Exception($"Expected count of files is 1, but was {files.Count}");
			}

			var file = files.Single();
			if (file == null || file.Length <= 0)
				throw new Exception("The file is empty");
			var fileName = Path.GetFileName(file.FileName);
			if (fileName == null || !fileName.ToLower().EndsWith(".zip"))
				throw new Exception("The file should have .zip extension");
			UploadChanges(tmpCourseId, file.OpenReadStream().ToArray());
			var filesToDelete = ExtractFileNamesToDelete(tmpCourseId);
			var error = await TryPublishChanges(tmpCourseId, filesToDelete, isFull);
			if (error != null)
			{
				await tempCoursesRepo.UpdateOrAddTempCourseErrorAsync(tmpCourseId, error);
				return new TempCourseUpdateResponse
				{
					Message = error,
					ErrorType = ErrorType.CourseError
				};
			}

			await tempCoursesRepo.MarkTempCourseAsNotErroredAsync(tmpCourseId);
			var loadingTime = await tempCoursesRepo.UpdateTempCourseLoadingTimeAsync(tmpCourseId);
			return new TempCourseUpdateResponse
			{
				Message = $"Временный курс {tmpCourseId} успешно обновлен",
				LastUploadTime = loadingTime
			};
		}

		private List<string> ExtractFileNamesToDelete(string tmpCourseId)
		{
			var stagingFile = courseManager.GetStagingTempCourseFile(tmpCourseId);
			var filesToDelete = new List<string>();
			using (var zip = ZipFile.Read(stagingFile.FullName))
			{
				var e = zip["deleted.txt"];
				if (e is null)
					return new List<string>();
				var r = e.OpenReader();
				using var sr = new StreamReader(r);
				while (!sr.EndOfStream)
				{
					var line = sr.ReadLine();
					if (!string.IsNullOrEmpty(line))
						filesToDelete.Add(line);
				}
			}

			return filesToDelete
				.Select(x =>
				{
					if (x.StartsWith('\\') || x.StartsWith('/'))
						return x.Substring(1);
					return x;
				}).ToList();
		}

		private async Task<string> TryPublishChanges(string courseId, List<string> filesToDelete, bool isFull)
		{
			var revertStructure = GetRevertStructure(courseId, filesToDelete, isFull);
			DeleteFiles(revertStructure.DeletedFiles, revertStructure.DeletedDirectories);
			var courseDirectory = courseManager.GetExtractedCourseDirectory(courseId);
			DeleteEmptySubdirectories(courseDirectory.FullName);
			courseManager.ExtractTempCourseChanges(courseId);

			try
			{
				courseManager.ReloadCourseNotSafe(courseId);
				courseManager.UpdateCourseVersion(courseId, Guid.Empty);
				courseManager.NotifyCourseChanged(courseId);
			}
			catch (Exception error)
			{
				var errorMessage = error.Message;
				while (error.InnerException != null)
				{
					errorMessage += $"\n\n{error.InnerException.Message}";
					error = error.InnerException;
				}

				revertStructure.Revert();
				return errorMessage;
			}

			return null;
		}

		private static void DeleteFiles(List<FileContent> filesToDelete, List<string> directoriesToDelete)
		{
			filesToDelete.ForEach(file => System.IO.File.Delete(file.Path));
			directoriesToDelete.ForEach(DeleteNotEmptyDirectory);
		}

		private static void DeleteNotEmptyDirectory(string dirPath)
		{
			var files = Directory.GetFiles(dirPath);
			var dirs = Directory.GetDirectories(dirPath);

			foreach (var file in files)
			{
				System.IO.File.SetAttributes(file, FileAttributes.Normal);
				System.IO.File.Delete(file);
			}

			foreach (string dir in dirs)
			{
				DeleteNotEmptyDirectory(dir);
			}

			Directory.Delete(dirPath, false);
		}

		private void DeleteEmptySubdirectories(string startLocation)
		{
			foreach (var directory in Directory.GetDirectories(startLocation))
			{
				DeleteEmptySubdirectories(directory);
				if (Directory.GetFiles(directory).Length == 0 && 
					Directory.GetDirectories(directory).Length == 0)
				{
					Directory.Delete(directory, false);
				}
			}
		}

		private RevertStructure GetRevertStructure(string courseId, List<string> filesToDeleteRelativePaths, bool isFull)
		{
			var staging = courseManager.GetStagingTempCourseFile(courseId);
			var courseDirectory = courseManager.GetExtractedCourseDirectory(courseId);
			var pathPrefix = courseDirectory.FullName;
			var filesInDirectoriesToDelete = GetFilesInDirectoriesToDelete(filesToDeleteRelativePaths, pathPrefix);
			filesToDeleteRelativePaths.AddRange(filesInDirectoriesToDelete);
			var zip = ZipFile.Read(staging.FullName, new ReadOptions { Encoding = Encoding.UTF8 });
			var filesToChangeRelativePaths = zip.Entries
				.Where(x => !x.IsDirectory)
				.Select(x => x.FileName)
				.Select(x => x.Replace('/', '\\'))
				.ToList();
			var courseFileRelativePaths = Directory
				.EnumerateFiles(courseDirectory.FullName, "*.*", SearchOption.AllDirectories)
				.Select(file => TrimPrefix(file, pathPrefix))
				.ToHashSet();
			var revertStructure = GetRevertStructure(pathPrefix, filesToDeleteRelativePaths, filesToChangeRelativePaths, courseFileRelativePaths, isFull);
			return revertStructure;
		}

		private static List<string> GetFilesInDirectoriesToDelete(List<string> filesToDeleteRelativePaths, string pathPrefix)
		{
			return filesToDeleteRelativePaths
				.Select(path => Path.Combine(pathPrefix, path))
				.Where(Directory.Exists)
				.SelectMany(dir => Directory
					.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories))
				.Select(path => TrimPrefix(path, pathPrefix))
				.ToList();
		}

		private static string TrimPrefix(string text, string prefix)
		{
			return text.Substring(text.IndexOf(prefix) + prefix.Length + 1);
		}

		private static RevertStructure GetRevertStructure(
			string pathPrefix,
			List<string> filesToDeleteRelativePaths,
			List<string> filesToChangeRelativePaths,
			HashSet<string> courseFileRelativePaths,
			bool isFull)
		{
			if (isFull)
			{
				filesToDeleteRelativePaths.Clear();
				filesToDeleteRelativePaths.AddRange(courseFileRelativePaths);
			}

			var deletedFiles = filesToDeleteRelativePaths
				.Where(courseFileRelativePaths.Contains)
				.Select(relativePath => Path.Combine(pathPrefix, relativePath))
				.Select(path => new FileContent(path, System.IO.File.ReadAllBytes(path)))
				.ToList();
			var deletedDirectories = GetDeletedDirs(filesToDeleteRelativePaths, pathPrefix);
			return new RevertStructure
			{
				FilesBeforeChanges = filesToChangeRelativePaths
					.Where(courseFileRelativePaths.Contains)
					.Select(path => Path.Combine(pathPrefix, path))
					.Select(path => new FileContent(path, System.IO.File.ReadAllBytes(path)))
					.ToList(),
				AddedFiles = filesToChangeRelativePaths
					.Where(file => !courseFileRelativePaths.Contains(file))
					.Select(path => Path.Combine(pathPrefix, path))
					.ToList(),
				DeletedFiles = deletedFiles,
				DeletedDirectories = deletedDirectories
			};
		}

		private static List<string> GetDeletedDirs(List<string> filesToDeleteRelativePaths, string pathPrefix)
		{
			return filesToDeleteRelativePaths.Select(path => Path.Combine(pathPrefix, path))
				.Where(path => Directory.Exists(path) &&
								path.StartsWith(pathPrefix) &&
								!path.Contains(".."))
				.ToList();
		}

		private void UploadChanges(string courseId, byte[] content)
		{
			log.Info($"Start upload course '{courseId}'");
			var stagingFile = courseManager.GetStagingTempCourseFile(courseId);
			System.IO.File.WriteAllBytes(stagingFile.FullName, content);
		}

		private static string GetTmpCourseId(string baseCourseId, string userId)
		{
			return $"{baseCourseId}_{userId}";
		}
	}

	internal struct FileContent
	{
		public FileContent(string path, byte[] content)
		{
			Path = path;
			Content = content;
		}

		public readonly string Path;
		public readonly byte[] Content;
	}

	internal class RevertStructure
	{
		public List<FileContent> FilesBeforeChanges = new List<FileContent>();
		public List<string> AddedFiles = new List<string>();
		public List<FileContent> DeletedFiles = new List<FileContent>();
		public List<string> DeletedDirectories = new List<string>();

		public void Revert()
		{
			DeletedFiles.ForEach(file => new FileInfo(file.Path).Directory.Create());
			static void WriteContent(FileContent fileContent)
			{
				var fInfo = new FileInfo(fileContent.Path);
				if (fInfo.Exists && fInfo.Attributes.HasFlag(FileAttributes.Hidden))
					fInfo.Attributes &= ~FileAttributes.Hidden; // WriteAllBytes кидает ошибку при записи в скрытый файл
				File.WriteAllBytes(fileContent.Path, fileContent.Content);
			}
			FilesBeforeChanges.ForEach(WriteContent);
			DeletedFiles.ForEach(WriteContent);
			AddedFiles.ForEach(File.Delete);
		}
	}
}