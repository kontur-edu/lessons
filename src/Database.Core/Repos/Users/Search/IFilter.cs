using System.Linq;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos.Users.Search
{
	public interface IFilter
	{
		Task<IQueryable<ApplicationUser>> FilterAsync(IQueryable<ApplicationUser> users, UserSearchRequest request);
	}
}