using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;

namespace Database.Repos.CourseRoles
{
	public interface ICourseRoleUsersFilter
	{
		Task<List<string>> GetListOfUsersWithCourseRoleAsync(CourseRoleType? courseRoleType, [CanBeNull]string courseId, bool includeHighRoles=false);
		Task<List<string>> GetListOfUsersByPrivilegeAsync(bool onlyPrivileged, string courseId);
	}
}