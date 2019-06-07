using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Ionic.Zip;
using JetBrains.Annotations;
using log4net;
using Telegram.Bot.Types.Enums;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Configuration;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Courses.Units;
using Ulearn.Core.Helpers;
using Ulearn.Core.Telegram;

namespace Ulearn.Core
{
	public class CourseManager
	{
		private const string examplePackageName = "Help";

		private static readonly ILog log = LogManager.GetLogger(typeof(CourseManager));

		private readonly DirectoryInfo stagedDirectory;
		private readonly DirectoryInfo coursesDirectory;
		private readonly DirectoryInfo coursesVersionsDirectory;

		private readonly ConcurrentDictionary<string, Course> courses = new ConcurrentDictionary<string, Course>(StringComparer.InvariantCultureIgnoreCase);

		/* LRU-cache for course versions. 50 is a capactiy of the cache. */
		private readonly LruCache<Guid, Course> versionsCache = new LruCache<Guid, Course>(50);

		private readonly ExerciseStudentZipsCache exerciseStudentZipsCache = new ExerciseStudentZipsCache();

		/* TODO (andgein): Use DI */
		private static readonly CourseLoader loader = new CourseLoader(new UnitLoader(new XmlSlideLoader()));
		private static readonly ErrorsBot errorsBot = new ErrorsBot();

		public CourseManager(DirectoryInfo baseDirectory)
			: this(
				baseDirectory.GetSubdirectory("Courses.Staging"),
				baseDirectory.GetSubdirectory("Courses.Versions"),
				baseDirectory.GetSubdirectory("Courses")
			)
		{
		}

		public CourseManager(DirectoryInfo stagedDirectory, DirectoryInfo coursesVersionsDirectory, DirectoryInfo coursesDirectory)
		{
			this.stagedDirectory = stagedDirectory;
			this.coursesDirectory = coursesDirectory;
			this.coursesVersionsDirectory = coursesVersionsDirectory;
		}

		public virtual IEnumerable<Course> GetCourses()
		{
			LoadCoursesIfNotYet();
			return courses.Values.OrderBy(c => c.Title, StringComparer.OrdinalIgnoreCase);
		}

		[NotNull]
		public virtual Course GetCourse(string courseId)
		{
			LoadCoursesIfNotYet();
			return courses.Get(courseId);
		}

		[CanBeNull]
		public Course FindCourse(string courseId)
		{
			try
			{
				return GetCourse(courseId);
			}
			catch (Exception e) when (e is KeyNotFoundException || e is CourseNotFoundException) 
			{
				return null;
			}
		}

		public bool HasCourse(string courseId)
		{
			return FindCourse(courseId) != null;
		}

		public Course GetVersion(Guid versionId)
		{
			if (versionsCache.TryGet(versionId, out var version))
				return version;

			var versionFile = GetCourseVersionFile(versionId);
			version = LoadCourseFromZip(versionFile);

			/* Add version to cache for fast loading next time */
			versionsCache.Add(versionId, version);
			return version;
		}

		public FileInfo GetStagingCourseFile(string courseId)
		{
			var packageName = GetPackageName(courseId);
			if (Path.GetInvalidFileNameChars().Any(packageName.Contains))
				throw new Exception(courseId);
			return stagedDirectory.GetFile(packageName);
		}

		public DirectoryInfo GetExtractedCourseDirectory(string courseId)
		{
			return coursesDirectory.GetSubdirectory(courseId);
		}

		public DirectoryInfo GetExtractedVersionDirectory(Guid versionId)
		{
			return GetExtractedCourseDirectory(versionId.GetNormalizedGuid());
		}

		public FileInfo GetCourseVersionFile(Guid versionId)
		{
			var packageName = GetPackageName(versionId);
			return coursesVersionsDirectory.GetFile(packageName);
		}

		public string GetStagingCoursePath(string courseId)
		{
			return GetStagingCourseFile(courseId).FullName;
		}

		private static readonly object reloadLock = new object();

		private void LoadCoursesIfNotYet()
		{
			Exception firstException = null;
			lock (reloadLock)
			{
				if (courses.Count != 0)
					return;
				var courseZips = stagedDirectory.GetFiles("*.zip");
				var courseIdsWithZips = courseZips.Select(z => GetCourseId(z.FullName));
				LoadCourseZipsToDiskFromExternalStorage(courseIdsWithZips);
				
				log.Info($"Загружаю курсы из {stagedDirectory.FullName}");
				courseZips = stagedDirectory.GetFiles("*.zip");
				foreach (var zipFile in courseZips)
				{
					log.Info($"Обновляю курс из {zipFile.Name}");
					try
					{
						var courseId = GetCourseId(zipFile.FullName);
						ReloadCourse(courseId);
					}
					catch (Exception e)
					{
						log.Error($"Не могу загрузить курс из {zipFile.FullName}", e);
						if (firstException == null)
							firstException = new Exception("Error loading course from " + zipFile.Name, e);
					}
				}
			}
			if (firstException != null)
				throw firstException;
		}

		protected virtual void LoadCourseZipsToDiskFromExternalStorage(IEnumerable<string> existingOnDiskCourseIds)
		{
			// NONE
		} 

		protected Course ReloadCourse(string courseId)
		{
			/* First try load course from directory */
			var courseDir = GetExtractedCourseDirectory(courseId);
			try
			{
				log.Info($"Сначала попробую загрузить уже распакованный курс из {courseDir.FullName}");
				return ReloadCourseFromDirectory(courseDir);
			}
			catch (Exception e)
			{
				log.Warn("Не смог загрузить курс из папки", e);
				var zipFile = GetStagingCourseFile(courseId);
				log.Info($"Буду загружать из zip-архива: {zipFile.FullName}");
				errorsBot.PostToChannel($"Не смог загрузить курс из папки {courseDir}, буду загружать из zip-архива {zipFile.FullName}:\n{e.Message.EscapeMarkdown()}\n```{e.StackTrace}```", ParseMode.Markdown);
				return ReloadCourseFromZip(zipFile);
			}
		}

		private Course ReloadCourseFromZip(FileInfo zipFile)
		{
			var course = LoadCourseFromZip(zipFile);
			courses[course.Id] = course;
			log.Info($"Курс {course.Id} загружен из {zipFile.FullName} и сохранён в памяти");
			if (exerciseStudentZipsCache.IsEnabled)
				exerciseStudentZipsCache.DeleteCourseZips(course.Id);
			return course;
		}

		private Course ReloadCourseFromDirectory(DirectoryInfo directory)
		{
			var course = LoadCourseFromDirectory(directory);
			courses[course.Id] = course;
			log.Info($"Курс {course.Id} загружен из {directory.FullName} и сохранён в памяти");
			if (exerciseStudentZipsCache.IsEnabled)
				exerciseStudentZipsCache.DeleteCourseZips(course.Id);
			return course;
		}

		private void UnzipFile(FileInfo zipFile, DirectoryInfo unpackDirectory)
		{
			using (var zip = ZipFile.Read(zipFile.FullName, new ReadOptions { Encoding = Encoding.GetEncoding(866) }))
			{
				unpackDirectory.ClearDirectory();
				zip.ExtractAll(unpackDirectory.FullName, ExtractExistingFileAction.OverwriteSilently);
			}
		}

		private DirectoryInfo UnzipCourseFile(FileInfo zipFile)
		{
			var courseOrVersionId = GetCourseId(zipFile.Name);
			var courseDir = coursesDirectory.CreateSubdirectory(courseOrVersionId);
			log.Info($"Распаковываю архив с курсом из {zipFile.FullName} в {courseDir.FullName}");
			UnzipFile(zipFile, courseDir);
			return courseDir;
		}

		public Course LoadCourseFromZip(FileInfo zipFile)
		{
			var courseDir = UnzipCourseFile(zipFile);
			return LoadCourseFromDirectory(courseDir);
		}

		public Course LoadCourseFromDirectory(DirectoryInfo dir)
		{
			WaitWhileCourseIsLocked(GetCourseId(dir.Name));
			return loader.Load(dir);
		}

		public static string GetCourseId(string packageName)
		{
			return Path.GetFileNameWithoutExtension(packageName);
		}

		public string GetPackageName(string courseId)
		{
			return courseId + ".zip";
		}

		public string GetPackageName(Guid versionId)
		{
			return versionId.GetNormalizedGuid() + ".zip";
		}

		public DateTime GetLastWriteTime(string courseId)
		{
			return stagedDirectory.GetFile(GetPackageName(courseId)).LastWriteTime;
		}

		public bool TryCreateCourse(string courseId, string courseTitle, Guid firstVersionId)
		{
			if (courseId.Any(GetInvalidCharacters().Contains))
				return false;

			var package = stagedDirectory.GetFile(GetPackageName(courseId));
			if (package.Exists)
				return true;

			var examplePackage = stagedDirectory.GetFile(GetPackageName(examplePackageName));
			if (!examplePackage.Exists)
				CreateEmptyCourse(courseId, courseTitle, package.FullName);
			else
				CreateCourseFromExample(courseId, courseTitle, package.FullName, examplePackage);

			ReloadCourseFromZip(package);

			var versionFile = GetCourseVersionFile(firstVersionId);
			File.Copy(package.FullName, versionFile.FullName);
			
			return true;
		}

		public void EnsureVersionIsExtracted(Guid versionId)
		{
			var versionDirectory = GetExtractedVersionDirectory(versionId);
			if (!versionDirectory.Exists)
			{
				Directory.CreateDirectory(versionDirectory.FullName);
				UnzipFile(GetCourseVersionFile(versionId), versionDirectory);
			}
		}

		private static void CreateEmptyCourse(string courseId, string courseTitle, string path)
		{
			using (var zip = new ZipFile(Encoding.GetEncoding(866)))
			{
				zip.AddEntry("course.xml",
					"<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n" +
					$"<course xmlns=\"https://ulearn.me/schema/v2\" title=\"{courseTitle.EncodeQuotes()}\">\n" +
					@"<units><add>*\unit.xml</add></units>" + 
					"</course>",
					Encoding.UTF8);
				zip.Save(path);
			}
		}

		private static void CreateCourseFromExample(string courseId, string courseTitle, string path, FileInfo examplePackage)
		{
			examplePackage.CopyTo(path, true);
			File.SetLastWriteTime(examplePackage.FullName, DateTime.Now);
			
			var nsResolver = new XmlNamespaceManager(new NameTable());
			nsResolver.AddNamespace("ulearn", "https://ulearn.me/schema/v2");
			using (var zip = ZipFile.Read(path, new ReadOptions { Encoding = Encoding.GetEncoding(866) }))
			{
				var courseXml = zip.Entries.FirstOrDefault(e => Path.GetFileName(e.FileName) == "course.xml" && !e.IsDirectory);
				if (courseXml != null)
					UpdateXmlAttribute(zip[courseXml.FileName], "//ulearn:course", "title", courseTitle, zip, nsResolver);
			}
		}

		private static void UpdateXmlElement(ZipEntry entry, string selector, string value, ZipFile zip, IXmlNamespaceResolver nsResolver)
		{
			UpdateXmlEntity(entry, selector, element => element.Value = value, zip, nsResolver);
		}

		private static void UpdateXmlAttribute(ZipEntry entry, string selector, string attribute, string value, ZipFile zip, IXmlNamespaceResolver nsResolver)
		{
			UpdateXmlEntity(entry, selector, element =>
			{
				var elementAttribute = element.Attribute(attribute);
				if (elementAttribute != null)
					elementAttribute.Value = value;
			}, zip, nsResolver);
		}

		private static void UpdateXmlEntity(ZipEntry entry, string selector, Action<XElement> update, ZipFile zip, IXmlNamespaceResolver nsResolver)
		{
			var output = new MemoryStream();
			using (var entryStream = entry.OpenReader())
			{
				var xml = XDocument.Load(entryStream);
				var element = xml.XPathSelectElement(selector, nsResolver);
				update(element.EnsureNotNull($"no element [{selector}] in zip entry {entry.FileName}"));
				xml.Save(output);
			}
			zip.UpdateEntry(entry.FileName, output.GetBuffer());
			zip.Save();
		}

		public bool HasPackageFor(string courseId)
		{
			return GetStagingCourseFile(courseId).Exists;
		}

		public static char[] GetInvalidCharacters()
		{
			return new[] { '&' }.Concat(Path.GetInvalidFileNameChars()).Concat(Path.GetInvalidPathChars()).Distinct().ToArray();
		}

		public Course FindCourseBySlideById(Guid slideId)
		{
			return GetCourses().FirstOrDefault(c => c.Slides.Count(s => s.Id == slideId) > 0);
		}

		public void UpdateCourse(Course course)
		{
			if (!courses.ContainsKey(course.Id))
				return;
			
			if (exerciseStudentZipsCache.IsEnabled)
				exerciseStudentZipsCache.DeleteCourseZips(course.Id);
			
			courses[course.Id] = course;
		}

		private readonly TimeSpan waitBetweenLockTries = TimeSpan.FromSeconds(0.1);
		private readonly TimeSpan lockLifeTime = TimeSpan.FromMinutes(20);
		private const int updateCourseEachOperationTriesCount = 5;

		private FileInfo GetCourseLockFile(string courseId)
		{
			return coursesDirectory.GetFile("~" + courseId + ".lock");
		}

		private static bool TryCreateLockFile(FileInfo lockFile)
		{
			var tempFileName = Path.GetTempFileName();
			try
			{
				if (!lockFile.Directory.Exists)
					lockFile.Directory.Create();
				new FileInfo(tempFileName).MoveTo(lockFile.FullName);
				return true;
			}
			catch (IOException)
			{
				return false;
			}
		}

		public void LockCourse(string courseId)
		{
			var lockFile = GetCourseLockFile(courseId);
			while (true)
			{
				if (TryCreateLockFile(lockFile))
					return;

				log.Info($"Курс {courseId} заблокирован, жду {waitBetweenLockTries.TotalSeconds} секунд");

				Thread.Sleep(waitBetweenLockTries);

				try
				{
					lockFile.Refresh();
					/* If lock-file has been created ago, just delete it and unzip course again */
					if (lockFile.Exists && lockFile.LastWriteTime < DateTime.Now.Subtract(lockLifeTime))
					{
						log.Info($"Курс {courseId} заблокирован слишком давно, снимаю блокировку");

						lockFile.Delete();
						UnzipCourseFile(GetStagingCourseFile(courseId));
					}
				}
				catch (IOException)
				{
				}
			}
		}

		public void ReleaseCourse(string courseId)
		{
			GetCourseLockFile(courseId).Delete();
		}

		public void WaitWhileCourseIsLocked(string courseId)
		{
			LockCourse(courseId);
			ReleaseCourse(courseId);
		}

		public void MoveCourse(Course course, DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory)
		{
			var tempDirectoryName = coursesDirectory.GetSubdirectory(Path.GetRandomFileName());
			LockCourse(course.Id);

			try
			{
				FuncUtils.TrySeveralTimes(() => Directory.Move(destinationDirectory.FullName, tempDirectoryName.FullName), updateCourseEachOperationTriesCount);

				try
				{
					FuncUtils.TrySeveralTimes(() => Directory.Move(sourceDirectory.FullName, destinationDirectory.FullName), updateCourseEachOperationTriesCount);
				}
				catch (IOException)
				{
					/* In case of any file system's error rollback previous operation */
					FuncUtils.TrySeveralTimes(() => Directory.Move(tempDirectoryName.FullName, destinationDirectory.FullName), updateCourseEachOperationTriesCount);
					throw;
				}
				FixFileReferencesInCourse(course, sourceDirectory, destinationDirectory);

				UpdateCourse(course);
			}
			finally
			{
				ReleaseCourse(course.Id);
			}
			FuncUtils.TrySeveralTimes(() => tempDirectoryName.ClearDirectory(true), updateCourseEachOperationTriesCount);
		}

		private void FixFileReferencesInCourse(Course course, DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory)
		{
			foreach (var instructorNote in course.Units.Select(u => u.InstructorNote).Where(n => n != null))
				instructorNote.File = (FileInfo) GetNewPathForFileAfterMoving(instructorNote.File, sourceDirectory, destinationDirectory);

			foreach (var slide in course.Slides)
			{
				slide.Info.SlideFile = (FileInfo) GetNewPathForFileAfterMoving(slide.Info.SlideFile, sourceDirectory, destinationDirectory);

				foreach (var exerciseBlock in slide.Blocks.OfType<CsProjectExerciseBlock>())
					exerciseBlock.SlideFolderPath = (DirectoryInfo) GetNewPathForFileAfterMoving(exerciseBlock.SlideFolderPath, sourceDirectory, destinationDirectory);

				slide.Meta?.FixPaths(slide.Info.SlideFile);
			}

			course.CourseXmlDirectory = (DirectoryInfo) GetNewPathForFileAfterMoving(course.CourseXmlDirectory, sourceDirectory, destinationDirectory);
		}

		private static FileSystemInfo GetNewPathForFileAfterMoving(FileSystemInfo file, DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory)
		{
			if (!file.IsInDirectory(sourceDirectory))
				return file;
			if (sourceDirectory.FullName == file.FullName)
				return destinationDirectory;

			var relativePath = file.GetRelativePath(sourceDirectory.FullName);
			var newPath = Path.Combine(destinationDirectory.FullName, relativePath);

			if (file is DirectoryInfo)
				return new DirectoryInfo(newPath);
			return new FileInfo(newPath);
		}

		public static DirectoryInfo GetCoursesDirectory()
		{
			// var coursesDirectory = ConfigurationManager.AppSettings["ulearn.coursesDirectory"];
			var coursesDirectory = ApplicationConfiguration.Read<UlearnConfiguration>().CoursesDirectory;
			if (string.IsNullOrEmpty(coursesDirectory))
				coursesDirectory = Utils.GetAppPath() + @"\..\Courses\";

			return new DirectoryInfo(coursesDirectory);
		}
	}
}