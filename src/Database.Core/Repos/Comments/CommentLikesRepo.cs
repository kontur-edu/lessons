using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Models.Comments;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Z.EntityFramework.Plus;

namespace Database.Repos.Comments
{
	/* This class is fully migrated to .NET Core and EF Core */
	public class CommentLikesRepo : ICommentLikesRepo
	{
		private readonly UlearnDb db;

		public CommentLikesRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task LikeAsync(int commentId, string userId)
		{
			await db.CommentLikes.Where(like => like.UserId == userId && like.CommentId == commentId).DeleteAsync().ConfigureAwait(false);
			
			db.CommentLikes.Add(new CommentLike
			{
				UserId = userId,
				CommentId = commentId,
				Timestamp = DateTime.Now,
			});
			
			try
			{
				await db.SaveChangesAsync().ConfigureAwait(false);
			}
			catch (Exception)
			{
				/* Somebody other have added like already. Ok, it's not a problem */	
			}
		}

		public async Task UnlikeAsync(int commentId, string userId)
		{
			await db.CommentLikes.Where(like => like.UserId == userId && like.CommentId == commentId).DeleteAsync().ConfigureAwait(false);
		}

		public Task<bool> DidUserLikeComment(int commentId, string userId)
		{
			return db.CommentLikes.Where(like => like.UserId == userId && like.CommentId == commentId).AnyAsync();
		}

		public Task<List<CommentLike>> GetLikesAsync(int commentId)
		{
			return db.CommentLikes.Include(like => like.User).Where(like => like.CommentId == commentId).OrderBy(like => like.Timestamp).ToListAsync();
		}

		public async Task<List<ApplicationUser>> GetUsersLikedComment(int commentId)
		{
			var likes = await GetLikesAsync(commentId).ConfigureAwait(false);
			return likes.Select(x => x.User).ToList();
		}

		public Task<int> GetLikesCountAsync(int commentId)
		{
			return db.CommentLikes.CountAsync(like => like.CommentId == commentId);
		}

		/// <returns>{ commentId => likesCount }</returns>
		public async Task<DefaultDictionary<int, int>> GetLikesCountsAsync(IEnumerable<int> commentIds)
		{
			var likesCountByComment = await db.CommentLikes
				.Where(like => commentIds.Contains(like.CommentId))
				.GroupBy(like => like.CommentId)
				.Select(g => new { CommentId = g.Key, LikesCount = g.Count()})
				.ToListAsync()
				.ConfigureAwait(false);
			return likesCountByComment.ToDictionary(x => x.CommentId, x => x.LikesCount).ToDefaultDictionary();
		}

		public Task<List<int>> GetCommentsLikedByUserAsync(string courseId, Guid slideId, string userId)
		{
			return db.CommentLikes
				.Where(like => like.UserId == userId && like.Comment.SlideId == slideId && like.Comment.CourseId == courseId)
				.Select(x => x.CommentId)
				.ToListAsync();
		}

		public Task<List<int>> GetCommentsLikedByUserAsync(string courseId, string userId)
		{
			return db.CommentLikes
				.Where(like => like.UserId == userId && like.Comment.CourseId == courseId)
				.Select(x => x.CommentId)
				.ToListAsync();
		}
	}
}