using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Database.DataContexts;
using log4net;
using Ulearn.Core;
using Ulearn.Core.Courses;

namespace Database
{
	public class WebCourseManager : CourseManager
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(WebCourseManager));
		public static readonly WebCourseManager Instance = new WebCourseManager();

		private readonly Dictionary<string, Guid> loadedCourseVersions = new Dictionary<string, Guid>();
		private readonly ConcurrentDictionary<string, DateTime> courseVersionFetchTime = new ConcurrentDictionary<string, DateTime>();
		private readonly TimeSpan fetchCourseVersionEvery = TimeSpan.FromMinutes(1);

		private WebCourseManager()
			: base(GetCoursesDirectory())
		{
		}

		private readonly object @lock = new object();

		public override Course GetCourse(string courseId)
		{
			Course course;
			try
			{
				course = base.GetCourse(courseId);
			}
			catch (Exception e) when (e is KeyNotFoundException || e is CourseNotFoundException)
			{
				course = null;
			}
			if (IsCourseVersionWasUpdatedRecent(courseId))
				return course ?? throw new KeyNotFoundException($"Key {courseId} not found");

			courseVersionFetchTime[courseId] = DateTime.Now;
			var coursesRepo = new CoursesRepo();
			var publishedVersion = coursesRepo.GetPublishedCourseVersion(courseId);

			if (publishedVersion == null)
				return course ?? throw new KeyNotFoundException($"Key {courseId} not found");

			lock (@lock)
			{
				if (loadedCourseVersions.TryGetValue(courseId.ToLower(), out var loadedVersionId)
					&& loadedVersionId != publishedVersion.Id)
				{
					log.Info($"Загруженная версия курса {courseId} отличается от актуальной ({loadedVersionId.ToString()} != {publishedVersion.Id}). Обновляю курс.");
					course = ReloadCourse(courseId);
				}
				loadedCourseVersions[courseId.ToLower()] = publishedVersion.Id;
			}
			return course ?? throw new KeyNotFoundException($"Key {courseId} not found");
		}

		private bool IsCourseVersionWasUpdatedRecent(string courseId)
		{
			if (courseVersionFetchTime.TryGetValue(courseId, out var lastFetchTime))
				return lastFetchTime > DateTime.Now.Subtract(fetchCourseVersionEvery);
			return false;
		}

		public void UpdateCourseVersion(string courseId, Guid versionId)
		{
			lock (@lock)
			{
				loadedCourseVersions[courseId.ToLower()] = versionId;
			}
		}
		
		protected override void LoadCourseZipsToDiskFromExternalStorage(IEnumerable<string> existingOnDiskCourseIds)
		{
			log.Info($"Загружаю курсы из БД");
			var coursesRepo = new CoursesRepo();
			var files = coursesRepo.GetCourseFiles(existingOnDiskCourseIds);
			foreach (var zipFile in files)
			{
				try
				{
					var stagingCourseFile = GetStagingCourseFile(zipFile.CourseId);
					File.WriteAllBytes(stagingCourseFile.FullName, zipFile.File);
					var versionCourseFile = GetCourseVersionFile(zipFile.CourseVersionId);
					if (!versionCourseFile.Exists)
						File.WriteAllBytes(versionCourseFile.FullName, zipFile.File);
				}
				catch(Exception ex)
				{
					log.Error($"Не смог загрузить {zipFile.CourseId} из базы данных", ex);
				}
			}
		}
	}
}