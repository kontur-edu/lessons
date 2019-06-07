using System.Linq;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos.Users.Search
{
	public class FilterByLmsRole : IFilter
	{
		private readonly IUsersRepo usersRepo;

		public FilterByLmsRole(IUsersRepo usersRepo)
		{
			this.usersRepo = usersRepo;
		}

		public async Task<IQueryable<ApplicationUser>> FilterAsync(IQueryable<ApplicationUser> users, UserSearchRequest request)
		{
			if (request.LmsRole == null)
				return users;

			var userIds = await usersRepo.GetUserIdsWithLmsRoleAsync(request.LmsRole.Value).ConfigureAwait(false);
			return users.Where(u => userIds.Contains(u.Id));
		}
	}
}