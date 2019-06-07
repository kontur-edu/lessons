using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos.CourseRoles;
using Database.Repos.Groups;

namespace Database.Repos.Users.Search
{
	public class AccessRestrictor : IAccessRestrictor
	{
		private readonly IUsersRepo usersRepo;
		private readonly ICourseRolesRepo courseRolesRepo;
		private readonly IGroupAccessesRepo groupAccessesRepo;

		public AccessRestrictor(IUsersRepo usersRepo, ICourseRolesRepo courseRolesRepo, IGroupAccessesRepo groupAccessesRepo)
		{
			this.usersRepo = usersRepo;
			this.courseRolesRepo = courseRolesRepo;
			this.groupAccessesRepo = groupAccessesRepo;
		}

		public async Task<IQueryable<ApplicationUser>> RestrictUsersSetAsync(IQueryable<ApplicationUser> users, ApplicationUser currentUser, string courseId,
			bool hasSystemAdministratorAccess, bool hasCourseAdminAccess, bool hasInstructorAccessToGroupMembers, bool hasInstructorAccessToCourseInstructors)
		{
			if (hasSystemAdministratorAccess && usersRepo.IsSystemAdministrator(currentUser))
				return users;

			if (hasCourseAdminAccess && await courseRolesRepo.HasUserAccessToAnyCourseAsync(currentUser.Id, CourseRoleType.CourseAdmin).ConfigureAwait(false))
				return users;

			var userIds = new HashSet<string>();

			if (hasInstructorAccessToGroupMembers)
			{
				var groupsMembers = await groupAccessesRepo.GetMembersOfAllGroupsAvailableForUserAsync(currentUser.Id).ConfigureAwait(false);
				userIds.UnionWith(groupsMembers.Select(m => m.UserId));
			}

			if (hasInstructorAccessToCourseInstructors)
			{
				var courseInstructors =
					courseId != null
						? await courseRolesRepo.GetUsersWithRoleAsync(courseId, CourseRoleType.Instructor)
						: (await groupAccessesRepo.GetInstructorsOfAllGroupsAvailableForUserAsync(currentUser.Id).ConfigureAwait(false)).Select(u => u.Id);
				userIds.UnionWith(courseInstructors);
			}

			return users.Where(u => userIds.Contains(u.Id));
		}
	}
}