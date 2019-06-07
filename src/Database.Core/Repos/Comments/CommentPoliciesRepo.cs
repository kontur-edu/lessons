using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Models.Comments;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace Database.Repos.Comments
{
	/* This class is fully migrated to .NET Core and EF Core */
	public class CommentPoliciesRepo : ICommentPoliciesRepo
	{
		private readonly UlearnDb db;

		public CommentPoliciesRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task<CommentsPolicy> GetCommentsPolicyAsync(string courseId)
		{
			var policy = await db.CommentsPolicies.FirstOrDefaultAsync(x => x.CourseId == courseId).ConfigureAwait(false);
			return policy ?? new CommentsPolicy
			{
				CourseId = courseId,
				IsCommentsEnabled = true,
				ModerationPolicy = CommentModerationPolicy.Postmoderation,
				OnlyInstructorsCanReply = false,
			};
		}

		public async Task SaveCommentsPolicyAsync(CommentsPolicy policy)
		{
			using (var transaction = db.Database.BeginTransaction())
			{
				await db.CommentsPolicies.Where(x => x.CourseId == policy.CourseId).DeleteAsync().ConfigureAwait(false);
				db.CommentsPolicies.Add(policy);
				await db.SaveChangesAsync().ConfigureAwait(false);
				
				transaction.Commit();
			}
		}
	}
}