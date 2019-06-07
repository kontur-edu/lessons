using System.Threading.Tasks;
using Database;
using Database.Repos;
using Database.Repos.Comments;
using Database.Repos.CourseRoles;
using Database.Repos.Groups;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Serilog;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Web.Api.Models.Parameters.Comments;
using Ulearn.Web.Api.Models.Responses.Comments;

namespace Ulearn.Web.Api.Controllers.Comments
{
	[Route("/comment-policies")]
	public class CommentPoliciesController : BaseCommentController
	{
		private readonly ICommentPoliciesRepo commentPoliciesRepo;

		public CommentPoliciesController(ILogger logger, IWebCourseManager courseManager, UlearnDb db, IUsersRepo usersRepo,
			ICommentsRepo commentsRepo, ICommentLikesRepo commentLikesRepo, ICoursesRepo coursesRepo, ICourseRolesRepo courseRolesRepo, INotificationsRepo notificationsRepo,
			ICommentPoliciesRepo commentPoliciesRepo, IGroupMembersRepo groupMembersRepo, IGroupAccessesRepo groupAccessesRepo)
			: base(logger, courseManager, db, usersRepo, commentsRepo, commentLikesRepo, coursesRepo, courseRolesRepo, notificationsRepo, groupMembersRepo, groupAccessesRepo)
		{
			this.commentPoliciesRepo = commentPoliciesRepo;
		}

		public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var courseId = (string)context.ActionArguments["courseId"];
			if (!courseManager.HasCourse(courseId))
			{
				context.Result = NotFound(new ErrorResponse($"Course {courseId} not found"));
				return Task.CompletedTask;
			}
			
			return base.OnActionExecutionAsync(context, next);
		}

		/// <summary>
		/// Политика комментариев в курсе
		/// </summary>
		[HttpGet]
		public async Task<ActionResult<CommentPolicyResponse>> Policy([FromQuery(Name = "course_id")][BindRequired]string courseId)
		{
			var policy = await commentPoliciesRepo.GetCommentsPolicyAsync(courseId).ConfigureAwait(false);
			return new CommentPolicyResponse
			{
				AreCommentsEnabled = policy.IsCommentsEnabled,
				ModerationPolicy = policy.ModerationPolicy,
				OnlyInstructorsCanReply = policy.OnlyInstructorsCanReply
			};
		}

		/// <summary>
		/// Изменить политику комментариев в курсе
		/// </summary>
		[HttpPatch]
		[Authorize(Policy = "CourseAdmins")]
		public async Task<IActionResult> UpdatePolicy([FromQuery(Name = "course_id")][BindRequired]string courseId, [FromBody] UpdatePolicyParameters parameters)
		{
			var policy = await commentPoliciesRepo.GetCommentsPolicyAsync(courseId).ConfigureAwait(false);
			
			if (parameters.AreCommentsEnabled.HasValue)
				policy.IsCommentsEnabled = parameters.AreCommentsEnabled.Value;
			if (parameters.ModerationPolicy.HasValue)
				policy.ModerationPolicy = parameters.ModerationPolicy.Value;
			if (parameters.OnlyInstructorsCanReply.HasValue)
				policy.OnlyInstructorsCanReply = parameters.OnlyInstructorsCanReply.Value;

			await commentPoliciesRepo.SaveCommentsPolicyAsync(policy).ConfigureAwait(false);

			return Ok(new SuccessResponseWithMessage($"Comment policy for {courseId} successfully updated"));
		}
	}
}