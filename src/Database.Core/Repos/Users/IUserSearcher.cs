using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos.Users
{
	public interface IUserSearcher
	{
		Task<List<FoundUser>> SearchUsersAsync(UserSearchRequest request, bool strict = false, int offset = 0, int count = 50);
	}
}