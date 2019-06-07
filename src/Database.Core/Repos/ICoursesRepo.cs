using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Database.Models;
using Ulearn.Common;

namespace Database.Repos
{
	public interface ICoursesRepo
	{
		Task<CourseVersion> GetPublishedCourseVersionAsync(string courseId);
		Task<List<CourseVersion>> GetCourseVersionsAsync(string courseId);
		Task<CourseVersion> AddCourseVersionAsync(string courseId, Guid versionId, string authorId);
		Task MarkCourseVersionAsPublishedAsync(Guid versionId);
		Task DeleteCourseVersionAsync(string courseId, Guid versionId);
		Task<List<CourseVersion>> GetPublishedCourseVersionsAsync();
		Task<CourseAccess> GrantAccessAsync(string courseId, string userId, CourseAccessType accessType, string grantedById);
		bool CanRevokeAccess(string courseId, string userId, IPrincipal revokedBy);
		Task<List<CourseAccess>> RevokeAccessAsync(string courseId, string userId, CourseAccessType accessType);
		Task<List<CourseAccess>> GetCourseAccessesAsync(string courseId);
		Task<List<CourseAccess>> GetCourseAccessesAsync(string courseId, string userId);
		Task<DefaultDictionary<string, List<CourseAccess>>> GetCoursesAccessesAsync(IEnumerable<string> coursesIds);
		Task<bool> HasCourseAccessAsync(string userId, string courseId, CourseAccessType accessType);
		Task<List<CourseAccess>> GetUserAccessesAsync(string userId);
		Task<List<string>> GetPublishedCourseIdsAsync();
		Task<List<string>> GetCoursesUserHasAccessTo(string userId, CourseAccessType accessType);
		Task AddCourseFile(string courseId, Guid versionId, byte[] content);
		Task<CourseFile> GetCourseFileAsync(string courseId);
		Task<List<CourseFile>> GetCourseFilesAsync(IEnumerable<string> exceptCourseIds);
	}
}