using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using uLearn.Web.DataContexts;
using uLearn.Web.Models;

namespace uLearn.Web
{
	public static class UserExtentions
	{
		private const string courseRoleClaimType = "CourseRole";

		public static bool HasAccessFor(this IPrincipal principal, string courseId, CourseRole minAccessLevel)
		{
			if (principal.IsSystemAdministrator())
				return true;

			var courseRole = principal.GetAllRoles().FirstOrDefault(t => t.Item1 == courseId);
			if (courseRole == null)
				return false;

			return courseRole.Item2 <= minAccessLevel;
		}

		public static bool HasAccess(this IPrincipal principal, CourseRole minAccessLevel)
		{
			if (principal.IsSystemAdministrator())
				return true;

			var roles = principal.GetAllRoles().Select(t => t.Item2).ToList();

			if (!roles.Any())
				return false;
			return roles.Min() <= minAccessLevel;
		}

		public static bool IsBestAccessFor(this IPrincipal principal, CourseRole role, string courseId)
		{
			if (principal.IsSystemAdministrator())
				return false;
			var bestRole = principal.GetAllRoles().FirstOrDefault(tuple => tuple.Item1 == courseId);
			if (bestRole == null)
				return false;
			return role == bestRole.Item2;
		}

		private static IEnumerable<Tuple<string, CourseRole>> GetAllRoles(this IPrincipal principal)
		{
			var roleTuples = principal
				.ToClaimsPrincipal()
				.FindAll(courseRoleClaimType)
				.Select(claim => claim.Value.Split())
				.Select(s => Tuple.Create(s[0], s[1]));
			foreach (var roleTuple in roleTuples)
			{
				CourseRole role;
				if (!Enum.TryParse(roleTuple.Item2, true, out role))
					continue;
				yield return Tuple.Create(roleTuple.Item1, role);
			}
		}

		public static IEnumerable<string> GetCoursesIdFor(this IPrincipal principal, CourseRole role)
		{
			return principal.GetAllRoles().Where(t => t.Item2 == role).Select(t => t.Item1);
		}

		private static ClaimsPrincipal ToClaimsPrincipal(this IPrincipal principal)
		{
			return principal as ClaimsPrincipal ?? new ClaimsPrincipal(principal);
		}

		public static IEnumerable<string> GetControllableCoursesId(this IPrincipal principal)
		{
			if (!principal.IsSystemAdministrator())
				return principal.GetCoursesIdFor(CourseRole.CourseAdmin);
			var courseManager = WebCourseManager.Instance;
			return courseManager.GetCourses().Select(course => course.Id);
		}

		public static bool IsSystemAdministrator(this IPrincipal principal)
		{
			return principal.IsInRole(LmsRoles.SysAdmin);
		}

		public static void AddCourseRoles(this ClaimsIdentity identity, Dictionary<string, CourseRole> roles)
		{
			foreach (var role in roles)
				identity.AddCourseRole(role.Key, role.Value);
		}

		private static void AddCourseRole(this ClaimsIdentity identity, string courseId, CourseRole role) 
		{
			identity.AddClaim(new Claim(courseRoleClaimType, courseId + " " + role));
		}

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
	}
}