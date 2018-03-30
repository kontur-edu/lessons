using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Database.Repos;
using log4net;
using uLearn;

namespace Database
{
	public class WebCourseManager : CourseManager
	{
		private readonly CoursesRepo coursesRepo;
		private static readonly ILog log = LogManager.GetLogger(typeof(WebCourseManager));

		private readonly Dictionary<string, Guid> loadedCourseVersions = new Dictionary<string, Guid>();
		private readonly ConcurrentDictionary<string, DateTime> courseVersionFetchTime = new ConcurrentDictionary<string, DateTime>();
		private readonly TimeSpan fetchCourseVersionEvery = TimeSpan.FromMinutes(1);

		public WebCourseManager(CoursesRepo coursesRepo)
			: base(GetCoursesDirectory())
		{
			this.coursesRepo = coursesRepo;
		}

		private readonly object @lock = new object();

		public override Course GetCourse(string courseId)
		{
			var course = base.GetCourse(courseId);
			if (IsCourseVersionWasUpdatedRecent(courseId))
				return course;

			courseVersionFetchTime[courseId] = DateTime.Now;
			
			var publishedVersion = coursesRepo.GetPublishedCourseVersion(courseId);

			if (publishedVersion == null)
				return course;

			lock (@lock)
			{
				if (loadedCourseVersions.TryGetValue(courseId, out var loadedVersionId)
					&& loadedVersionId != publishedVersion.Id)
				{
					log.Info($"Загруженная версия курса {courseId} отличается от актуальной. Обновляю курс.");
					course = ReloadCourse(courseId);
				}
				loadedCourseVersions[courseId] = publishedVersion.Id;
			}
			return course;
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
				loadedCourseVersions[courseId] = versionId;
			}
		}

		// public static readonly WebCourseManager Instance = new WebCourseManager(TODO);
	}
}