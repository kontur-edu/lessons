using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;

namespace Database.DataContexts
{
	public class UserRolesRepo
	{
		private readonly ULearnDb db;

		public UserRolesRepo(ULearnDb db)
		{
			this.db = db;
		}

		public UserRolesRepo()
			: this(new ULearnDb())
		{
		}
			
		public Dictionary<string, CourseRole> GetRoles(string userId)
		{
			return db.UserRoles
				.Where(role => role.UserId == userId)
				.GroupBy(role => role.CourseId)
				.ToDictionary(g => g.Key, g => g.Select(role => role.Role).Min(), StringComparer.OrdinalIgnoreCase);
		}

		public async Task<bool> ToggleRole(string courseId, string userId, CourseRole role)
		{
			var userRole = db.UserRoles.FirstOrDefault(u => u.UserId == userId && u.Role == role && u.CourseId == courseId);
			if (userRole == null)
				db.UserRoles.Add(new UserRole
				{
					UserId = userId,
					CourseId = courseId,
					Role = role
				});
			else
				db.UserRoles.Remove(userRole);
			await db.SaveChangesAsync();

			return userRole == null;
		}

		public List<string> GetListOfUsersWithCourseRole(CourseRole? courseRole, string courseId, bool includeHighRoles = false)
		{
			if (!courseRole.HasValue)
				return null;

			var usersQuery = (IQueryable<UserRole>)db.UserRoles;
			usersQuery = includeHighRoles
				? usersQuery.Where(userRole => userRole.Role <= courseRole)
				: usersQuery.Where(userRole => userRole.Role == courseRole);

			if (!string.IsNullOrEmpty(courseId))
				usersQuery = usersQuery.Where(userRole => userRole.CourseId == courseId);
			return usersQuery.Select(user => user.UserId).Distinct().ToList();
		}

		public List<string> GetListOfUsersByPrivilege(bool onlyPrivileged, string courseId)
		{
			if (!onlyPrivileged)
				return null;

			IQueryable<UserRole> usersQuery = db.UserRoles;
			if (courseId != null)
				usersQuery = usersQuery.Where(userRole => userRole.CourseId == courseId);
			return usersQuery.Select(userRole => userRole.UserId).Distinct().ToList();
		}
	}
}