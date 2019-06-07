using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Database.Models;
using Database.Repos.Users;

namespace Database.Extensions
{
	public static class UserExtensions
	{
		private const string courseRoleClaimType = "CourseRole";
		
		/* TODO (andgein): Refactor code: just call CourseRolesRepo's methods */
		[Obsolete("Use CourseRolesRepo.HasUserAccessToCourseAsync() instead")]
		public static bool HasAccessFor(this IPrincipal principal, string courseId, CourseRoleType minAccessLevel)
		{
			if (principal.IsSystemAdministrator())
				return true;

			var courseRole = principal.GetAllRoles().FirstOrDefault(t => string.Equals(t.Item1, courseId, StringComparison.OrdinalIgnoreCase));

			return courseRole?.Item2 <= minAccessLevel;
		}

		[Obsolete("Use CourseRolesRepo.HasUserAccessToAnyCourseAsync() instead")]
		public static bool HasAccess(this IPrincipal principal, CourseRoleType minAccessLevel)
		{
			if (principal.IsSystemAdministrator())
				return true;

			var roles = principal.GetAllRoles().Select(t => t.Item2).ToList();

			if (!roles.Any())
				return false;
			return roles.Min() <= minAccessLevel;
		}

		private static IEnumerable<Tuple<string, CourseRoleType>> GetAllRoles(this IPrincipal principal)
		{
			var roleTuples = principal
				.ToClaimsPrincipal()
				.FindAll(courseRoleClaimType)
				.Select(claim => claim.Value.Split())
				.Select(s => Tuple.Create(s[0], s[1]));
			foreach (var roleTuple in roleTuples)
			{
				if (!Enum.TryParse(roleTuple.Item2, true, out CourseRoleType role))
					continue;
				yield return Tuple.Create(roleTuple.Item1, role);
			}
		}

		[Obsolete]
		public static IEnumerable<string> GetCoursesIdFor(this IPrincipal principal, CourseRoleType roleType)
		{
			return principal.GetAllRoles().Where(t => t.Item2 <= roleType).Select(t => t.Item1);
		}

		private static ClaimsPrincipal ToClaimsPrincipal(this IPrincipal principal)
		{
			return principal as ClaimsPrincipal ?? new ClaimsPrincipal(principal);
		}

		[Obsolete]
		public static bool IsSystemAdministrator(this IPrincipal principal)
		{
			return principal.IsInRole(LmsRoleType.SysAdmin.ToString());
		}

		[Obsolete]
		public static void AddCourseRoles(this ClaimsIdentity identity, Dictionary<string, CourseRoleType> roles)
		{
			foreach (var role in roles)
				identity.AddCourseRole(role.Key, role.Value);
		}

		private static void AddCourseRole(this ClaimsIdentity identity, string courseId, CourseRoleType roleType)
		{
			identity.AddClaim(new Claim(courseRoleClaimType, courseId + " " + roleType));
		}

		/*
		public static async Task<ClaimsIdentity> GenerateUserIdentityAsync(this ApplicationUser user, UserManager<ApplicationUser> manager, UserRolesRepo userRoles)
		{
			var identity = await manager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
			identity.AddCourseRoles(userRoles.GetRoles(user.Id));
			return identity;
		}

		public static async Task<ClaimsIdentity> GenerateUserIdentityAsync(this ApplicationUser user, UserManager<ApplicationUser> manager)
		{
			var userRoles = new UserRolesRepo();
			return await user.GenerateUserIdentityAsync(manager, userRoles);
		}
		*/

		/*
		public static bool HasSystemAccess(this ApplicationUser user, SystemAccessType accessType)
		{
			var systemAccessesRepo = new SystemAccessesRepo();
			return systemAccessesRepo.HasSystemAccess(user.Id, accessType);
		}
		
		public static bool HasSystemAccess(this ClaimsPrincipal User, SystemAccessType accessType)
		{
			var systemAccessesRepo = new SystemAccessesRepo();
			return systemAccessesRepo.HasSystemAccess(User.GetUserId(), accessType);
		}
		*/
		
		public static bool IsUlearnBot(this ApplicationUser user)
		{
			return user.UserName == UsersRepo.UlearnBotUsername;
		}
	}
}