using System;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos.CourseRoles;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Ulearn.Common.Extensions;

namespace Database.Repos.Groups
{
	/* This class is fully migrated to .NET Core and EF Core */
	public class GroupsCreatorAndCopier : IGroupsCreatorAndCopier
	{
		private readonly UlearnDb db;
		private readonly ILogger logger;
		private readonly ICourseRoleUsersFilter courseRoleUsersFilter;
		private readonly IManualCheckingsForOldSolutionsAdder manualCheckingsForOldSolutionsAdder;

		public GroupsCreatorAndCopier(UlearnDb db, ILogger logger, ICourseRoleUsersFilter courseRoleUsersFilter, IManualCheckingsForOldSolutionsAdder manualCheckingsForOldSolutionsAdder)
		{
			this.db = db;
			this.logger = logger;
			this.courseRoleUsersFilter = courseRoleUsersFilter;
			this.manualCheckingsForOldSolutionsAdder = manualCheckingsForOldSolutionsAdder;
		}

		public async Task<Group> CreateGroupAsync(
			string courseId,
			string name,
			string ownerId,
			bool isManualCheckingEnabled=false,
			bool isManualCheckingEnabledForOldSolutions=false,
			bool canUsersSeeGroupProgress=true,
			bool defaultProhibitFurtherReview=true,
			bool isInviteLinkEnabled=true)
		{
			logger.Information($"Создаю новую группу в курсе {courseId}: «{name}»");
			var group = new Group
			{
				CourseId = courseId,
				Name = name,
				OwnerId = ownerId,
				CreateTime = DateTime.Now,
				
				IsManualCheckingEnabled = isManualCheckingEnabled,
				IsManualCheckingEnabledForOldSolutions = isManualCheckingEnabledForOldSolutions,
				CanUsersSeeGroupProgress = canUsersSeeGroupProgress,
				DefaultProhibitFutherReview = defaultProhibitFurtherReview,
				
				InviteHash = Guid.NewGuid(),
				IsInviteLinkEnabled = isInviteLinkEnabled,
			};
			db.Groups.Add(group);
			await db.SaveChangesAsync().ConfigureAwait(false);

			return group;
		}
		
		/* Copy group from one course to another. Replace owner only if newOwnerId is not empty */
		public async Task<Group> CopyGroupAsync(Group group, string courseId, string newOwnerId=null)
		{
			logger.Information($"Копирую группу «{group.Name}» (id={group.Id}) в курс {courseId}");
			
			var newGroup = await CopyGroupWithoutMembersAsync(group, courseId, newOwnerId).ConfigureAwait(false);
			await CopyGroupMembersAsync(group, newGroup).ConfigureAwait(false);
			await CopyGroupAccessesAsync(group, newGroup).ConfigureAwait(false);

			/* We can also copy group's scoring-group settings if they are in one course */
			if (group.CourseId.EqualsIgnoreCase(courseId))
			{
				await CopyEnabledAdditionalScoringGroupsAsync(group, newGroup).ConfigureAwait(false);
			}

			return newGroup;
		}

		private Task<Group> CopyGroupWithoutMembersAsync(Group group, string courseId, string newOwnerId)
		{
			var newName = group.Name;
			if (courseId == group.CourseId)
				newName += " (копия)";

			return CreateGroupAsync(
				courseId,
				newName,
				string.IsNullOrEmpty(newOwnerId) ? group.OwnerId : newOwnerId,
				group.IsManualCheckingEnabled,
				group.IsManualCheckingEnabledForOldSolutions,
				group.CanUsersSeeGroupProgress,
				group.DefaultProhibitFutherReview,
				group.IsInviteLinkEnabled
			);
		}

		private async Task CopyGroupMembersAsync(Group group, Group newGroup)
		{
			logger.Information($"Копирую участников из группы «{group.Name}» (id={group.Id}) в группу «{newGroup.Name}» (id={newGroup.Id})");
			var members = group.NotDeletedMembers.Select(m => new GroupMember
			{
				UserId = m.UserId,
				GroupId = newGroup.Id,
				AddingTime = DateTime.Now,
			}).ToList();
			db.GroupMembers.AddRange(members);

			if (newGroup.IsManualCheckingEnabledForOldSolutions)
			{
				logger.Information($"Добавляю старые решения студентов в очередь на проверку");
				await manualCheckingsForOldSolutionsAdder.AddManualCheckingsForOldSolutionsAsync(newGroup.CourseId, members.Select(m => m.UserId).ToList()).ConfigureAwait(false);
			}

			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		private async Task CopyGroupAccessesAsync(Group group, Group newGroup)
		{
			logger.Information($"Копирую доступы к группе «{group.Name}» (id={group.Id}) в группу «{newGroup.Name}» (id={newGroup.Id})");
			var accesses = await db.GroupAccesses.Where(a => a.GroupId == group.Id && a.IsEnabled).ToListAsync().ConfigureAwait(false);
			var courseInstructorsIds = await courseRoleUsersFilter.GetListOfUsersWithCourseRoleAsync(CourseRoleType.Instructor, newGroup.CourseId, includeHighRoles: true).ConfigureAwait(false);
			foreach (var access in accesses)
			{
				if (! courseInstructorsIds.Contains(access.UserId))
					continue;
				if (newGroup.OwnerId == access.UserId)
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

			await db.SaveChangesAsync().ConfigureAwait(false);
		}
		
		private async Task CopyEnabledAdditionalScoringGroupsAsync(Group group, Group newGroup)
		{
			logger.Information($"Копирую включенные scoring-group-ы из группы «{group.Name}» (id={group.Id}) в группу «{newGroup.Name}» (id={newGroup.Id})");
			
			var enabledAdditionalScoringGroups = await db.EnabledAdditionalScoringGroups
				.Where(s => s.GroupId == group.Id)
				.Select(s => s.ScoringGroupId)
				.ToListAsync()
				.ConfigureAwait(false);
			
			foreach (var scoringGroupId in enabledAdditionalScoringGroups)
				db.EnabledAdditionalScoringGroups.Add(new EnabledAdditionalScoringGroup
				{
					GroupId = newGroup.Id,
					ScoringGroupId = scoringGroupId,
				});

			await db.SaveChangesAsync().ConfigureAwait(false);
		}
	}
}