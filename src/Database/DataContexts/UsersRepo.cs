using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using EntityFramework.Functions;
using JetBrains.Annotations;
using Microsoft.AspNet.Identity;
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
		public List<UserRolesInfo> FilterUsers(UserSearchQueryModel query, UserManager<ApplicationUser> userManager, int limit=100)
		{
			var role = db.Roles.FirstOrDefault(r => r.Name == query.Role);
			var users = db.Users.Where(u => !u.IsDeleted);
			if (!string.IsNullOrEmpty(query.NamePrefix))
			{
				var usersIds = GetUsersByNamePrefix(query.NamePrefix).Select(u => u.Id);
				users = users.Where(u => usersIds.Contains(u.Id));
			}
			return users
				.FilterByRole(role, userManager)
				.FilterByUserIds(
					userRolesRepo.GetListOfUsersWithCourseRole(query.CourseRole, query.CourseId, query.IncludeHighCourseRoles),
					userRolesRepo.GetListOfUsersByPrivilege(query.OnlyPrivileged, query.CourseId)
				)
				.GetUserRolesInfo(limit, userManager);
		}

		public List<string> FilterUsersByNamePrefix(string namePrefix)
		{
			var deletedUserIds = db.Users.Where(u => u.IsDeleted).Select(u => u.Id).ToList();
			return GetUsersByNamePrefix(namePrefix).Where(u => !deletedUserIds.Contains(u.Id)).Select(u => u.Id).ToList();
		}

		/* Pass limit=0 to disable limiting */
		public List<UserRolesInfo> GetCourseInstructors(string courseId, UserManager<ApplicationUser> userManager, int limit = 50)
		{
			return db.Users
				.Where(u => ! u.IsDeleted)
				.FilterByUserIds(userRolesRepo.GetListOfUsersWithCourseRole(CourseRole.Instructor, courseId, includeHighRoles: true))
				.GetUserRolesInfo(limit, userManager);
		}

		/* Pass limit=0 to disable limiting */
		public List<UserRolesInfo> GetCourseAdmins(string courseId, UserManager<ApplicationUser> userManager, int limit = 50)
		{
			return db.Users
				.Where(u => ! u.IsDeleted)
				.FilterByUserIds(userRolesRepo.GetListOfUsersWithCourseRole(CourseRole.CourseAdmin, courseId, includeHighRoles: true))
				.GetUserRolesInfo(limit, userManager);
		}

		public List<string> GetSysAdminsIds(UserManager<ApplicationUser> userManager)
		{
			var role = db.Roles.FirstOrDefault(r => r.Name == LmsRoles.SysAdmin.ToString());
			if (role == null)
				return new List<string>();
			return db.Users.Where(u => !u.IsDeleted).FilterByRole(role, userManager).Select(u => u.Id).ToList();
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

		private const string nameSpace = nameof(UsersRepo);
		private const string dbo = nameof(dbo);

		[TableValuedFunction(nameof(GetUsersByNamePrefix), nameSpace, Schema = dbo)]
		// ReSharper disable once MemberCanBePrivate.Global
		public IQueryable<UserIdWrapper> GetUsersByNamePrefix(string name)
		{
			if (string.IsNullOrEmpty(name))
				return db.Users.Where(u => !u.IsDeleted).Select(u => new UserIdWrapper(u.Id));
			
			var splittedName = name.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			var nameQuery = string.Join(" & ", splittedName.Select(s => "\"" + s.Trim().Replace("\"", "\\\"") + "*\""));
			var nameParameter = new ObjectParameter("name", nameQuery);
			return db.ObjectContext().CreateQuery<UserIdWrapper>($"[{nameof(GetUsersByNamePrefix)}](@name)", nameParameter);
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
			if (! db.Users.Any(u => u.UserName == UlearnBotUsername))
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