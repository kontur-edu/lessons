using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using uLearn.Web.Models;

namespace uLearn.Web.DataContexts
{
	public static class AppUserQueryExtentions
	{
		public static IQueryable<ApplicationUser> FilterByName(this IQueryable<ApplicationUser> applicationUsers, string namePrefix)
		{
			return String.IsNullOrEmpty(namePrefix)
				? applicationUsers
				: applicationUsers.Where(u => u.UserName.StartsWith(namePrefix));
		}

		public static IQueryable<ApplicationUser> FilterByRole(this IQueryable<ApplicationUser> applicationUsers, IdentityRole role, UserManager<ApplicationUser> userManager)
		{
			
			return role == null
				? applicationUsers
				: applicationUsers.Where(u => u.Roles.Any(r => r.RoleId == role.Id));
		}

		public static IQueryable<ApplicationUser> FilterByUserIds(this IQueryable<ApplicationUser> applicationUsers, params List<string>[] idLists)
		{
			HashSet<string> goodIds = null;
			foreach (var idList in idLists.Where(idList => idList != null))
			{
				if (goodIds == null)
					goodIds = new HashSet<string>(idList);
				else
					goodIds.IntersectWith(idList);
			}

			return goodIds == null
				? applicationUsers
				: applicationUsers.Where(user => goodIds.Contains(user.Id));
		}

		public static List<UserRolesInfo> GetUserRolesInfo(this IQueryable<ApplicationUser> applicationUsers, int count, UserManager<ApplicationUser> userManager)
		{
			return applicationUsers
				.OrderBy(u => u.UserName)
				.Take(count).ToList()
				.Select(user => new UserRolesInfo
				{
					UserId = user.Id,
					UserName = user.UserName,
					GroupName = user.GroupName,
					Roles = userManager.GetRoles(user.Id).ToList()
				}).ToList();
		}

	}
}