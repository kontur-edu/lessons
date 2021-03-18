using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Vostok.Logging.Abstractions;
using Ulearn.Core;
using Ulearn.Core.Courses;

namespace Database
{
	public class WebCourseManager : CourseManager, IWebCourseManager
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(WebCourseManager));

		private readonly Dictionary<string, Guid> loadedCourseVersions = new Dictionary<string, Guid>();
		private readonly ConcurrentDictionary<string, DateTime> courseVersionFetchTime = new ConcurrentDictionary<string, DateTime>();
		private DateTime lastCoursesListFetchTime = DateTime.MinValue;
		private readonly TimeSpan fetchCourseVersionEvery = TimeSpan.FromSeconds(10);
		private readonly IServiceScopeFactory serviceScopeFactory;
		public event CourseChangedEventHandler CourseChangedEvent;

		public WebCourseManager(IServiceScopeFactory serviceScopeFactory)
			: base(GetCoursesDirectory())
		{
			this.serviceScopeFactory = serviceScopeFactory;
		}

		private readonly object @lock = new object();

		public override Course GetCourse(string courseId)
		{
			return GetCourseAsync(courseId).Result;
		}

		public async Task<Course> FindCourseAsync(string courseId)
		{
			try
			{
				return await GetCourseAsync(courseId);
			}
			catch (Exception e) when (e is KeyNotFoundException || e is CourseNotFoundException || e is CourseLoadingException)
			{
				return null;
			}
			catch (AggregateException e)
			{
				var ie = e.InnerException;
				if (ie is KeyNotFoundException || ie is CourseNotFoundException || ie is CourseLoadingException)
					return null;
				throw;
			}
		}

		///
		/// <exception cref="CourseLoadingException"></exception>
		///
		[NotNull]
		public async Task<Course> GetCourseAsync(string courseId)
		{
			Course course;
			try
			{
				course = base.GetCourse(courseId);
			}
			catch (Exception e) when (e is KeyNotFoundException || e is CourseNotFoundException || e is CourseLoadingException)
			{
				course = null;
			}
			catch (AggregateException e)
			{
				var ie = e.InnerException;
				if (ie is KeyNotFoundException || ie is CourseNotFoundException || ie is CourseLoadingException)
					course = null;
				else
					throw;
			}

			if (IsCourseVersionWasUpdatedRecent(courseId) || CourseIsBroken(courseId))
				return course ?? throw new CourseNotFoundException(courseId);

			courseVersionFetchTime[courseId] = DateTime.Now;

			using (var scope = serviceScopeFactory.CreateScope())
			{
				var coursesRepo = (CoursesRepo)scope.ServiceProvider.GetService(typeof(ICoursesRepo));
				var publishedVersion = await coursesRepo.GetPublishedCourseVersionAsync(courseId).ConfigureAwait(false);

				if (publishedVersion == null)
					return course ?? throw new CourseNotFoundException(courseId);

				ReloadCourseIfLoadedAndPublishedVersionsAreDifferent(courseId, publishedVersion);
				return base.GetCourse(courseId);
			}
		}

		public override IEnumerable<Course> GetCourses()
		{
			return GetCoursesAsync().Result;
		}

		public async Task<bool> HasCourseAsync(string courseId)
		{
			return await FindCourseAsync(courseId) != null;
		}

		public async Task<IEnumerable<Course>> GetCoursesAsync()
		{
			if (lastCoursesListFetchTime > DateTime.Now.Subtract(fetchCourseVersionEvery))
				return base.GetCourses();

			using (var scope = serviceScopeFactory.CreateScope())
			{
				var coursesRepo = (CoursesRepo)scope.ServiceProvider.GetService(typeof(ICoursesRepo));
				var tempCoursesRepo = (TempCoursesRepo)scope.ServiceProvider.GetService(typeof(ITempCoursesRepo));
				var publishedCourseVersions = await coursesRepo.GetPublishedCourseVersionsAsync().ConfigureAwait(false);

				lastCoursesListFetchTime = DateTime.Now;
				foreach (var courseVersion in publishedCourseVersions)
				{
					if (CourseIsBroken(courseVersion.CourseId))
						continue;
					try
					{
						ReloadCourseIfLoadedAndPublishedVersionsAreDifferent(courseVersion.CourseId, courseVersion);
					}
					catch (FileNotFoundException)
					{
						/* Sometimes zip-archive with course has been deleted already. It's strange but ok */
						log.Warn("Это странно, что я не смог загрузить с диска курс, который, если верить базе данных, был опубликован. Но ничего, просто проигнорирую");
					}
				}
				await LoadTempCoursesIfNotYetAsync(tempCoursesRepo);
				return base.GetCourses();
			}
		}

		private async Task LoadTempCoursesIfNotYetAsync(ITempCoursesRepo tempCoursesRepo)
		{
			var tempCourses = await tempCoursesRepo.GetTempCoursesAsync();
			tempCourses
				.Where(tempCourse => !HasCourse(tempCourse.CourseId))
				.ToList()
				.ForEach(course => TryReloadCourse(course.CourseId));
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

		public void NotifyCourseChanged(string courseId)
		{
			CourseChangedEvent?.Invoke(courseId);
		}

		private void ReloadCourseIfLoadedAndPublishedVersionsAreDifferent(string courseId, CourseVersion publishedVersion)
		{
			lock (@lock)
			{
				var isCourseLoaded = loadedCourseVersions.TryGetValue(courseId.ToLower(), out var loadedVersionId);
				if ((isCourseLoaded && loadedVersionId != publishedVersion.Id) || !isCourseLoaded)
				{
					var actual = isCourseLoaded ? loadedVersionId.ToString() : "<none>";
					log.Info($"Загруженная версия курса {courseId} отличается от актуальной ({actual} != {publishedVersion.Id}). Обновляю курс.");
					if (TryReloadCourse(courseId))
						NotifyCourseChanged(courseId);
				}

				loadedCourseVersions[courseId.ToLower()] = publishedVersion.Id;
			}
		}
	}
}