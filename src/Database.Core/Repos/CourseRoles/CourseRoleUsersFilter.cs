using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Database.Repos.CourseRoles
{
	public class CourseRoleUsersFilter : ICourseRoleUsersFilter
	{
		private readonly UlearnDb db;

		public CourseRoleUsersFilter(UlearnDb db)
		{
			this.db = db;
		}

		public Task<List<string>> GetListOfUsersWithCourseRoleAsync(CourseRoleType? courseRoleType, string courseId, bool includeHighRoles=false)
		{
			if (!courseRoleType.HasValue)
				return null;

			var courseRoles = db.CourseRoles.AsQueryable();
			courseRoles = includeHighRoles
				? courseRoles.Where(userRole => userRole.Role <= courseRoleType)
				: courseRoles.Where(userRole => userRole.Role == courseRoleType);

			if (!string.IsNullOrEmpty(courseId))
				courseRoles = courseRoles.Where(userRole => userRole.CourseId == courseId);
			return courseRoles.Select(user => user.UserId).Distinct().ToListAsync();
		}
		
		public Task<List<string>> GetListOfUsersByPrivilegeAsync(bool onlyPrivileged, string courseId)
		{
			if (!onlyPrivileged)
				return null;

			var courseRoles = db.CourseRoles.AsQueryable();
			if (courseId != null)
				courseRoles = courseRoles.Where(userRole => userRole.CourseId == courseId);
			return courseRoles.Select(userRole => userRole.UserId).Distinct().ToListAsync();
		}
	}
}