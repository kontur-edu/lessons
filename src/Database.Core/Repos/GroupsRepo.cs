using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Database.Extensions;
using Database.Models;
using JetBrains.Annotations;
using log4net;
using Microsoft.EntityFrameworkCore;
using uLearn;
using uLearn.Quizes;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace Database.Repos
{
	public class GroupsRepo
	{
		private readonly ILog log = LogManager.GetLogger(typeof(GroupsRepo));

		private readonly UlearnDb db;
		private readonly SlideCheckingsRepo slideCheckingsRepo;
		private readonly UserSolutionsRepo userSolutionsRepo;
		private readonly UserQuizzesRepo userQuizzesRepo;
		private readonly VisitsRepo visitsRepo;
		private readonly UserRolesRepo userRolesRepo;

		private readonly WebCourseManager courseManager;

		public GroupsRepo(
			UlearnDb db,
			SlideCheckingsRepo slideCheckingsRepo, UserSolutionsRepo userSolutionsRepo, UserQuizzesRepo userQuizzesRepo, VisitsRepo visitsRepo, UserRolesRepo userRolesRepo,
			WebCourseManager courseManager)
		{
			this.db = db;
			this.slideCheckingsRepo = slideCheckingsRepo;
			this.userSolutionsRepo = userSolutionsRepo;
			this.userQuizzesRepo = userQuizzesRepo;
			this.visitsRepo = visitsRepo;
			this.userRolesRepo = userRolesRepo;
			this.courseManager = courseManager;
		}

		public bool CanUserSeeAllCourseGroups(ClaimsPrincipal user, string courseId)
		{
			return user.HasAccessFor(courseId, CourseRole.CourseAdmin);
		}

		public async Task<Group> CreateGroup(string courseId, string name, string ownerId, bool isManualCheckingEnabled = false, bool isManualCheckingEnabledForOldSolutions = false)
		{
			var group = new Group
			{
				CourseId = courseId,
				Name = name,
				OwnerId = ownerId,
				IsManualCheckingEnabled = isManualCheckingEnabled,
				IsManualCheckingEnabledForOldSolutions = isManualCheckingEnabledForOldSolutions,
			};
			db.Groups.Add(group);
			await db.SaveChangesAsync();

			return group;
		}

		/* Copy group from one course to another. Replaces owner only if newOwnerId is not empty */

		public async Task<Group> CopyGroup(Group group, string courseId, string newOwnerId = "")
		{
			var newGroup = await CopyGroupWithoutMembers(group, courseId, newOwnerId);
			await CopyGroupMembers(group, newGroup);
			await CopyGroupAccesses(group, newGroup);

			/* We can also copy group's scoring-group settings if their are in one course */
			if (courseId == group.CourseId)
			{
				await CopyEnabledAdditionalScoringGroups(group, newGroup);
			}

			return newGroup;
		}

		private async Task<Group> CopyGroupWithoutMembers(Group group, string courseId, string newOwnerId)
		{
			var newName = group.Name;
			if (courseId == group.CourseId)
				newName += " (копия)";
			var newGroup = new Group
			{
				CourseId = courseId,
				OwnerId = string.IsNullOrEmpty(newOwnerId) ? group.OwnerId : newOwnerId,
				Name = newName,
				CanUsersSeeGroupProgress = group.CanUsersSeeGroupProgress,
				IsManualCheckingEnabled = group.IsManualCheckingEnabled,
				IsInviteLinkEnabled = group.IsInviteLinkEnabled,
				InviteHash = Guid.NewGuid(),
			};
			db.Groups.Add(newGroup);
			await db.SaveChangesAsync();
			return newGroup;
		}

		private async Task CopyGroupMembers(Group group, Group newGroup)
		{
			var members = group.Members.Select(m => new GroupMember
			{
				UserId = m.UserId,
				GroupId = newGroup.Id,
			});
			db.GroupMembers.AddRange(members);
			await db.SaveChangesAsync();
		}

		private async Task CopyGroupAccesses(Group group, Group newGroup)
		{
			var accesses = db.GroupAccesses.Where(a => a.GroupId == group.Id && a.IsEnabled).ToList();
			var courseInstructorsIds = userRolesRepo.GetListOfUsersWithCourseRole(CourseRole.Instructor, newGroup.CourseId, includeHighRoles: true);
			foreach (var access in accesses)
			{
				if (! courseInstructorsIds.Contains(access.UserId))
					continue;
				db.GroupAccesses.Add(new GroupAccess
				{
					GroupId = newGroup.Id,
					UserId = access.UserId,
					AccessType = access.AccessType,
					GrantedById = access.GrantedById,
					GrantTime = DateTime.Now,
					IsEnabled = true,
				});
			}

			await db.SaveChangesAsync();
		}
		
		private async Task CopyEnabledAdditionalScoringGroups(Group group, Group newGroup)
		{
			var enabledAdditionalScoringGroups = db.EnabledAdditionalScoringGroups.Where(s => s.GroupId == group.Id).Select(s => s.ScoringGroupId).ToList();
			foreach (var scoringGroupId in enabledAdditionalScoringGroups)
				db.EnabledAdditionalScoringGroups.Add(new EnabledAdditionalScoringGroup
				{
					GroupId = newGroup.Id,
					ScoringGroupId = scoringGroupId,
				});

			await db.SaveChangesAsync();
		}

		public async Task<Group> ModifyGroup(int groupId, string newName, bool newIsManualCheckingEnabled, bool newIsManualCheckingEnabledForOldSolutions, bool defaultProhibitFutherReview)
		{
			var group = FindGroupById(groupId);
			group.Name = newName;
			group.IsManualCheckingEnabled = newIsManualCheckingEnabled;

			if (!group.IsManualCheckingEnabledForOldSolutions && newIsManualCheckingEnabledForOldSolutions)
				await AddManualCheckingsForOldSolutions(group.CourseId, group.Members.Select(m => m.UserId).ToList());

			group.IsManualCheckingEnabledForOldSolutions = newIsManualCheckingEnabledForOldSolutions;
			group.DefaultProhibitFutherReview = defaultProhibitFutherReview;
			await db.SaveChangesAsync();

			return group;
		}

		public async Task ChangeOwner(int groupId, string newOwnerId)
		{
			var group = FindGroupById(groupId);
			group.OwnerId = newOwnerId;

			await db.SaveChangesAsync();
		}

		public async Task<Group> ArchiveGroup(int groupId, bool isArchived)
		{
			var group = FindGroupById(groupId);
			group.IsArchived = isArchived;
			await db.SaveChangesAsync();

			return group;
		}

		public async Task RemoveGroup(int groupId)
		{
			var group = db.Groups.Find(groupId);
			if (group != null)
			{
				group.IsDeleted = true;
				await db.SaveChangesAsync();
			}
		}

		public async Task<GroupMember> AddUserToGroup(int groupId, string userId)
		{
			var groupMember = new GroupMember
			{
				GroupId = groupId,
				UserId = userId,
				AddingTime = DateTime.Now,
			};
			using (var transaction = db.Database.BeginTransaction())
			{
				/* Don't add member if it's already exists */
				var existsMember = db.GroupMembers.FirstOrDefault(m => m.GroupId == groupId && m.UserId == userId);
				if (existsMember != null)
					return null;

				db.GroupMembers.Add(groupMember);
				await db.SaveChangesAsync();

				transaction.Commit();
			}

			var group = FindGroupById(groupId);
			if (group != null && group.IsManualCheckingEnabledForOldSolutions)
				await AddManualCheckingsForOldSolutions(group.CourseId, userId);

			return groupMember;
		}

		public async Task<GroupMember> AddUserToGroup(int groupId, ApplicationUser user)
		{
			return await AddUserToGroup(groupId, user.Id);
		}

		private async Task AddManualCheckingsForOldSolutions(string courseId, IEnumerable<string> usersIds)
		{
			foreach (var userId in usersIds)
				await AddManualCheckingsForOldSolutions(courseId, userId);
		}

		private async Task AddManualCheckingsForOldSolutions(string courseId, string userId)
		{
			log.Info($"Создаю ручные проверки для всех решения пользователя {userId} в курсе {courseId}");

			var course = courseManager.GetCourse(courseId);

			/* For exercises */
			var acceptedSubmissionsBySlide = userSolutionsRepo.GetAllAcceptedSubmissionsByUser(courseId, userId)
				.GroupBy(s => s.SlideId)
				.ToDictionary(g => g.Key, g => g.ToList());
			foreach (var acceptedSubmissionsForSlide in acceptedSubmissionsBySlide.Values)
				/* If exists at least one manual checking for at least one submissions on slide, then ignore this slide */
				if (!acceptedSubmissionsForSlide.Any(s => s.ManualCheckings.Any()))
				{
					/* Otherwise found the latest accepted submission */
					var lastSubmission = acceptedSubmissionsForSlide.OrderByDescending(s => s.Timestamp).First();

					var slideId = lastSubmission.SlideId;
					var slide = course.FindSlideById(slideId) as ExerciseSlide;
					if (slide == null || !slide.Exercise.RequireReview)
						continue;

					log.Info($"Создаю ручную проверку для решения {lastSubmission.Id}, слайд {slideId}");
					await slideCheckingsRepo.AddManualExerciseChecking(courseId, slideId, userId, lastSubmission);
					await visitsRepo.MarkVisitsAsWithManualChecking(slideId, userId);
				}

			/* For quizzes */
			var passedQuizzesIds = userQuizzesRepo.GetIdOfQuizPassedSlides(courseId, userId);
			foreach (var quizSlideId in passedQuizzesIds)
			{
				var slide = course.FindSlideById(quizSlideId) as QuizSlide;
				if (slide == null || !slide.ManualChecking)
					continue;
				if (!userQuizzesRepo.IsWaitingForManualCheck(courseId, quizSlideId, userId))
				{
					log.Info($"Создаю ручную проверку для теста {slide.Id}");
					await slideCheckingsRepo.AddQuizAttemptForManualChecking(courseId, quizSlideId, userId);
					await visitsRepo.MarkVisitsAsWithManualChecking(quizSlideId, userId);
				}
			}
		}

		public async Task<GroupMember> RemoveUserFromGroup(int groupId, string userId)
		{
			var member = db.GroupMembers.FirstOrDefault(m => m.GroupId == groupId && m.UserId == userId);
			if (member != null)
				db.GroupMembers.Remove(member);

			await db.SaveChangesAsync();
			return member;
		}
		
		public async Task<List<GroupMember>> RemoveUsersFromGroup(int groupId, List<string> userIds)
		{
			var members = db.GroupMembers.Where(m => m.GroupId == groupId && userIds.Contains(m.UserId)).ToList();
			db.GroupMembers.RemoveRange(members);

			await db.SaveChangesAsync();
			return members;
		}
		
		public async Task<List<GroupMember>> CopyUsersFromOneGroupToAnother(int fromGroupId, int toGroupId, List<string> userIds)
		{
			var membersUserIds = db.GroupMembers.Where(m => m.GroupId == fromGroupId && userIds.Contains(m.UserId)).Select(m => m.UserId).ToList();
			var newMembers = new List<GroupMember>();
			foreach (var memberUserId in membersUserIds)
				newMembers.Add(await AddUserToGroup(toGroupId, memberUserId));
			
			return newMembers;
		}

		public Group FindGroupById(int groupId)
		{
			return db.Groups.FirstOrDefault(g => g.Id == groupId && !g.IsDeleted);
		}

		public Group FindGroupByInviteHash(Guid hash)
		{
			return db.Groups.FirstOrDefault(g => g.InviteHash == hash && !g.IsDeleted && g.IsInviteLinkEnabled);
		}

		public IEnumerable<Group> GetGroups(string courseId, bool includeArchived = false)
		{
			var groups = db.Groups.Where(g => g.CourseId == courseId && !g.IsDeleted);
			if (!includeArchived)
				groups = groups.Where(g => !g.IsArchived);
			return groups;
		}

		public bool IsGroupAvailableForUser(int groupId, ClaimsPrincipal user)
		{
			var group = FindGroupById(groupId);
			/* Course admins can see all groups */
			if (CanUserSeeAllCourseGroups(user, group.CourseId))
				return true;

			if (!user.HasAccessFor(group.CourseId, CourseRole.Instructor))
				return false;

			var userId = user.GetUserId();
			var hasAccess = db.GroupAccesses.Any(a => a.UserId == userId && a.GroupId == groupId && a.IsEnabled);
			return !group.IsDeleted && (group.OwnerId == userId || hasAccess);
		}

		public List<Group> GetAvailableForUserGroups(string courseId, ClaimsPrincipal user, bool includeArchived = false)
		{
			if (!user.HasAccessFor(courseId, CourseRole.Instructor))
				return new List<Group>();

			return GetAvailableForUserGroups(new List<string> { courseId }, user, includeArchived);
		}

		public List<Group> GetAvailableForUserGroups(List<string> coursesIds, ClaimsPrincipal user, bool includeArchived = false)
		{
			var coursesWhereUserCanSeeAllGroups = coursesIds.Where(id => CanUserSeeAllCourseGroups(user, id)).ToList();
			var otherCourses = new HashSet<string>(coursesIds).Except(coursesWhereUserCanSeeAllGroups).ToList();

			var userId = user.GetUserId();
			var groupsWithAccess = new HashSet<int>(db.GroupAccesses.Where(a => a.UserId == userId && a.IsEnabled).Select(a => a.GroupId));
			var groups = db.Groups.Where(g => !g.IsDeleted && (includeArchived || !g.IsArchived) &&
											(
												/* Course admins can see all groups */
												coursesWhereUserCanSeeAllGroups.Contains(g.CourseId) ||
												/* Other instructor can see only public or own groups */
												(otherCourses.Contains(g.CourseId) && (g.OwnerId == userId || groupsWithAccess.Contains(g.Id)))
											)
			);

			return groups
				.OrderBy(g => g.IsArchived)
				.ThenBy(g => g.OwnerId != userId)
				.ThenBy(g => g.Name)
				.ToList();
		}

		public Task<List<Group>> GetMyGroupsFilterAccessibleToUserAsync(string courseId, ClaimsPrincipal user, bool includeArchived = false)
		{
			return GetMyGroupsFilterAccessibleToUserAsync(courseId, user.GetUserId(), includeArchived);
		}

		public Task<List<Group>> GetMyGroupsFilterAccessibleToUserAsync(string courseId, string userId, bool includeArchived = false)
		{
			var accessableGroupsIds = db.GroupAccesses.Where(a => a.Group.CourseId == courseId && a.UserId == userId && a.IsEnabled).Select(a => a.GroupId);

			var groups = db.Groups.Where(g => g.CourseId == courseId && !g.IsDeleted && (g.OwnerId == userId || accessableGroupsIds.Contains(g.Id)));
			if (!includeArchived)
				groups = groups.Where(g => !g.IsArchived);
			return groups.ToListAsync();
		}

		public List<ApplicationUser> GetGroupMembersAsUsers(int groupId)
		{
			return db.GroupMembers.Where(m => m.GroupId == groupId).Select(m => m.User).ToList();
		}

		public List<GroupMember> GetGroupMembers(int groupId)
		{
			return db.GroupMembers.Include(m => m.User).Where(m => m.GroupId == groupId).ToList();
		}
		
		public Task<List<GroupMember>> GetGroupsMembersAsync(IEnumerable<int> groupsIds)
		{
			return db.GroupMembers.Include(m => m.User).Where(m => groupsIds.Contains(m.GroupId)).ToListAsync();
		}

		/* Instructor can view student if he is course admin or if student is member of one of accessable for instructor group */
		public async Task<bool> CanInstructorViewStudentAsync(ClaimsPrincipal instructor, string studentId)
		{
			if (instructor.HasAccess(CourseRole.CourseAdmin))
				return true;

			var coursesIds = courseManager.GetCourses().Select(c => c.Id).ToList();
			var groups = GetAvailableForUserGroups(coursesIds, instructor);
			var members = await GetGroupsMembersAsync(groups.Select(g => g.Id).ToList());
			return members.Select(m => m.UserId).Contains(studentId);
		}

		public Dictionary<string, List<Group>> GetUsersGroups(List<string> courseIds, IEnumerable<string> userIds, ClaimsPrincipal currentUser, int maxCount = 3)
		{
			var canSeeAllGroups = courseIds.ToDictionary(c => c.ToLower(), c => CanUserSeeAllCourseGroups(currentUser, c));
			var currentUserId = currentUser.GetUserId();

			var groupsWithAccess = new HashSet<int>(db.GroupAccesses.Where(a => a.UserId == currentUserId && a.IsEnabled).Select(a => a.GroupId));
			var usersGroups = db.GroupMembers
				.Where(m => userIds.Contains(m.UserId) && courseIds.Contains(m.Group.CourseId))
				.GroupBy(m => m.UserId)
				.ToDictionary(group => group.Key, group => group.ToList())
				.ToDictionary(
					kv => kv.Key,
					kv => kv.Value.Select(m => m.Group)
						.Distinct()
						.Where(g => (g.OwnerId == currentUserId || groupsWithAccess.Contains(g.Id) || canSeeAllGroups[g.CourseId.ToLower()]) && !g.IsDeleted && !g.IsArchived)
						.OrderBy(g => g.OwnerId != currentUserId)
						.Take(maxCount)
						.ToList()
				);

			return usersGroups;
		}

		public Dictionary<string, List<string>> GetUsersGroupsNames(List<string> courseIds, IEnumerable<string> userIds, ClaimsPrincipal currentUser, int maxCount = 3)
		{
			var usersGroups = GetUsersGroups(courseIds, userIds, currentUser, maxCount + 1);
			return usersGroups.ToDictionary(
				kv => kv.Key,
				kv => kv.Value.Select((g, idx) => idx >= maxCount ? "..." : g.Name).ToList());
		}

		public Dictionary<string, List<int>> GetUsersGroupsIds(List<string> courseIds, IEnumerable<string> userIds, ClaimsPrincipal currentUser, int maxCount = 3)
		{
			var usersGroups = GetUsersGroups(courseIds, userIds, currentUser, maxCount);
			return usersGroups.ToDictionary(
				kv => kv.Key,
				kv => kv.Value.Select(g => g.Id).ToList());
		}

		public Dictionary<string, string> GetUsersGroupsNamesAsStrings(List<string> courseIds, IEnumerable<string> userIds, ClaimsPrincipal currentUser, int maxCount = 3)
		{
			var usersGroups = GetUsersGroupsNames(courseIds, userIds, currentUser, maxCount);
			return usersGroups.ToDictionary(kv => kv.Key, kv => string.Join(", ", kv.Value));
		}

		public Dictionary<string, string> GetUsersGroupsNamesAsStrings(string courseId, IEnumerable<string> userIds, ClaimsPrincipal currentUser, int maxCount = 3)
		{
			return GetUsersGroupsNamesAsStrings(new List<string> { courseId }, userIds, currentUser, maxCount);
		}

		public string GetUserGroupsNamesAsString(List<string> courseIds, string userId, ClaimsPrincipal currentUser, int maxCount = 3)
		{
			var usersGroups = GetUsersGroupsNamesAsStrings(courseIds, new List<string> { userId }, currentUser, maxCount);
			return usersGroups.GetOrDefault(userId, "");
		}

		public string GetUserGroupsNamesAsString(string courseId, string userId, ClaimsPrincipal currentUser, int maxCount = 3)
		{
			return GetUserGroupsNamesAsString(new List<string> { courseId }, userId, currentUser, maxCount);
		}

		public async Task EnableInviteLink(int groupId, bool isEnabled)
		{
			var group = db.Groups.Find(groupId);
			if (group != null)
			{
				group.IsInviteLinkEnabled = isEnabled;
				await db.SaveChangesAsync();
			}
		}

		public IEnumerable<string> GetUsersIdsForAllGroups(string courseId)
		{
			var groupsIds = GetGroups(courseId).Select(g => g.Id);
			return db.GroupMembers.Where(m => groupsIds.Contains(m.GroupId)).Select(m => m.UserId);
		}

		public Dictionary<string, List<int>> GetUsersGroupsIds(string courseId, IEnumerable<string> usersIds)
		{
			var groupsIds = GetGroups(courseId).Select(g => g.Id);
			return db.GroupMembers
				.Where(m => groupsIds.Contains(m.GroupId) && usersIds.Contains(m.UserId))
				.GroupBy(m => m.UserId)
				.ToDictionary(g => g.Key, g => g.Select(m => m.GroupId).ToList());
		}

		public List<int> GetUserGroupsIds(string courseId, string userId)
		{
			return GetUsersGroupsIds(courseId, new List<string> { userId }).GetOrDefault(userId, new List<int>());
		}

		public List<Group> GetUserGroups(string courseId, string userId)
		{
			var userGroupsIds = GetUserGroupsIds(courseId, userId);
			return GetGroups(courseId).Where(g => userGroupsIds.Contains(g.Id)).ToList();
		}

		public bool IsManualCheckingEnabledForUser(Course course, string userId)
		{
			if (course.Settings.IsManualCheckingEnabled)
				return true;

			var userGroupsIds = db.GroupMembers
				.Where(m => m.Group.CourseId == course.Id && m.UserId == userId && !m.Group.IsDeleted)
				.Select(m => m.GroupId)
				.Distinct()
				.ToList();
			var userGroups = db.Groups.Where(g => userGroupsIds.Contains(g.Id)).ToList();
			return userGroups.Any(g => g.IsManualCheckingEnabled);
		}

		public async Task<bool> GetDefaultProhibitFutherReviewForUserAsync(string courseId, string userId, ClaimsPrincipal instructor)
		{
			var accessibleGroups = await GetMyGroupsFilterAccessibleToUserAsync(courseId, instructor);
			var userGroupsIds = await db.GroupMembers
				.Where(m => m.Group.CourseId == courseId && m.UserId == userId && !m.Group.IsDeleted)
				.Select(m => m.GroupId)
				.Distinct()
				.ToListAsync();

			var accessibleGroupsIds = accessibleGroups.Select(g => g.Id);			
			/* Return true if exists at least one group with enabled DefaultProhibitFutherReview */
			return await db.Groups.AnyAsync(
				g => accessibleGroupsIds.Contains(g.Id) &&
					userGroupsIds.Contains(g.Id) &&
					g.DefaultProhibitFutherReview
			);
		}

		public async Task EnableAdditionalScoringGroupsForGroup(int groupId, IEnumerable<string> scoringGroupsIds)
		{
			using (var transaction = db.Database.BeginTransaction())
			{
				db.EnabledAdditionalScoringGroups.RemoveRange(
					db.EnabledAdditionalScoringGroups.Where(e => e.GroupId == groupId)
				);

				foreach (var scoringGroupId in scoringGroupsIds)
					db.EnabledAdditionalScoringGroups.Add(new EnabledAdditionalScoringGroup
					{
						GroupId = groupId,
						ScoringGroupId = scoringGroupId
					});

				await db.SaveChangesAsync();

				transaction.Commit();
			}
		}

		public List<EnabledAdditionalScoringGroup> GetEnabledAdditionalScoringGroups(string courseId)
		{
			var groupsIds = GetGroups(courseId).Select(g => g.Id);
			return db.EnabledAdditionalScoringGroups.Where(e => groupsIds.Contains(e.GroupId)).ToList();
		}

		public List<EnabledAdditionalScoringGroup> GetEnabledAdditionalScoringGroupsForGroup(int groupId)
		{
			return db.EnabledAdditionalScoringGroups.Where(e => e.GroupId == groupId).ToList();
		}

		public List<GroupLabel> GetLabels(string ownerId)
		{
			return db.GroupLabels.Where(l => !l.IsDeleted && l.OwnerId == ownerId).ToList();
		}

		public async Task<GroupLabel> CreateLabel(string ownerId, string name, string colorHex)
		{
			var label = new GroupLabel
			{
				OwnerId = ownerId,
				Name = name,
				IsDeleted = false,
				ColorHex = colorHex
			};
			db.GroupLabels.Add(label);
			await db.SaveChangesAsync();
			return label;
		}

		public async Task AddLabelToGroup(int groupId, int labelId)
		{
			using (var transaction = db.Database.BeginTransaction())
			{
				if (db.LabelsOnGroups.Any(g => g.LabelId == labelId && g.GroupId == groupId))
					return;

				var labelOnGroup = new LabelOnGroup
				{
					GroupId = groupId,
					LabelId = labelId,
				};
				db.LabelsOnGroups.Add(labelOnGroup);
				await db.SaveChangesAsync();

				transaction.Commit();
			}
		}

		public async Task RemoveLabelFromGroup(int groupId, int labelId)
		{
			var labels = db.LabelsOnGroups.Where(g => g.LabelId == labelId && g.GroupId == groupId);
			db.LabelsOnGroups.RemoveRange(labels);
			await db.SaveChangesAsync();
		}

		public List<int> GetGroupsWithSpecificLabel(int labelId)
		{
			return db.LabelsOnGroups.Where(l => l.LabelId == labelId).Select(l => l.GroupId).ToList();
		}

		public DefaultDictionary<int, List<int>> GetGroupsLabels(IEnumerable<int> groupsIds)
		{
			var groupsIdsSet = new HashSet<int>(groupsIds);
			return db.LabelsOnGroups
				.Where(l => groupsIdsSet.Contains(l.GroupId))
				.GroupBy(l => l.GroupId)
				.ToDictionary(g => g.Key, g => g.Select(l => l.LabelId).ToList())
				.ToDefaultDictionary();
		}

		[CanBeNull]
		public GroupLabel FindLabelById(int labelId)
		{
			return db.GroupLabels.Find(labelId);
		}

		/* Group accesses */

		public async Task<GroupAccess> GrantAccess(int groupId, string userId, GroupAccessType accessType, string grantedById)
		{
			var currentAccess = db.GroupAccesses.FirstOrDefault(a => a.GroupId == groupId && a.UserId == userId);
			if (currentAccess == null)
			{
				currentAccess = new GroupAccess
				{
					GroupId = groupId,
					UserId = userId,
				};
				db.GroupAccesses.Add(currentAccess);
			}
			currentAccess.AccessType = accessType;
			currentAccess.GrantedById = grantedById;
			currentAccess.GrantTime = DateTime.Now;
			currentAccess.IsEnabled = true;

			await db.SaveChangesAsync();
			return db.GroupAccesses.Include(a => a.GrantedBy).Single(a => a.Id == currentAccess.Id);
		}

		public bool CanRevokeAccess(int groupId, string userId, ClaimsPrincipal revokedBy)
		{
			var revokedById = revokedBy.GetUserId();

			var group = FindGroupById(groupId);
			if (group.OwnerId == revokedById || revokedBy.HasAccessFor(group.CourseId, CourseRole.CourseAdmin))
				return true;
			return db.GroupAccesses.Any(a => a.GroupId == groupId && a.UserId == userId && a.GrantedById == revokedById && a.IsEnabled);
		}

		public async Task<List<GroupAccess>> RevokeAccess(int groupId, string userId)
		{
			var accesses = db.GroupAccesses.Where(a => a.GroupId == groupId && a.UserId == userId).ToList();
			foreach (var access in accesses)
				access.IsEnabled = false;

			await db.SaveChangesAsync();
			return accesses;
		}

		public List<GroupAccess> GetGroupAccesses(int groupId)
		{
			return db.GroupAccesses.Include(a => a.User).Where(a => a.GroupId == groupId && a.IsEnabled).ToList();
		}

		public DefaultDictionary<int, List<GroupAccess>> GetGroupsAccesses(IEnumerable<int> groupsIds)
		{
			return db.GroupAccesses.Include(a => a.User)
				.Where(a => groupsIds.Contains(a.GroupId) && a.IsEnabled)
				.GroupBy(a => a.GroupId)
				.ToDictionary(g => g.Key, g => g.ToList())
				.ToDefaultDictionary();
		}
	}
}