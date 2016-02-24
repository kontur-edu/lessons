using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using ApprovalTests.Reporters;
using Microsoft.AspNet.Identity;
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

        public async Task<Comment> AddComment(IPrincipal author, string courseId, string slideId, int parentCommentId, string commentText)
        {
			var commentsPolicy = GetCommentsPolicy(courseId);
	        var isInstructor = author.HasAccessFor(courseId, CourseRole.Instructor);
	        var isApproved = commentsPolicy.ModerationPolicy == CommentModerationPolicy.Postmoderation || isInstructor;

			/* Instructors' replies are automaticly correct */
			var isReply = parentCommentId != -1;
			var isCorrectAnswer = isReply && isInstructor;

			var comment = db.Comments.Create();
			comment.AuthorId = author.Identity.GetUserId();
			comment.CourseId = courseId;
			comment.SlideId = slideId;
			comment.ParentCommentId = parentCommentId;
			comment.Text = commentText;
	        comment.IsApproved = isApproved;
	        comment.IsCorrectAnswer = isCorrectAnswer;
			comment.PublishTime = DateTime.Now;
	        db.Comments.Add(comment);
            await db.SaveChangesAsync();

	        return db.Comments.Find(comment.Id);
        }

	    public Comment GetCommentById(int commentId)
	    {
		    return db.Comments.Find(commentId);
	    }

        public IEnumerable<Comment> GetSlideComments(string courseId, string slideId)
        {
            return db.Comments.Where(x => x.SlideId == slideId && !x.IsDeleted);
        }

	    public IEnumerable<Comment> GetCourseComments(string courseId)
	    {
		    return db.Comments.Where(x => x.CourseId == courseId && !x.IsDeleted);
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

	    public CommentsPolicy GetCommentsPolicy(string courseId)
	    {
		    var policy = db.CommentsPolicies.FirstOrDefault(x => x.CourseId == courseId);
		    if (policy == null)
				policy = new CommentsPolicy
				{
					CourseId = courseId,
				};
			return policy;
	    }

	    public async Task SaveCommentsPolicy(CommentsPolicy policy)
	    {
		    using (var transaction = db.Database.BeginTransaction())
		    {
			    var query = db.CommentsPolicies.Where(x => x.CourseId == policy.CourseId);
			    if (query.Any())
			    {
				    db.CommentsPolicies.Remove(query.First());
				    await db.SaveChangesAsync();
			    }
			    db.CommentsPolicies.Add(policy);
			    await db.SaveChangesAsync();
			    transaction.Commit();
		    }
	    }

	    public async Task<Comment> ModifyComment(int commentId, Action<Comment> modifyAction)
	    {
			var comment = db.Comments.Find(commentId);
		    modifyAction(comment);
			await db.SaveChangesAsync();
		    return comment;
	    }

	    public async Task EditCommentText(int commentId, string newText)
	    {
		    await ModifyComment(commentId, c => c.Text = newText);
	    }

	    public async Task ApproveComment(int commentId)
	    {
		    await ModifyComment(commentId, c => c.IsApproved = true);
	    }

	    public async Task RemoveComment(int commentId)
	    {
			await ModifyComment(commentId, c => c.IsDeleted = true);
	    }

		public async Task<Comment> RestoreComment(int commentId)
		{
			return await ModifyComment(commentId, c => c.IsDeleted = false);
		}

	    public async Task PinComment(int commentId)
	    {
		    await ModifyComment(commentId, c => c.IsPinnedToTop = true);
		}

		public async Task UnpinComment(int commentId)
		{
			await ModifyComment(commentId, c => c.IsPinnedToTop = false);
		}

	    public async Task MarkCommentAsCorrectAnswer(int commentId, bool isCorrect=true)
	    {
		    await ModifyComment(commentId, c => c.IsCorrectAnswer = isCorrect);
	    }
    }
}