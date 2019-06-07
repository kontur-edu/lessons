using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;
using Database.Models.Comments;
using Ulearn.Common;

namespace Database.Repos.Comments
{
	public interface ICommentLikesRepo
	{
		Task LikeAsync(int commentId, string userId);
		Task UnlikeAsync(int commentId, string userId);

		Task<List<CommentLike>> GetLikesAsync(int commentId);
		Task<List<ApplicationUser>> GetUsersLikedComment(int commentId);
		
		/// <returns>{commentId => likesCount}</returns>
		Task<DefaultDictionary<int, int>> GetLikesCountsAsync(IEnumerable<int> commentIds);
		Task<int> GetLikesCountAsync(int commentId);

		Task<List<int>> GetCommentsLikedByUserAsync(string courseId, Guid slideId, string userId);
		Task<List<int>> GetCommentsLikedByUserAsync(string courseId, string userId);
		Task<bool> DidUserLikeComment(int commentId, string userId);
	}
}