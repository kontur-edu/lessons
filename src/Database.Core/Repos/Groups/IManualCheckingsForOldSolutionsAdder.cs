using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repos.Groups
{
	public interface IManualCheckingsForOldSolutionsAdder
	{
		Task AddManualCheckingsForOldSolutionsAsync(string courseId, IEnumerable<string> usersIds);
		Task AddManualCheckingsForOldSolutionsAsync(string courseId, string userId);
	}
}