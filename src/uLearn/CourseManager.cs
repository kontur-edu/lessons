using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;

namespace uLearn
{
	public class CourseManager
	{
		private readonly DirectoryInfo coursesDirectory;
		private readonly Dictionary<string, Course> courses = new Dictionary<string, Course>(StringComparer.InvariantCultureIgnoreCase);
		private readonly CourseLoader loader = new CourseLoader();

		public CourseManager(DirectoryInfo baseDirectory)
			: this(
			baseDirectory.GetSubdir("Courses.Staging"),
			baseDirectory.GetSubdir("Courses"))
		{
		}

		public CourseManager(DirectoryInfo stagedDirectory, DirectoryInfo coursesDirectory)
		{
			StagedDirectory = stagedDirectory;
			this.coursesDirectory = coursesDirectory;
		}

		public DirectoryInfo StagedDirectory { get; private set; }

		public IEnumerable<Course> GetCourses()
		{
			LoadCoursesIfNotYet();
			return courses.Values;
		}

		public Course GetCourse(string courseId)
		{
			LoadCoursesIfNotYet();
			return courses.Get(courseId);
		}

		public IEnumerable<StagingPackage> GetStagingPackages()
		{
			return StagedDirectory.GetFiles("*.zip").Select(f => new StagingPackage(f.Name, f.LastWriteTime));
		}

		public string GetStagingCoursePath(string name)
		{
			var packageName = GetPackageName(name);
			if (Path.GetInvalidFileNameChars().Any(name.Contains)) 
				throw new Exception(name);
			return StagedDirectory.GetFile(name).FullName;
		}

		private static readonly object ReloadLock = new object();
		
		private void LoadCoursesIfNotYet()
		{
			lock (ReloadLock)
			{
				if (courses.Count != 0) return;
				var courseZips = StagedDirectory.GetFiles("*.zip");
				foreach (var zipFile in courseZips)
					try
					{
						ReloadCourseFromZip(zipFile);
					}
					catch
					{
						//throw new Exception("Error loading course from " + zipFile.Name, e);
					}
			}
		}

		public string ReloadCourse(string courseId)
		{
			var file = StagedDirectory.GetFile(GetPackageName(courseId));
			return ReloadCourseFromZip(file);
		}

		private string ReloadCourseFromZip(FileInfo zipFile)
		{
			string courseId = "";
			using (var zip = ZipFile.Read(zipFile.FullName, new ReadOptions {Encoding = Encoding.GetEncoding(866)}))
			{
				courseId = GetCourseId(zipFile.Name);
				var courseDir = coursesDirectory.CreateSubdirectory(courseId);
				Directory.Delete(courseDir.FullName, true);
				courseDir.Create();
				zip.ExtractAll(courseDir.FullName, ExtractExistingFileAction.OverwriteSilently);
				ReloadCourse(courseDir);
			}
			return courseId;
		}

		public void ReloadCourse(DirectoryInfo dir)
		{
			var course = loader.LoadCourse(dir);
			courses[course.Id] = course;
		}

		public static string GetCourseId(string packageName)
		{
			return Path.GetFileNameWithoutExtension(packageName);
		}

		public string GetPackageName(string courseId)
		{
			return courseId + ".zip";
		}

		public DateTime GetLastWriteTime(Course course)
		{
			return StagedDirectory.GetFile(GetPackageName(course.Id)).LastWriteTime;
		}
	}
}