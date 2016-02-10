using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using uLearn.Web.DataContexts;
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

			var model = new SlideCommentsModel
			{
				CourseId = courseId,
				SlideId = slideId,
				TopLevelComments = topLevelComments,
				CommentsByParent = commentsByParent,
			};
			return PartialView(model);
		}
	}

	public class SlideCommentsModel
	{
		public string CourseId { get; set; }
		public string SlideId { get; set; }
		public List<Comment> TopLevelComments { get; set; }
		public Dictionary<int, List<Comment>> CommentsByParent { get; set; }
	}
}