using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using uLearn.Web.Models;

namespace uLearn.Web.DataContexts
{
	public class UsersRepo
	{
		private readonly ULearnDb db;
		private UserRolesRepo userRolesRepo;

		public UsersRepo() : this(new ULearnDb())
		{
			
		}

		public UsersRepo(ULearnDb db)
		{
			this.db = db;
			userRolesRepo = new UserRolesRepo(db);
		}

		public List<UserRolesInfo> FilterUsers(UserSearchQueryModel query, UserManager<ApplicationUser> userManager)
		{
			var role = db.Roles.FirstOrDefault(r => r.Name == query.Role);
			return db.Users
				.FilterByName(query.NamePrefix)
				.FilterByRole(role, userManager)
				.FilterByUserIds(
					userRolesRepo.GetListOfUsersWithCourseRole(query.CourseRole, query.CourseId),
					userRolesRepo.GetListOfUsersByPrivilege(query.OnlyPrivileged, query.CourseId)
				)
				.GetUserRolesInfo(50, userManager);
		}
 
	}
}