using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;
using Microsoft.AspNet.Identity;
using Npgsql;
using Ulearn.Common;
using Ulearn.Core;

namespace Database.DataContexts
{
	public class UsersRepo
	{
		private readonly ULearnDb db;
		private readonly UserRolesRepo userRolesRepo;

		public const string UlearnBotUsername = "ulearn-bot";

		public UsersRepo(ULearnDb db)
		{
			this.db = db;
			userRolesRepo = new UserRolesRepo(db);
		}

		public ApplicationUser FindUserById(string id)
		{
			var user = db.Users.Find(id);
			return user == null || user.IsDeleted ? null : user;
		}

		/* Pass limit=0 to disable limiting */
		public List<UserRolesInfo> FilterUsers(UserSearchQueryModel query, int limit = 100)
		{
			var usersIdsByNamePrefix = string.IsNullOrEmpty(query.NamePrefix)
				? null
				: GetUsersByNamePrefix(query.NamePrefix);
			return FilterUsers(query, null, usersIdsByNamePrefix, limit);
		}

		[ItemCanBeNull]
		public List<UserRolesInfo> FilterUsersByEmail(UserSearchQueryModel query, int limit = 100)
		{
			if (string.IsNullOrEmpty(query.NamePrefix) || !query.NamePrefix.Contains('@'))
				return null;
			var email = query.NamePrefix;
			var usersIdsByEmail = db.Users.Where(u => u.Email == email).Select(u => u.Id);
			return FilterUsers(query, usersIdsByEmail, null, limit);
		}

		private List<UserRolesInfo> FilterUsers(UserSearchQueryModel query, [CanBeNull]IQueryable<string> userIdsQuery, [CanBeNull]List<string> userIds, int limit)
		{
			var roles = db.Roles.ToList();
			var role = string.IsNullOrEmpty(query.Role) ? null : roles.FirstOrDefault(r => r.Name == query.Role);
			var users = db.Users.Include(u => u.Roles).Where(u => !u.IsDeleted);
			if (userIdsQuery != null)
				users = users.Where(u => userIdsQuery.Contains(u.Id));

			return users
				.FilterByRole(role)
				.FilterByUserIds(
					userRolesRepo.GetListOfUsersWithCourseRole(query.CourseRole, query.CourseId, query.IncludeHighCourseRoles),
					userRolesRepo.GetListOfUsersByPrivilege(query.OnlyPrivileged, query.CourseId),
					userIds
				)
				.GetUserRolesInfo(limit, roles);
		}

		public List<string> FilterUsersByNamePrefix(string namePrefix)
		{
			if (string.IsNullOrEmpty(namePrefix))
				return db.Users.Where(u => !u.IsDeleted).Select(u => u.Id).ToList();
			return GetUsersByNamePrefix(namePrefix).ToList();
		}

		/* Pass limit=0 to disable limiting */
		public List<UserRolesInfo> GetCourseInstructors(string courseId, UserManager<ApplicationUser> userManager, int limit = 50)
		{
			return db.Users
				.Where(u => !u.IsDeleted)
				.FilterByUserIds(userRolesRepo.GetListOfUsersWithCourseRole(CourseRole.Instructor, courseId, includeHighRoles: true))
				.GetUserRolesInfo(limit, db.Roles.ToList());
		}

		/* Pass limit=0 to disable limiting */
		public List<UserRolesInfo> GetCourseAdmins(string courseId, UserManager<ApplicationUser> userManager, int limit = 50)
		{
			return db.Users
				.Where(u => !u.IsDeleted)
				.FilterByUserIds(userRolesRepo.GetListOfUsersWithCourseRole(CourseRole.CourseAdmin, courseId, includeHighRoles: true))
				.GetUserRolesInfo(limit, db.Roles.ToList());
		}

		public List<string> GetSysAdminsIds()
		{
			var role = db.Roles.FirstOrDefault(r => r.Name == LmsRoles.SysAdmin.ToString());
			if (role == null)
				return new List<string>();
			return db.Users.Where(u => !u.IsDeleted).FilterByRole(role).Select(u => u.Id).ToList();
		}

		public Task ChangeTelegram(string userId, long chatId, string chatTitle)
		{
			var user = FindUserById(userId);
			if (user == null)
				return Task.CompletedTask;

			user.TelegramChatId = chatId;
			user.TelegramChatTitle = chatTitle;
			return db.SaveChangesAsync();
		}

		public Task ConfirmEmail(string userId, bool isConfirmed = true)
		{
			var user = FindUserById(userId);
			if (user == null)
				return Task.CompletedTask;

			user.EmailConfirmed = isConfirmed;
			return db.SaveChangesAsync();
		}

		private static Regex nonWordChars = new Regex(@"[^\w\s\-]*", RegexOptions.Compiled);
		private List<string> GetUsersByNamePrefix(string name, int limit = 100)
		{
			var escapedName = nonWordChars.Replace(name, "").Trim();
			var sql = 
$@"SELECT ""Id""
FROM ""AspNetUsers""
WHERE ""IsDeleted"" = False and ""Names"" ~* @query
LIMIT {limit};"; // ~* - регвыр ignoreCase
			var userIds = db.Database.SqlQuery<string>(
				sql,
				new NpgsqlParameter<string>("@query", $@"(^|\s){escapedName}")
			).ToList();
			return userIds;
		}

		public async Task UpdateLastConfirmationEmailTime(ApplicationUser user)
		{
			user.LastConfirmationEmailTime = DateTime.Now;
			await db.SaveChangesAsync();
		}

		public async Task ChangeEmail(ApplicationUser user, string email)
		{
			user.Email = email;
			user.EmailConfirmed = false;
			await db.SaveChangesAsync();
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

		public void CreateUlearnBotUserIfNotExists()
		{
			if (!db.Users.Any(u => u.UserName == UlearnBotUsername))
			{
				var user = new ApplicationUser
				{
					UserName = UlearnBotUsername,
					FirstName = "Ulearn",
					LastName = "bot",
					Email = "support@ulearn.me",
				};
				var userManager = new ULearnUserManager(db);
				userManager.Create(user, StringUtils.GenerateSecureAlphanumericString(10));

				db.SaveChanges();
			}
		}

		public List<ApplicationUser> FindUsersByUsernameOrEmail(string usernameOrEmail)
		{
			return db.Users.Where(u => u.UserName == usernameOrEmail || u.Email == usernameOrEmail).ToList();
		}

		public List<ApplicationUser> FindUsersByEmail(string email)
		{
			return db.Users.Where(u => u.Email == email).ToList();
		}

		public List<ApplicationUser> FindUsersByConfirmedEmail(string email)
		{
			return db.Users.Where(u => u.Email == email && u.EmailConfirmed).ToList();
		}

		public List<ApplicationUser> FindUsersByConfirmedEmails(IEnumerable<string> emails)
		{
			return db.Users.Where(u => emails.Contains(u.Email) && u.EmailConfirmed).ToList();
		}

		public IEnumerable<ApplicationUser> GetUsersByIds(IEnumerable<string> usersIds)
		{
			return db.Users.Where(u => usersIds.Contains(u.Id));
		}

		public async Task DeleteUserAsync(ApplicationUser user)
		{
			user.IsDeleted = true;
			/* Change name to make creating new user with same name possible */
			user.UserName = user.UserName + "__deleted__" + (new Random().Next(100000));
			await db.SaveChangesAsync();
		}
	}

	/* System.String is not available for table-valued functions so we need to create ComplexTyped wrapper */
}