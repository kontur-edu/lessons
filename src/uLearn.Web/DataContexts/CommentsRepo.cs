using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using uLearn.Web.Models;

namespace uLearn.Web.DataContexts
{
    public class CommentsRepo
    {
        private readonly ULearnDb db;

        public CommentsRepo() : this(new ULearnDb())
		{

        }

        public CommentsRepo(ULearnDb db)
        {
            this.db = db;
        }

        public async Task<Comment> AddComment(string authorId, string courseId, string slideId, int parentCommentId, string commentText)
        {
	        var comment = db.Comments.Create();
			comment.AuthorId = authorId;
			comment.CourseId = courseId;
			comment.SlideId = slideId;
			comment.ParentCommentId = parentCommentId;
			comment.Text = commentText;
			comment.PublishTime = DateTime.Now;
	        db.Comments.Add(comment);
            await db.SaveChangesAsync();

	        return db.Comments.Find(comment.Id);
        }

        public IEnumerable<Comment> GetSlideComments(string courseId, string slideId)
        {
            return db.Comments.Where(x => x.SlideId == slideId && ! x.IsDeleted);
        }

		///<returns>(likesCount, isLikedByThisUsed)</returns>
		public async Task<Tuple<int, bool>> LikeComment(int commentId, string userId)
		{
			var commentForLike = db.Comments.Find(commentId);
			if (commentForLike == null)
				throw new Exception("Comment " + commentId + " not found");
			var hisLike = db.CommentLikes.FirstOrDefault(x => x.UserId == userId && x.CommentId == commentId);
			var votedAlready = hisLike != null;
			var likesCount = commentForLike.Likes.Count;
			if (votedAlready)
			{
				db.CommentLikes.Remove(hisLike);
				likesCount--;
			}
			else
			{
				db.CommentLikes.Add(new CommentLike
				{
					UserId = userId,
					CommentId = commentId,
					Timestamp = DateTime.Now,
				});
				likesCount++;
			}
			await db.SaveChangesAsync();
			return Tuple.Create(likesCount, !votedAlready);
		}

		public IEnumerable<CommentLike> GetCommentLikes(Comment comment)
		{
			return db.CommentLikes.Where(x => x.CommentId == comment.Id);
		}

		public IEnumerable<ApplicationUser> GetCommentLikers(Comment comment)
		{
			return GetCommentLikes(comment).Select(x => x.User);
		}

		public int GetCommentLikesCount(Comment comment)
		{
			return db.CommentLikes.Count(x => x.CommentId == comment.Id);
		}

		/// <returns>{commentId => likesCount}</returns>
		public Dictionary<int, int> GetCommentsLikesCounts(IEnumerable<Comment> comments)
		{
			var commentsIds = comments.Select(x => x.Id).ToImmutableHashSet();
			return db.CommentLikes.Where(x => commentsIds.Contains(x.CommentId))
				.GroupBy(x => x.CommentId)
				.ToDictionary(x => x.Key, x => x.Count());
		}

	    public IEnumerable<int> GetSlideCommentsLikedByUser(string courseId, string slideId, string userId)
	    {
		    return db.CommentLikes.Where(x => x.UserId == userId && x.Comment.SlideId == slideId).Select(x => x.CommentId);
	    }
	}
}