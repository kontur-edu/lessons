using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos.Users;
using Microsoft.EntityFrameworkCore;

namespace Database.Repos.CourseRoles
{
	/* This repo is fully migrated to .NET Core and EF Core */
	public class CourseRolesRepo : ICourseRolesRepo
	{
		private readonly UlearnDb db;
		private readonly IUsersRepo usersRepo;

		public CourseRolesRepo(UlearnDb db, IUsersRepo usersRepo)
		{
			this.db = db;
			this.usersRepo = usersRepo;
		}
			
		public Task<Dictionary<string, CourseRoleType>> GetRolesAsync(string userId)
		{
			return db.CourseRoles
				.Where(role => role.UserId == userId)
				.GroupBy(role => role.CourseId)
				.ToDictionaryAsync(g => g.Key, g => g.Select(role => role.Role).Min());
		}

		public async Task<bool> ToggleRoleAsync(string courseId, string userId, CourseRoleType roleType)
		{
			var role = await db.CourseRoles.FirstOrDefaultAsync(u => u.UserId == userId && u.Role == roleType && u.CourseId == courseId).ConfigureAwait(false);
			if (role == null)
				db.CourseRoles.Add(new CourseRole
				{
					UserId = userId,
					CourseId = courseId,
					Role = roleType
				});
			else
				db.CourseRoles.Remove(role);
			await db.SaveChangesAsync().ConfigureAwait(false);

			return role == null;
		}

		public async Task<bool> HasUserAccessToCourseAsync(string userId, string courseId, CourseRoleType minCourseRoleType)
		{
			var user = await usersRepo.FindUserByIdAsync(userId).ConfigureAwait(false);
			if (usersRepo.IsSystemAdministrator(user))
				return true;
			
			return await db.CourseRoles.Where(r => r.UserId == userId && r.CourseId == courseId && r.Role <= minCourseRoleType).AnyAsync().ConfigureAwait(false);
		}

		public async Task<bool> HasUserAccessToAnyCourseAsync(string userId, CourseRoleType minCourseRoleType)
		{
			var user = await usersRepo.FindUserByIdAsync(userId).ConfigureAwait(false);
			if (usersRepo.IsSystemAdministrator(user))
				return true;
			
			return await db.CourseRoles.Where(r => r.UserId == userId && r.Role <= minCourseRoleType).AnyAsync().ConfigureAwait(false);
		}

		public async Task<List<string>> GetCoursesWhereUserIsInRoleAsync(string userId, CourseRoleType minCourseRoleType)
		{
			var roles = await db.CourseRoles.Where(r => r.UserId == userId && r.Role <= minCourseRoleType).ToListAsync().ConfigureAwait(false);
			return roles.Select(r => r.CourseId).ToList();
		}

		public Task<List<string>> GetUsersWithRoleAsync(string courseId, CourseRoleType minCourseRoleType)
		{
			return db.CourseRoles.Where(r => r.CourseId == courseId && r.Role <= minCourseRoleType).Select(r => r.UserId).ToListAsync();
		}
	}
}