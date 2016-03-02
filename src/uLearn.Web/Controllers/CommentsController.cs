using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Security.Principal;
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

			var isInstructor = User.HasAccessFor(courseId, CourseRole.Instructor);
			var isAuthorizedAndCanComment = CanAddCommentHere(User, courseId, false);
			var canReply = CanAddCommentHere(User, courseId, true);
			var canModerateComments = User.Identity.IsAuthenticated && isInstructor;
			var canSeeNotApprovedComments = User.Identity.IsAuthenticated && isInstructor;

			var model = new SlideCommentsModel
			{
				CourseId = courseId,
				SlideId = slideId,
				IsAuthorizedAndCanComment = isAuthorizedAndCanComment,
				CanReply = canReply,
				CanModerateComments = canModerateComments,
				CanSeeNotApprovedComments = canSeeNotApprovedComments,
				TopLevelComments = topLevelComments,
				CommentsByParent = commentsByParent,
				CommentsLikesCounts = commentsLikesCounts,
				CommentsLikedByUser = commentsLikedByUser,
				CurrentUser = User.Identity.IsAuthenticated ? userManager.FindById(User.Identity.GetUserId()) : null,
			};
			return PartialView(model);
		}

		private bool CanAddCommentHere(IPrincipal user, string courseId, bool isReply)
		{
			if (!User.Identity.IsAuthenticated)
				return false;

			var commentPolicy = commentsRepo.GetCommentsPolicy(courseId);
			var isInstructor = user.HasAccessFor(courseId, CourseRole.Instructor);

			if (!isInstructor && !commentPolicy.IsCommentsEnabled)
				return false;

			if (isReply && !isInstructor && commentPolicy.OnlyInstructorsCanReply)
				return false;

			return true;
		}

		[ULearnAuthorize]
		[HttpPost]
		[ValidateInput(false)]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> AddComment(string courseId, string slideId, string commentText, string parentCommentId)
		{
			var parentCommentIdInt = -1;
			if (parentCommentId != null)
				int.TryParse(parentCommentId, out parentCommentIdInt);

			if (! CanAddComment(User, courseId, parentCommentIdInt != -1))
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

			var comment = await commentsRepo.AddComment(User, courseId, slideId, parentCommentIdInt, commentText);
			var canReply = CanAddComment(User, courseId, true);

			return PartialView("_Comment", new CommentViewModel
			{
				Comment = comment,
				LikesCount = 0,
				IsLikedByUser = false,
				Replies = new List<CommentViewModel>(),
				IsCommentVisibleForUser = true,
				CanEditAndDeleteComment = true,
				CanModerateComment = User.HasAccessFor(courseId, CourseRole.Instructor),
				CanReply = canReply,
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
		public async Task<ActionResult> ApproveComment(int commentId, bool isApproved=true)
		{
			var comment = commentsRepo.GetCommentById(commentId);
			if (!User.HasAccessFor(comment.CourseId, CourseRole.Instructor))
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

			await commentsRepo.ApproveComment(commentId, isApproved);
			return new HttpStatusCodeResult(HttpStatusCode.OK);
		}

		[ULearnAuthorize(MinAccessLevel = CourseRole.Instructor)]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> PinComment(int commentId, bool isPinned)
		{
			var comment = commentsRepo.GetCommentById(commentId);
			if (!User.HasAccessFor(comment.CourseId, CourseRole.Instructor))
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			
			await commentsRepo.PinComment(commentId, isPinned);
			return new HttpStatusCodeResult(HttpStatusCode.OK);
		}

		private bool CanEditAndDeleteComment(IPrincipal user, Comment comment)
		{
			return user.HasAccessFor(comment.CourseId, CourseRole.Instructor) ||
					user.Identity.GetUserId() == comment.AuthorId;
		}
		
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> DeleteComment(int commentId)
		{
			var comment = commentsRepo.GetCommentById(commentId);
			if (! CanEditAndDeleteComment(User, comment))
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

			await commentsRepo.DeleteComment(commentId);
			return new HttpStatusCodeResult(HttpStatusCode.OK);
		}
		
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> RestoreComment(int commentId)
		{
			var comment = commentsRepo.GetCommentById(commentId);
			if (!CanEditAndDeleteComment(User, comment))
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

			await commentsRepo.RestoreComment(commentId);
			return new HttpStatusCodeResult(HttpStatusCode.OK);
		}

		[ValidateInput(false)]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> EditCommentText(int commentId, string newText)
		{
			var comment = commentsRepo.GetCommentById(commentId);
			if (!CanEditAndDeleteComment(User, comment))
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

			await commentsRepo.EditCommentText(commentId, newText);
			return new HttpStatusCodeResult(HttpStatusCode.OK);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> MarkAsCorrectAnswer(int commentId, bool isCorrect=true)
		{
			var comment = commentsRepo.GetCommentById(commentId);
			if (!CanEditAndDeleteComment(User, comment))
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

			await commentsRepo.MarkCommentAsCorrectAnswer(commentId, isCorrect);
			return new HttpStatusCodeResult(HttpStatusCode.OK);
		}
	}

	public class SlideCommentsModel
	{
		public string CourseId { get; set; }
		public string SlideId { get; set; }
		public bool IsAuthorizedAndCanComment { get; set; }
		public bool CanReply { get; set; }
		public bool CanModerateComments { get; set; }
		public bool CanSeeNotApprovedComments { get; set; }
		public List<Comment> TopLevelComments { get; set; }
		public Dictionary<int, List<Comment>> CommentsByParent { get; set; }
		public Dictionary<int, int> CommentsLikesCounts { get; set; }
		public ImmutableHashSet<int> CommentsLikedByUser { get; set; }
		public ApplicationUser CurrentUser { get; set; }
	}
}