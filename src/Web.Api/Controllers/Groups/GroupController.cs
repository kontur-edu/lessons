using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Database.Repos.CourseRoles;
using Database.Repos.Groups;
using Database.Repos.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Core.Courses;
using Ulearn.Web.Api.Models.Parameters.Groups;
using Ulearn.Web.Api.Models.Responses.Groups;

namespace Ulearn.Web.Api.Controllers.Groups
{
	[Route("/groups/{groupId:int:min(0)}/")]
	[ProducesResponseType((int) HttpStatusCode.NotFound)]
	[ProducesResponseType((int) HttpStatusCode.Forbidden)]
	[Authorize]
	public class GroupController : BaseGroupController
	{
		private readonly IGroupsRepo groupsRepo;
		private readonly IGroupAccessesRepo groupAccessesRepo;
		private readonly IGroupMembersRepo groupMembersRepo;
		private readonly ICourseRolesRepo courseRolesRepo;
		private readonly INotificationsRepo notificationsRepo;
		private readonly IGroupsCreatorAndCopier groupsCreatorAndCopier;

		public GroupController(ILogger logger, WebCourseManager courseManager, UlearnDb db,
			IGroupsRepo groupsRepo, IGroupAccessesRepo groupAccessesRepo, IGroupMembersRepo groupMembersRepo, IUsersRepo usersRepo, ICourseRolesRepo courseRolesRepo, INotificationsRepo notificationsRepo,
			IGroupsCreatorAndCopier groupsCreatorAndCopier)
			: base(logger, courseManager, db, usersRepo)
		{
			this.groupsRepo = groupsRepo;
			this.groupAccessesRepo = groupAccessesRepo;
			this.groupMembersRepo = groupMembersRepo;
			this.courseRolesRepo = courseRolesRepo;
			this.notificationsRepo = notificationsRepo;
			this.groupsCreatorAndCopier = groupsCreatorAndCopier;
		}

		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var groupId = (int) context.ActionArguments["groupId"];
			
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			if (group == null)
			{
				context.Result = NotFound(new ErrorResponse($"Group with id {groupId} not found"));
				return;
			}

			var isVisible = await groupAccessesRepo.IsGroupVisibleForUserAsync(group, UserId).ConfigureAwait(false);
			if (!isVisible)
			{
				context.Result = StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You have no access to this group"));
				return;
			}

			context.ActionArguments["group"] = group;

			await base.OnActionExecutionAsync(context, next).ConfigureAwait(false);
		}

		/// <summary>
		/// Информация о группе
		/// </summary>
		[HttpGet]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		public async Task<ActionResult<GroupInfo>> Group(int groupId)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			var members = await groupMembersRepo.GetGroupMembersAsync(groupId).ConfigureAwait(false);
			var accesses = await groupAccessesRepo.GetGroupAccessesAsync(groupId).ConfigureAwait(false);
			return BuildGroupInfo(group, members.Count, accesses);
		}

		/// <summary>
		/// Изменить группу
		/// </summary>
		[HttpPatch]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		public async Task<ActionResult<GroupInfo>> UpdateGroup(int groupId, [FromBody] UpdateGroupParameters parameters)
		{
			var hasEditAccess = await groupAccessesRepo.HasUserEditAccessToGroupAsync(groupId, UserId).ConfigureAwait(false);
			if (!hasEditAccess)
			{
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You have no edit access to this group"));
			}
			
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			
			var newName = parameters.Name ?? group.Name;
			var newIsManualCheckingEnabled = parameters.IsManualCheckingEnabled ?? group.IsManualCheckingEnabled;
			var newIsManualCheckingEnabledForOldSolutions = parameters.IsManualCheckingEnabledForOldSolutions ?? group.IsManualCheckingEnabledForOldSolutions;
			var newDefaultProhibitFurtherReview = parameters.DefaultProhibitFurtherReview ?? group.DefaultProhibitFutherReview;
			var newCanUsersSeeGroupProgress = parameters.CanStudentsSeeGroupProgress ?? group.CanUsersSeeGroupProgress;
			await groupsRepo.ModifyGroupAsync(
				groupId,
				newName,
				newIsManualCheckingEnabled,
				newIsManualCheckingEnabledForOldSolutions,
				newDefaultProhibitFurtherReview,
				newCanUsersSeeGroupProgress
			).ConfigureAwait(false);

			if (parameters.IsArchived.HasValue)
				await groupsRepo.ArchiveGroupAsync(groupId, parameters.IsArchived.Value).ConfigureAwait(false);

			if (parameters.IsInviteLinkEnabled.HasValue)
				await groupsRepo.EnableInviteLinkAsync(groupId, parameters.IsInviteLinkEnabled.Value).ConfigureAwait(false);

			return BuildGroupInfo(await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false));
		}

		/// <summary>
		/// Удалить группу
		/// </summary>
		[HttpDelete]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		public async Task<IActionResult> DeleteGroup(int groupId)
		{
			var hasEditAccess = await groupAccessesRepo.HasUserEditAccessToGroupAsync(groupId, UserId).ConfigureAwait(false);
			if (!hasEditAccess)
			{
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You have no edit access to this group"));
			}
			await groupsRepo.DeleteGroupAsync(groupId).ConfigureAwait(false);
			return Ok(new SuccessResponseWithMessage($"Group {groupId} has been deleted"));
		}

		/// <summary>
		/// Сменить владельца группы
		/// </summary>
		[HttpPut("owner")]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		[SwaggerResponse((int) HttpStatusCode.NotFound, Description = "Can't find user or user is not an instructor")]
		public async Task<IActionResult> ChangeOwner(int groupId, [FromBody] ChangeOwnerParameters parameters)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);

			var isCourseAdmin = await courseRolesRepo.HasUserAccessToCourseAsync(UserId, group.CourseId, CourseRoleType.CourseAdmin).ConfigureAwait(false);
			var canChangeOwner = group.OwnerId == UserId || isCourseAdmin;
			if (!canChangeOwner)
				return StatusCode((int) HttpStatusCode.Forbidden, new ErrorResponse("You can't change the owner of this group. Only current owner and course admin can change the owner."));

			/* New owner should exist and be a course instructor */
			var user = await usersRepo.FindUserByIdAsync(parameters.OwnerId).ConfigureAwait(false);
			if (user == null)
				return NotFound(new ErrorResponse($"Can't find user with id {parameters.OwnerId}"));
			var isInstructor = await courseRolesRepo.HasUserAccessToCourseAsync(parameters.OwnerId, group.CourseId, CourseRoleType.Instructor).ConfigureAwait(false);
			if (!isInstructor)
				return NotFound(new ErrorResponse($"User {parameters.OwnerId} is not an instructor of course {group.CourseId}"));
			
			/* Grant full access to previous owner */
			await groupAccessesRepo.GrantAccessAsync(groupId, group.OwnerId, GroupAccessType.FullAccess, UserId).ConfigureAwait(false);
			/* Change owner */
			await groupsRepo.ChangeGroupOwnerAsync(groupId, parameters.OwnerId).ConfigureAwait(false);
			/* Revoke access from new owner */
			await groupAccessesRepo.RevokeAccessAsync(groupId, parameters.OwnerId).ConfigureAwait(false);

			return Ok(new SuccessResponseWithMessage($"New group's owner is {parameters.OwnerId}"));
		}

		/// <summary>
		/// Копирует группу в тот же или другой курс
		/// </summary>
		[HttpPost("copy")]
		[SwaggerResponse((int) HttpStatusCode.Created, Description = "Group has been copied", Type = typeof(CopyGroupResponse))]
		[SwaggerResponse((int) HttpStatusCode.NotFound, Description = "Course not found")]
		[SwaggerResponse((int) HttpStatusCode.Forbidden, Description = "You have no access to destination course. You should be instructor or course admin.")]
		public async Task<ActionResult<CopyGroupResponse>> Copy(int groupId, [FromQuery] CopyGroupParameters parameters)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			if (!courseManager.HasCourse(parameters.DestinationCourseId))
				return NotFound(new ErrorResponse($"Course {parameters.DestinationCourseId} not found"));
			if (! await CanCreateGroupInCourseAsync(UserId, parameters.DestinationCourseId).ConfigureAwait(false))
				return Forbid();
			
			var newOwnerId = parameters.ChangeOwner ? UserId : null;
			var newGroup = await groupsCreatorAndCopier.CopyGroupAsync(group, parameters.DestinationCourseId, newOwnerId).ConfigureAwait(false);

			var url = Url.Action(new UrlActionContext { Action = nameof(Group), Controller = "Group", Values = new { groupId = group.Id }});
			return Created(url, new CopyGroupResponse
			{
				Id = newGroup.Id,
				ApiUrl = url
			});
		}
		
		private async Task<bool> CanCreateGroupInCourseAsync(string userId, string courseId)
		{
			return await courseRolesRepo.HasUserAccessToCourseAsync(userId, courseId, CourseRoleType.Instructor).ConfigureAwait(false) || 
				await IsSystemAdministratorAsync().ConfigureAwait(false);
		}

		/// <summary>
		/// Список scoring-group курса (примеры: Упражнения, Активность на практике) с информаций о том, включены ли они для этой группы
		/// </summary>
		[HttpGet("scores")]
		public async Task<ActionResult<GroupScoringGroupsResponse>> ScoringGroups(int groupId)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			var course = courseManager.FindCourse(group.CourseId);
			if (course == null)
			{
				logger.Error($"It's strange: group {groupId} exists, but course {group.CourseId} not. I will return 404");
				return NotFound(new ErrorResponse("Group or course not found"));
			}

			var scoringGroups = course.Settings.Scoring.Groups.Values.ToList();
			var scoringGroupsCanBeSetInSomeUnit = GetScoringGroupsCanBeSetInSomeUnit(course);
			var enabledScoringGroups = await groupsRepo.GetEnabledAdditionalScoringGroupsForGroupAsync(groupId).ConfigureAwait(false);
			return new GroupScoringGroupsResponse
			{
				Scores = scoringGroups.Select(scoringGroup => BuildGroupScoringGroupInfo(scoringGroup, scoringGroupsCanBeSetInSomeUnit, enabledScoringGroups)).ToList(),
			};
		}

		/// <summary>
		/// Сохраняет информацию о том, какие scoring-group включены для группы
		/// </summary>
		[HttpPost("scores")]
		public async Task<IActionResult> SetScoringGroups(int groupId, SetScoringGroupsParameters parameters)
		{
			var hasEditAccess = await groupAccessesRepo.HasUserEditAccessToGroupAsync(groupId, UserId).ConfigureAwait(false);
			if (!hasEditAccess)
			{
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You have no edit access to this group"));
			}
			
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			var course = courseManager.FindCourse(group.CourseId);
			if (course == null)
			{
				logger.Error($"It's strange: group {groupId} exists, but course {group.CourseId} not. I will return 404");
				return NotFound(new ErrorResponse("Group or course not found"));
			}

			var courseScoringGroupIds = course.Settings.Scoring.Groups.Values.Select(g => g.Id).ToList();
			var scoringGroupsCanBeSetInSomeUnit = GetScoringGroupsCanBeSetInSomeUnit(course).Select(g => g.Id).ToList();
			foreach (var scoringGroupId in parameters.Scores) {
				if (!courseScoringGroupIds.Contains(scoringGroupId))
					return NotFound(new ErrorResponse($"Score {scoringGroupId} not found in course {course.Id}"));
				
				var scoringGroup = course.Settings.Scoring.Groups[scoringGroupId];
				if (scoringGroup.EnabledForEveryone)
					return BadRequest(new ErrorResponse($"You can not enable or disable additional scoring for {scoringGroupId}, because it is enabled for all groups by course creator."));
				
				if (! scoringGroupsCanBeSetInSomeUnit.Contains(scoringGroupId))
					return BadRequest(new ErrorResponse($"You can not enable or disable additional scoring for {scoringGroupId}, because there is no additional scores for this score in any unit. Contact with course creator."));
			}

			await groupsRepo.EnableAdditionalScoringGroupsForGroupAsync(groupId, parameters.Scores).ConfigureAwait(false);

			return Ok(new SuccessResponseWithMessage($"Scores for group {groupId} updated"));
		}

		private static GroupScoringGroupInfo BuildGroupScoringGroupInfo(ScoringGroup scoringGroup, List<ScoringGroup> scoringGroupsCanBeSetInSomeUnit, List<EnabledAdditionalScoringGroup> enabledScoringGroups)
		{
			var canBeSetByInstructorInSomeUnit = scoringGroupsCanBeSetInSomeUnit.Select(g => g.Id).Contains(scoringGroup.Id);
			var isEnabledManually = enabledScoringGroups.Select(g => g.ScoringGroupId).Contains(scoringGroup.Id);
			return new GroupScoringGroupInfo
			{
				Id = scoringGroup.Id,
				Name = scoringGroup.Name ?? "",
				Abbreviation = scoringGroup.Abbreviation ?? "",
				Description = scoringGroup.Description ?? "",
				AreAdditionalScoresEnabledForAllGroups = scoringGroup.EnabledForEveryone,
				CanInstructorSetAdditionalScoreInSomeUnit = canBeSetByInstructorInSomeUnit,
				AreAdditionalScoresEnabledInThisGroup = (scoringGroup.EnabledForEveryone || !canBeSetByInstructorInSomeUnit) ? (bool?) null : isEnabledManually
			};
		}

		private static List<ScoringGroup> GetScoringGroupsCanBeSetInSomeUnit(ICourse course)
		{
			return course.Units
				.SelectMany(u => u.Scoring.Groups.Values)
				.Where(g => g.CanBeSetByInstructor && !g.EnabledForEveryone)
				.DistinctBy(g => g.Id)
				.ToList();
		}

		/// <summary>
		/// Список студентов группы
		/// </summary>
		[HttpGet("students")]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		public async Task<ActionResult<GroupStudentsResponse>> GroupStudents(int groupId)
		{
			var members = await groupMembersRepo.GetGroupMembersAsync(groupId).ConfigureAwait(false);
			return new GroupStudentsResponse
			{
				Students = members.Select(m => new GroupStudentInfo
				{
					User = BuildShortUserInfo(m.User, discloseLogin: true),
					AddingTime = m.AddingTime
				}).ToList()
			};
		}

		/// <summary>
		/// Добавить студента в группу
		/// </summary>
		[HttpPost("students/{studentId:guid}")]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		[SwaggerResponse((int) HttpStatusCode.NotFound, Description = "Can't find user")]
		[SwaggerResponse((int) HttpStatusCode.Conflict, Description = "User is already a student of this group")]
		public async Task<IActionResult> AddStudent(int groupId, string studentId)
		{
			if (!await IsSystemAdministratorAsync().ConfigureAwait(false))
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("Only system administrator can add students to group directly"));
			
			var user = await usersRepo.FindUserByIdAsync(studentId).ConfigureAwait(false);
			if (user == null)
				return NotFound(new ErrorResponse($"Can't find user with id {studentId}"));
			
			var groupMember = await groupMembersRepo.AddUserToGroupAsync(groupId, studentId).ConfigureAwait(false);
			if (groupMember == null)
				return StatusCode((int)HttpStatusCode.Conflict, new ErrorResponse($"User {studentId} is already a student of group {groupId}"));

			return Ok(new SuccessResponseWithMessage($"Student {studentId} is added to group {groupId}"));
		}
		
		/// <summary>
		/// Удалить студента из группы 
		/// </summary>
		[HttpDelete("students/{studentId:guid}")]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		[SwaggerResponse((int) HttpStatusCode.NotFound, Description = "Can't find user or user is not a student of this group")]
		public async Task<IActionResult> RemoveStudent(int groupId, string studentId)
		{
			var hasEditAccess = await groupAccessesRepo.HasUserEditAccessToGroupAsync(groupId, UserId).ConfigureAwait(false);
			if (!hasEditAccess)
			{
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You have no edit access to this group"));
			}
			
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			
			var user = await usersRepo.FindUserByIdAsync(studentId).ConfigureAwait(false);
			if (user == null)
				return NotFound(new ErrorResponse($"Can't find user with id {studentId}"));

			var groupMember = await groupMembersRepo.RemoveUserFromGroupAsync(groupId, studentId).ConfigureAwait(false);
			if (groupMember == null)
				return NotFound(new ErrorResponse($"User {studentId} is not a student of group {groupId}"));
			
			await notificationsRepo.AddNotificationAsync(
				group.CourseId,
				new GroupMembersHaveBeenRemovedNotification(groupId, new List<string> { studentId }, usersRepo),
				UserId
			).ConfigureAwait(false);

			return Ok(new SuccessResponseWithMessage($"Student {studentId} is removed from group {groupId}"));
		}

		/// <summary>
		/// Удалить студентов из группы
		/// </summary>
		[HttpDelete("students")]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		public async Task<IActionResult> RemoveStudents(int groupId, RemoveStudentsParameters parameters)
		{
			var hasEditAccess = await groupAccessesRepo.HasUserEditAccessToGroupAsync(groupId, UserId).ConfigureAwait(false);
			if (!hasEditAccess)
			{
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You have no edit access to this group"));
			}
			
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);

			var members = await groupMembersRepo.RemoveUsersFromGroupAsync(groupId, parameters.StudentIds).ConfigureAwait(false);
			
			await notificationsRepo.AddNotificationAsync(
				group.CourseId,
				new GroupMembersHaveBeenRemovedNotification(group.Id, parameters.StudentIds, usersRepo),
				UserId
			).ConfigureAwait(false);

			return Ok(new SuccessResponseWithMessage($"{members.Count} students have been removed from group {group.Id}"));
		}

		/// <summary>
		/// Скопировать студентов в группу
		/// </summary>
		[HttpPost("students")]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		public async Task<IActionResult> CopyStudents(int groupId, CopyStudentsParameters parameters)
		{
			var destinationGroup = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			if (destinationGroup == null)
				return NotFound(new ErrorResponse($"Group {groupId} not found"));
			
			var hasDestinationGroupEditAccess = await groupAccessesRepo.HasUserEditAccessToGroupAsync(destinationGroup.Id, UserId).ConfigureAwait(false);
			if (!hasDestinationGroupEditAccess)
				return StatusCode((int) HttpStatusCode.Forbidden, new ErrorResponse($"You have no edit access to group {groupId}"));

			var membersOfAllGroupsAvailableForUser = (await groupAccessesRepo.GetMembersOfAllGroupsAvailableForUserAsync(UserId).ConfigureAwait(false))
				.Select(gm => gm.UserId);

			var studentsToCopySet = parameters.StudentIds.ToHashSet();
			studentsToCopySet.IntersectWith(membersOfAllGroupsAvailableForUser);

			var newMembers = await groupMembersRepo.AddUsersToGroupAsync(groupId, studentsToCopySet).ConfigureAwait(false);
			
			await notificationsRepo.AddNotificationAsync(
				destinationGroup.CourseId,
				new GroupMembersHaveBeenAddedNotification(groupId, parameters.StudentIds, usersRepo),
				UserId
			).ConfigureAwait(false);

			return Ok(new SuccessResponseWithMessage($"{newMembers.Count} students have been copied to group {groupId}"));
		}

		/// <summary>
		/// Список доступов к группе
		/// </summary>
		[HttpGet("accesses")]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		public async Task<ActionResult<GroupAccessesResponse>> GroupAccesses(int groupId)
		{
			var accesses = await groupAccessesRepo.GetGroupAccessesAsync(groupId).ConfigureAwait(false);
			return new GroupAccessesResponse
			{
				Accesses = accesses.Select(BuildGroupAccessesInfo).ToList()
			};
		}

		/// <summary>
		/// Выдать доступ к группе
		/// </summary>
		[HttpPost("accesses/{userId:guid}")]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		[SwaggerResponse((int) HttpStatusCode.Conflict, "User already has access to group")]
		/* TODO (andgein): We don't check that userId is Instructor of course. Should we check it? Or not? */
		public async Task<IActionResult> GrantAccess(int groupId, string userId)
		{
			var hasEditAccess = await groupAccessesRepo.HasUserEditAccessToGroupAsync(groupId, UserId).ConfigureAwait(false);
			if (!hasEditAccess)
			{
				return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse("You have no edit access to this group"));
			}
			
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			
			var user = await usersRepo.FindUserByIdAsync(userId).ConfigureAwait(false);
			if (user == null)
				return NotFound(new ErrorResponse($"User {userId} not found"));

			var alreadyHasAccess = await groupAccessesRepo.HasUserGrantedAccessToGroupOrIsOwnerAsync(groupId, userId).ConfigureAwait(false);
			if (alreadyHasAccess)
				return Conflict(new ErrorResponse($"User {userId} already has access to group {groupId}"));

			var access = await groupAccessesRepo.GrantAccessAsync(groupId, userId, GroupAccessType.FullAccess, UserId).ConfigureAwait(false);
			await notificationsRepo.AddNotificationAsync(group.CourseId, new GrantedAccessToGroupNotification { AccessId = access.Id }, UserId).ConfigureAwait(false);
			
			return Ok(new SuccessResponseWithMessage($"User {userId} has full access to {groupId}"));
		}

		/// <summary>
		/// Отозвать доступ к группе
		/// </summary>
		[HttpDelete("accesses/{userId:guid}")]
		[ProducesResponseType((int) HttpStatusCode.OK)]
		public async Task<IActionResult> RevokeAccess(int groupId, string userId)
		{
			var group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			
			var canRevokeAccess = await groupAccessesRepo.CanRevokeAccessAsync(groupId, userId, UserId).ConfigureAwait(false) ||
				await IsSystemAdministratorAsync().ConfigureAwait(false);
			if (!canRevokeAccess)
				return Forbid();

			var hasAccess = await groupAccessesRepo.HasUserGrantedAccessToGroupOrIsOwnerAsync(groupId, userId).ConfigureAwait(false);
			if (!hasAccess)
				return NotFound(new ErrorResponse($"User {userId} has no access to group {groupId}"));

			var accesses = await groupAccessesRepo.RevokeAccessAsync(groupId, userId).ConfigureAwait(false);
			foreach (var access in accesses)
				await notificationsRepo.AddNotificationAsync(
					group.CourseId,
					new RevokedAccessToGroupNotification { AccessId = access.Id },
					UserId
				).ConfigureAwait(false);

			return Ok(new SuccessResponseWithMessage($"User {userId} has no access to {groupId}"));
		}
	}
}