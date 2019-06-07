using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos.CourseRoles;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common;
using Ulearn.Core;

namespace Database.Repos.Users
{
	/* TODO (andgein): This repo is not fully migrated to .NET Core and EF Core */
	public class UsersRepo : IUsersRepo
	{
		private readonly UlearnDb db;
		private readonly UlearnUserManager userManager;
		private readonly ICourseRoleUsersFilter courseRoleUsersFilter;
		private IdentityRole sysAdminRole = null;

		public const string UlearnBotUsername = "ulearn-bot";

		public UsersRepo(UlearnDb db, UlearnUserManager userManager, ICourseRoleUsersFilter courseRoleUsersFilter)
		{
			this.db = db;
			this.userManager = userManager;
			this.courseRoleUsersFilter = courseRoleUsersFilter;
		}

		public async Task<ApplicationUser> FindUserByIdAsync(string userId)
		{
			var user = await db.Users.FindAsync(userId).ConfigureAwait(false);
			if (user == null)
				return null;
			
			return user.IsDeleted ? null : user;
		}

		/* Pass limit=0 to disable limiting */
		[Obsolete("Use UserSearcher instead")]
		public async Task<List<UserRolesInfo>> FindUsers(UserSearchQuery query, int limit=100)
		{
			var role = db.Roles.FirstOrDefault(r => r.Name == query.Role);
			var users = db.Users.Where(u => !u.IsDeleted);
			if (!string.IsNullOrEmpty(query.NamePrefix))
			{
				var usersIds = GetUsersByNamePrefix(query.NamePrefix).Select(u => u.Id);
				users = users.Where(u => usersIds.Contains(u.Id));
			}
			return await users
				.FilterByRole(role, userManager)
				.FilterByUserIds(
					await courseRoleUsersFilter.GetListOfUsersWithCourseRoleAsync(query.CourseRoleType, query.CourseId, query.IncludeHighCourseRoles).ConfigureAwait(false),
					await courseRoleUsersFilter.GetListOfUsersByPrivilegeAsync(query.OnlyPrivileged, query.CourseId).ConfigureAwait(false)
				)
				.GetUserRolesInfoAsync(limit, userManager).ConfigureAwait(false);
		}

		[Obsolete("Use UserSearcher instead")]
		public List<string> FilterUsersByNamePrefix(string namePrefix)
		{
			var deletedUserIds = db.Users.Where(u => u.IsDeleted).Select(u => u.Id).ToList();
			return GetUsersByNamePrefix(namePrefix).Where(u => !deletedUserIds.Contains(u.Id)).Select(u => u.Id).ToList();
		}
		
		/* Pass limit=0 to disable limiting */
		public async Task<List<UserRolesInfo>> GetCourseInstructorsAsync(string courseId, int limit = 50)
		{
			return await db.Users
				.Where(u => !u.IsDeleted)
				.FilterByUserIds(await courseRoleUsersFilter.GetListOfUsersWithCourseRoleAsync(CourseRoleType.Instructor, courseId, includeHighRoles: true).ConfigureAwait(false))
				.GetUserRolesInfoAsync(limit, userManager)
				.ConfigureAwait(false);
		}

		/* Pass limit=0 to disable limiting */
		public async Task<List<UserRolesInfo>> GetCourseAdminsAsync(string courseId, int limit = 50)
		{
			return await db.Users
				.Where(u => !u.IsDeleted)	
				.FilterByUserIds(await courseRoleUsersFilter.GetListOfUsersWithCourseRoleAsync(CourseRoleType.CourseAdmin, courseId, includeHighRoles: true).ConfigureAwait(false))
				.GetUserRolesInfoAsync(limit, userManager)
				.ConfigureAwait(false);
		}

		public Task<List<string>> FindUsersBySocialProviderKeyAsync(string providerKey)
		{
			return db.UserLogins.Where(login => login.ProviderKey == providerKey).Select(login => login.UserId).Distinct().ToListAsync();
		}

		public async Task<List<string>> GetUserIdsWithLmsRoleAsync(LmsRoleType lmsRole)
		{
			var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == lmsRole.ToString()).ConfigureAwait(false);
			if (role == null)
				return new List<string>();
			return db.Users.Where(u => ! u.IsDeleted).FilterByRole(role, userManager).Select(u => u.Id).ToList();
		}
		
		public Task<List<string>> GetSysAdminsIdsAsync()
		{
			return GetUserIdsWithLmsRoleAsync(LmsRoleType.SysAdmin);
		}

		public async Task ChangeTelegram(string userId, long chatId, string chatTitle)
		{
			var user = await FindUserByIdAsync(userId).ConfigureAwait(false);
			if (user == null)
				return;

			user.TelegramChatId = chatId;
			user.TelegramChatTitle = chatTitle;
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task ConfirmEmail(string userId, bool isConfirmed = true)
		{
			var user = await FindUserByIdAsync(userId).ConfigureAwait(false);
			if (user == null)
				return;

			user.EmailConfirmed = isConfirmed;
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		[Obsolete("Use UserSearcher instead")]
		private IQueryable<UserIdWrapper> GetUsersByNamePrefix(string name)
		{
			if (string.IsNullOrEmpty(name))
				return db.Users.Where(u => ! u.IsDeleted).Select(u => new UserIdWrapper(u.Id));
			
			var splittedName = name.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			var nameQuery = string.Join(" & ", splittedName.Select(s => "\"" + s.Trim().Replace("\"", "\\\"") + "*\""));
			return db.Users
				.FromSql("SELECT * FROM dbo.AspNetUsers WHERE IsDeleted = 0 AND CONTAINS([Names], {0})", nameQuery)
				.Select(u => new UserIdWrapper(u.Id));
		}

		public Task UpdateLastConfirmationEmailTime(ApplicationUser user)
		{
			user.LastConfirmationEmailTime = DateTime.Now;
			return db.SaveChangesAsync();
		}

		public Task ChangeEmail(ApplicationUser user, string email)
		{
			user.Email = email;
			user.EmailConfirmed = false;
			return db.SaveChangesAsync();
		}

		[NotNull]
		public ApplicationUser GetUlearnBotUser()
		{
			var user = db.Users.FirstOrDefault(u => u.UserName == UlearnBotUsername);
			if (user == null)
				throw new NotFoundException($"Ulearn bot user (username = {UlearnBotUsername}) not found");
			return user;
		}

		public string GetUlearnBotUserId()
		{
			var user = GetUlearnBotUser();
			return user.Id;
		}

		public async Task CreateUlearnBotUserIfNotExistsAsync()
		{
			var ulearnBotFound = await db.Users.AnyAsync(u => u.UserName == UlearnBotUsername).ConfigureAwait(false);
			if (! ulearnBotFound)
			{
				var user = new ApplicationUser
				{
					UserName = UlearnBotUsername,
					FirstName = "Ulearn",
					LastName = "bot",
					Email = "support@ulearn.me",
				};
				await userManager.CreateAsync(user, StringUtils.GenerateSecureAlphanumericString(10)).ConfigureAwait(false);

				await db.SaveChangesAsync().ConfigureAwait(false);
			}
		}

		public List<ApplicationUser> FindUsersByUsernameOrEmail(string usernameOrEmail)
		{
			return db.Users.Where(u => (u.UserName == usernameOrEmail || u.Email == usernameOrEmail) && ! u.IsDeleted).ToList();
		}

		public Task<List<ApplicationUser>> GetUsersByIdsAsync(IEnumerable<string> usersIds)
		{
			return db.Users.Where(u => usersIds.Contains(u.Id) && ! u.IsDeleted).ToListAsync();
		}
		
		public Task DeleteUserAsync(ApplicationUser user)
		{
			user.IsDeleted = true;
			/* Change name to make creating new user with same name possible */
			user.UserName = user.UserName + "__deleted__" + (new Random().Next(100000));
			return db.SaveChangesAsync();
		}

		public bool IsSystemAdministrator(ApplicationUser user)
		{
			if (user?.Roles == null)
				return false;
			
			if (sysAdminRole == null)
				sysAdminRole = db.Roles.First(r => r.Name == LmsRoleType.SysAdmin.ToString());	
			
			return user.Roles.Any(role => role.RoleId == sysAdminRole.Id);
		}
	}

	/* System.String is not available for table-valued functions so we need to create ComplexTyped wrapper */

	[ComplexType]
	public class UserIdWrapper
	{
		public UserIdWrapper(string userId)
		{
			Id = userId;
		}

		public string Id { get; set; }
	}
}