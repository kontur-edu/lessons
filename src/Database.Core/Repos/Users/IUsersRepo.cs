using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;

namespace Database.Repos.Users
{
	public interface IUsersRepo
	{
		[ItemCanBeNull]
		Task<ApplicationUser> FindUserByIdAsync(string userId);
		Task<List<UserRolesInfo>> FindUsers(UserSearchQuery query, int limit=100);
		List<string> FilterUsersByNamePrefix(string namePrefix);
		Task<List<UserRolesInfo>> GetCourseInstructorsAsync(string courseId, int limit = 50);
		Task<List<UserRolesInfo>> GetCourseAdminsAsync(string courseId, int limit = 50);
		Task<List<string>> GetSysAdminsIdsAsync();
		Task ChangeTelegram(string userId, long chatId, string chatTitle);
		Task ConfirmEmail(string userId, bool isConfirmed = true);
		Task UpdateLastConfirmationEmailTime(ApplicationUser user);
		Task ChangeEmail(ApplicationUser user, string email);
		ApplicationUser GetUlearnBotUser();
		string GetUlearnBotUserId();
		Task CreateUlearnBotUserIfNotExistsAsync();
		List<ApplicationUser> FindUsersByUsernameOrEmail(string usernameOrEmail);
		Task<List<ApplicationUser>> GetUsersByIdsAsync(IEnumerable<string> usersIds);
		Task DeleteUserAsync(ApplicationUser user);
		bool IsSystemAdministrator(ApplicationUser user);
		Task<List<string>> GetUserIdsWithLmsRoleAsync(LmsRoleType lmsRole);
		Task<List<string>> FindUsersBySocialProviderKeyAsync(string providerKey);
	}
}