using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Models.Comments;
using Database.Repos.CourseRoles;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace Database.Repos.Comments
{
	/* This class is fully migrated to .NET Core and EF Core */
	public class CommentsRepo : ICommentsRepo
	{
		private readonly UlearnDb db;
		private readonly ICommentPoliciesRepo commentPoliciesRepo;
		private readonly ICourseRolesRepo courseRolesRepo;

		public CommentsRepo(UlearnDb db, ICommentPoliciesRepo commentPoliciesRepo, ICourseRolesRepo courseRolesRepo)
		{
			this.db = db;
			this.commentPoliciesRepo = commentPoliciesRepo;
			this.courseRolesRepo = courseRolesRepo;
		}
		
		public async Task<Comment> AddCommentAsync(string authorId, string courseId, Guid slideId, int parentCommentId, bool isForInstructorsOnly, string commentText)
		{
			var commentsPolicy = await commentPoliciesRepo.GetCommentsPolicyAsync(courseId).ConfigureAwait(false);
			var isInstructor = await courseRolesRepo.HasUserAccessToCourseAsync(authorId, courseId, CourseRoleType.Instructor).ConfigureAwait(false);
			var isApproved = commentsPolicy.ModerationPolicy == CommentModerationPolicy.Postmoderation || isInstructor;

			/* Instructors' replies are automatically correct */
			var isReply = parentCommentId != -1;
			var isCorrectAnswer = isReply && isInstructor && !isForInstructorsOnly;
			
			var comment = new Comment
			{
				AuthorId = authorId,
				CourseId = courseId,
				SlideId = slideId,
				ParentCommentId = parentCommentId,
				Text = commentText,
				IsApproved = isApproved,
				IsCorrectAnswer = isCorrectAnswer,
				IsForInstructorsOnly = isForInstructorsOnly,
				PublishTime = DateTime.Now
			};
			db.Comments.Add(comment);
			await db.SaveChangesAsync().ConfigureAwait(false);

			return await FindCommentByIdAsync(comment.Id).ConfigureAwait(false);
		}

		public async Task<Comment> FindCommentByIdAsync(int commentId, bool includeDeleted=false)
		{
			var comment = await db.Comments.FindAsync(commentId).ConfigureAwait(false);
			if (comment == null || (!includeDeleted && comment.IsDeleted))
				return null;
			return comment;
		}

		public Task<List<Comment>> GetCommentsByIdsAsync(IEnumerable<int> commentIds)
		{
			return db.Comments.Where(c => !c.IsDeleted && commentIds.Contains(c.Id)).ToListAsync();
		}

		public Task<List<Comment>> GetRepliesAsync(int commentId)
		{
			return db.Comments.Where(c => !c.IsDeleted && c.ParentCommentId == commentId).ToListAsync();
		}

		public async Task<DefaultDictionary<int, List<Comment>>> GetRepliesAsync(IEnumerable<int> commentIds)
		{
			var replies = await db.Comments.Where(c => !c.IsDeleted && commentIds.Contains(c.ParentCommentId)).ToListAsync().ConfigureAwait(false);
			return replies.GroupBy(c => c.ParentCommentId).ToDictionary(g => g.Key, g => g.ToList()).ToDefaultDictionary();
		}

		public Task<List<Comment>> GetSlideCommentsAsync(string courseId, Guid slideId)
		{
			return db.Comments
				.Include(c => c.Author)
				.Where(c => c.SlideId == slideId && !c.IsDeleted)
				.OrderBy(c => !c.IsPinnedToTop)
				.ThenBy(c => c.PublishTime)
				.ToListAsync();
		}
		
		public Task<List<Comment>> GetSlideTopLevelCommentsAsync(string courseId, Guid slideId)
		{
			return db.Comments
				.Include(c => c.Author)
				.Where(c => c.SlideId == slideId && !c.IsDeleted && c.ParentCommentId == -1)
				.OrderBy(c => !c.IsPinnedToTop)
				.ThenByDescending(c => c.PublishTime)
				.ToListAsync();
		}

		public Task<List<Comment>> GetSlidesCommentsAsync(string courseId, IEnumerable<Guid> slidesIds)
		{
			return db.Comments
				.Include(c => c.Author)
				.Where(c => slidesIds.Contains(c.SlideId) && !c.IsDeleted)
				.OrderBy(c => !c.IsPinnedToTop)
				.ThenBy(c => c.PublishTime)
				.ToListAsync();
		}

		public Task<List<Comment>> GetCourseCommentsAsync(string courseId)
		{
			return db.Comments
				.Include(c => c.Author)
				.Where(c => c.CourseId == courseId && !c.IsDeleted)
				.OrderBy(c => c.PublishTime)
				.ToListAsync();
		}

		public async Task<Comment> ModifyCommentAsync(int commentId, Action<Comment> modifyAction, bool includeDeleted=false)
		{
			var comment = await FindCommentByIdAsync(commentId, includeDeleted).ConfigureAwait(false) ?? throw new ArgumentException($"Can't find comment with id {commentId}");
			modifyAction(comment);
			await db.SaveChangesAsync().ConfigureAwait(false);
			return comment;
		}

		public Task<Comment> EditCommentTextAsync(int commentId, string newText)
		{
			return ModifyCommentAsync(commentId, c => c.Text = newText);
		}

		public Task<Comment> ApproveCommentAsync(int commentId, bool isApproved)
		{
			return ModifyCommentAsync(commentId, c => c.IsApproved = isApproved);
		}

		public Task DeleteCommentAsync(int commentId)
		{
			return ModifyCommentAsync(commentId, c => c.IsDeleted = true);
		}

		public Task<Comment> RestoreCommentAsync(int commentId)
		{
			return ModifyCommentAsync(commentId, c => c.IsDeleted = false, includeDeleted: true);
		}

		public Task<Comment> PinCommentAsync(int commentId, bool isPinned)
		{
			return ModifyCommentAsync(commentId, c => c.IsPinnedToTop = isPinned);
		}

		public Task<Comment> MarkCommentAsCorrectAnswerAsync(int commentId, bool isCorrect = true)
		{
			return ModifyCommentAsync(commentId, c => c.IsCorrectAnswer = isCorrect);
		}

		public async Task<bool> IsUserAddedMaxCommentsInLastTimeAsync(string userId, int maxCount, TimeSpan lastTime)
		{
			var lastComments = db.Comments
				.Where(x => x.AuthorId == userId)
				.OrderByDescending(x => x.PublishTime)
				.Take(maxCount)
				.OrderBy(x => x.PublishTime);
			
			var commentsCount = await lastComments.CountAsync().ConfigureAwait(false);
			if (commentsCount < maxCount || maxCount <= 0)
				return false;
			
			var boundComment = await lastComments.FirstOrDefaultAsync().ConfigureAwait(false);
			return boundComment == null || boundComment.PublishTime >= DateTime.Now - lastTime;
		}
	}
}