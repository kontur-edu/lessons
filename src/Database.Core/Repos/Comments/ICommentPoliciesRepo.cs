using System.Threading.Tasks;
using Database.Models;
using Database.Models.Comments;

namespace Database.Repos.Comments
{
	public interface ICommentPoliciesRepo
	{
		Task<CommentsPolicy> GetCommentsPolicyAsync(string courseId);
		Task SaveCommentsPolicyAsync(CommentsPolicy policy);
	}
}