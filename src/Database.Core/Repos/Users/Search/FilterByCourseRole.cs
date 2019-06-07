using System;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos.CourseRoles;

namespace Database.Repos.Users.Search
{
	public class FilterByCourseRole : IFilter
	{
		private readonly ICourseRolesRepo courseRolesRepo;

		public FilterByCourseRole(ICourseRolesRepo courseRolesRepo)
		{
			this.courseRolesRepo = courseRolesRepo;
		}

		public async Task<IQueryable<ApplicationUser>> FilterAsync(IQueryable<ApplicationUser> users, UserSearchRequest request)
		{
			if (string.IsNullOrEmpty(request.CourseId) || !request.MinCourseRoleType.HasValue)
				return users;
			
			if (request.MinCourseRoleType == CourseRoleType.Student)
				throw new ArgumentException($"Can't search by students, sorry: there are too many students");

			var userIds = await courseRolesRepo.GetUsersWithRoleAsync(request.CourseId, request.MinCourseRoleType.Value).ConfigureAwait(false);
			return users.Where(u => userIds.Contains(u.Id));
		}
	}
}