using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Database.Repos;
using Ulearn.Core.Courses;

namespace Database
{
	public interface IWebCourseManager
	{
		Task<Course> GetCourseAsync(CoursesRepo coursesRepo, string courseId);
		Task<IEnumerable<Course>> GetCoursesAsync(ICoursesRepo coursesRepo);
		void UpdateCourseVersion(string courseId, Guid versionId);
		IEnumerable<Course> GetCourses();
		Course GetCourse(string courseId);
		Course FindCourse(string courseId);
		bool HasCourse(string courseId);
		Course GetVersion(Guid versionId);
		FileInfo GetStagingCourseFile(string courseId);
		DirectoryInfo GetExtractedCourseDirectory(string courseId);
		DirectoryInfo GetExtractedVersionDirectory(Guid versionId);
		FileInfo GetCourseVersionFile(Guid versionId);
		string GetStagingCoursePath(string courseId);
		Course LoadCourseFromZip(FileInfo zipFile);
		Course LoadCourseFromDirectory(DirectoryInfo dir);
		string GetPackageName(string courseId);
		string GetPackageName(Guid versionId);
		DateTime GetLastWriteTime(string courseId);
		bool TryCreateCourse(string courseId, string courseTitle, Guid firstVersionId);
		void EnsureVersionIsExtracted(Guid versionId);
		bool HasPackageFor(string courseId);
		Course FindCourseBySlideById(Guid slideId);
		void UpdateCourse(Course course);
		void LockCourse(string courseId);
		void ReleaseCourse(string courseId);
		void WaitWhileCourseIsLocked(string courseId);
		void MoveCourse(Course course, DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory);
	}
}