using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using uLearn.Web.DataContexts;
using uLearn.Web.FilterAttributes;
using uLearn.Web.Models;

namespace uLearn.Web.Controllers
{
	public class CommentsController : Controller
	{
		private readonly CommentsRepo commentsRepo = new CommentsRepo();
		private readonly UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ULearnDb()));

		public ActionResult SlideComments(string courseId, string slideId)
		{
			var comments = commentsRepo.GetSlideComments(courseId, slideId).ToList();

			var commentsByParent = comments.GroupBy(x => x.ParentCommentId)
				.ToDictionary(x => x.Key, x => x.OrderBy(c => c.PublishTime).ToList());
			
			/* Top-level comments (with ParentCommentId = -1)
			   are sorting by publish time, but pinned comments are always higher */
			List<Comment> topLevelComments;
			if (commentsByParent.ContainsKey(-1))
				topLevelComments = commentsByParent.Get(-1)
					.OrderBy(x => ! x.IsPinnedToTop).ThenBy(x => x.PublishTime).ToList();
			else
				topLevelComments = new List<Comment>();

			var commentsLikesCounts = commentsRepo.GetCommentsLikesCounts(comments);
			var commentsLikedByUser = commentsRepo.GetSlideCommentsLikedByUser(courseId, slideId, User.Identity.GetUserId()).ToImmutableHashSet();

			var isAuthorizedAndCanComment = User.Identity.IsAuthenticated;
			var canReply = User.Identity.IsAuthenticated;
			var canModerateComments = User.Identity.IsAuthenticated && User.HasAccessFor(courseId, CourseRole.Instructor);

			var model = new SlideCommentsModel
			{
				CourseId = courseId,
				SlideId = slideId,
				IsAuthorizedAndCanComment = isAuthorizedAndCanComment,
				CanReply = canReply,
				CanModerateComments = canModerateComments,
				TopLevelComments = topLevelComments,
				CommentsByParent = commentsByParent,
				CommentsLikesCounts = commentsLikesCounts,
				CommentsLikedByUser = commentsLikedByUser,
			};
			return PartialView(model);
		}

		[ULearnAuthorize]
		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> AddComment(string courseId, string slideId, string commentText, string parentCommentId)
		{
			var userId = User.Identity.GetUserId();
			var parentCommentIdInt = -1;
			if (parentCommentId != null)
				int.TryParse(parentCommentId, out parentCommentIdInt);
			var comment = await commentsRepo.AddComment(userId, courseId, slideId, parentCommentIdInt, commentText);

			return PartialView("_Comment", new CommentViewModel
			{
				Comment = comment,
				LikesCount = 0,
				IsLikedByUser = false,
				Replies = new List<CommentViewModel>(),
			});
		}

		[ULearnAuthorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> LikeComment(int commentId)
		{
			var userId = User.Identity.GetUserId();
			var res = await commentsRepo.LikeComment(commentId, userId);
			return Json(new { likesCount = res.Item1, liked = res.Item2 });
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task ApproveComment(int commentId)
		{
			await commentsRepo.ApproveComment(commentId);
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task RemoveComment(int commentId)
		{
			await commentsRepo.RemoveComment(commentId);
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task RestoreComment(int commentId)
		{
			await commentsRepo.RestoreComment(commentId);
		}
	}

	public class SlideCommentsModel
	{
		public string CourseId { get; set; }
		public string SlideId { get; set; }
		public bool IsAuthorizedAndCanComment { get; set; }
		public bool CanReply { get; set; }
		public bool CanModerateComments { get; set; }
		public List<Comment> TopLevelComments { get; set; }
		public Dictionary<int, List<Comment>> CommentsByParent { get; set; }
		public Dictionary<int, int> CommentsLikesCounts { get; set; }
		public ImmutableHashSet<int> CommentsLikedByUser { get; set; }
	}
}