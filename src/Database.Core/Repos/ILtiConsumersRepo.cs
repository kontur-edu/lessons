using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos
{
	public interface ILtiConsumersRepo
	{
		Task<LtiConsumer> FindAsync(string consumerKey);
	}
}