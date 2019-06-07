using System.Threading.Tasks;

namespace Database.Repos
{
	public interface IRestoreRequestRepo
	{
		Task<string> CreateRequest(string userId);
		string FindUserId(string requestId);
		Task DeleteRequest(string requestId);
		bool ContainsRequest(string requestId);
	}
}