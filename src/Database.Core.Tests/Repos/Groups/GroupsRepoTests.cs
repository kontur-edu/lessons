using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos.Groups;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Database.Core.Tests.Repos.Groups
{
	[TestFixture]
	public class GroupsRepoTests : BaseRepoTests
	{
		private GroupsRepo groupsRepo;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();

			var groupsCreatorAndCopier = serviceProvider.GetService<IGroupsCreatorAndCopier>();
			var manualCheckingsForOldSolutionsAdder = new Mock<IManualCheckingsForOldSolutionsAdder>();

			groupsRepo = new GroupsRepo(
				db, groupsCreatorAndCopier, manualCheckingsForOldSolutionsAdder.Object
			);
		}

		[Test]
		public async Task CreateGroup()
		{
			var group = await groupsRepo.CreateGroupAsync("CourseId", "GroupName", Guid.NewGuid().ToString()).ConfigureAwait(false);
			Assert.AreEqual(1, db.Groups.Count(), "Groups count should be equal to 1 after adding new group");
			
			Assert.AreEqual(group, await groupsRepo.FindGroupByIdAsync(group.Id).ConfigureAwait(false));
			
			Assert.AreEqual(false, group.IsArchived);
			Assert.AreEqual(false, group.IsDeleted);
			Assert.AreEqual(0, group.Members?.Count ?? 0);
			Assert.AreNotEqual(Guid.Empty, group.InviteHash);
			Assert.That(group.CreateTime, Is.EqualTo(DateTime.Now).Within(5).Seconds);
		}

		[Test]
		public async Task FindGroupByIdReturnsNullIfGroupIsNotFound()
		{
			Assert.IsNull(await groupsRepo.FindGroupByIdAsync(1000).ConfigureAwait(false));
		}

		[Test]
		public async Task FindGroupByInviteHash()
		{
			var group = await groupsRepo.CreateGroupAsync("CourseId", "GroupName", Guid.NewGuid().ToString()).ConfigureAwait(false);
			
			Assert.AreEqual(group, await groupsRepo.FindGroupByInviteHashAsync(group.InviteHash).ConfigureAwait(false));
		}

		[Test]
		public async Task ModifyGroup()
		{
			var group = await groupsRepo.CreateGroupAsync("CourseId", "GroupName", Guid.NewGuid().ToString()).ConfigureAwait(false);
			var groupId = group.Id; 
				
			group = await groupsRepo.ModifyGroupAsync(
				group.Id,
				"NewGroupName",
				newIsManualCheckingEnabled: true,
				newIsManualCheckingEnabledForOldSolutions: true,
				newDefaultProhibitFurtherReview: false,
				newCanUsersSeeGroupProgress: false
			).ConfigureAwait(false);
			
			Assert.AreEqual(groupId, group.Id);

			group = await groupsRepo.FindGroupByIdAsync(groupId).ConfigureAwait(false);
			
			Assert.IsNotNull(group);
			Assert.AreEqual("NewGroupName", group.Name);
			Assert.AreEqual(true, group.IsManualCheckingEnabled);
			Assert.AreEqual(true, group.IsManualCheckingEnabledForOldSolutions);
			Assert.AreEqual(false, group.DefaultProhibitFutherReview);
			Assert.AreEqual(false, group.CanUsersSeeGroupProgress);
		}

		[Test]
		public async Task DeleteGroup()
		{
			var group = await groupsRepo.CreateGroupAsync("CourseId", "GroupName", Guid.NewGuid().ToString()).ConfigureAwait(false);
			Assert.IsNotNull(await groupsRepo.FindGroupByIdAsync(group.Id).ConfigureAwait(false));
			
			await groupsRepo.DeleteGroupAsync(group.Id).ConfigureAwait(false);
			Assert.IsNull(await groupsRepo.FindGroupByIdAsync(group.Id).ConfigureAwait(false));
		}

		[Test]
		public async Task ChangeGroupOwner()
		{
			var oldOwnerId = Guid.NewGuid().ToString();
			var group = await groupsRepo.CreateGroupAsync("CourseId", "GroupName", oldOwnerId).ConfigureAwait(false);
			Assert.AreEqual(oldOwnerId, group.OwnerId);

			var newOwnerId = Guid.NewGuid().ToString();
			await groupsRepo.ChangeGroupOwnerAsync(group.Id, newOwnerId).ConfigureAwait(false);
			group = await groupsRepo.FindGroupByIdAsync(group.Id).ConfigureAwait(false);
			Assert.IsNotNull(group);
			Assert.AreEqual(newOwnerId, group.OwnerId);
		}

		[Test]
		public async Task ArchiveGroup()
		{
			var group = await groupsRepo.CreateGroupAsync("CourseId", "GroupName", Guid.NewGuid().ToString()).ConfigureAwait(false);
			Assert.IsFalse(group.IsArchived);

			for (var i = 0; i < 3; i++)
			{
				await groupsRepo.ArchiveGroupAsync(group.Id, true).ConfigureAwait(false);
				group = await groupsRepo.FindGroupByIdAsync(group.Id).ConfigureAwait(false);
				Assert.IsNotNull(group);
				Assert.IsTrue(group.IsArchived);
			}
			
			for (var i = 0; i < 3; i++)
			{
				await groupsRepo.ArchiveGroupAsync(group.Id, false).ConfigureAwait(false);
				group = await groupsRepo.FindGroupByIdAsync(group.Id).ConfigureAwait(false);
				Assert.IsNotNull(group);
				Assert.IsFalse(group.IsArchived);
			}
		}

		[Test]
		public async Task GetCourseGroups()
		{
			var ownerId = Guid.NewGuid().ToString();
			var group1 = await groupsRepo.CreateGroupAsync("CourseId1", "Group1", ownerId).ConfigureAwait(false);
			var group2 = await groupsRepo.CreateGroupAsync("CourseId1", "Group2", ownerId).ConfigureAwait(false);
			var group3 = await groupsRepo.CreateGroupAsync("CourseId2", "Group3", ownerId).ConfigureAwait(false);
			var group4 = await groupsRepo.CreateGroupAsync("CourseId2", "Group4", ownerId).ConfigureAwait(false);
			var group5 = await groupsRepo.CreateGroupAsync("CourseId1", "Group5", ownerId).ConfigureAwait(false);

			var course1Groups = await groupsRepo.GetCourseGroupsAsync("CourseId1").ConfigureAwait(false);
			CollectionAssert.AreEqual(new List<Group> { group1, group2, group5}, course1Groups);

			await groupsRepo.ArchiveGroupAsync(group5.Id, isArchived: true).ConfigureAwait(false);
			
			var nonArchivedGroups = await groupsRepo.GetCourseGroupsAsync("CourseId1").ConfigureAwait(false);
			CollectionAssert.AreEqual(new List<Group> { group1, group2}, nonArchivedGroups);
			
			var allGroupsIncludeArchived = await groupsRepo.GetCourseGroupsAsync("CourseId1", includeArchived: true).ConfigureAwait(false);
			CollectionAssert.AreEqual(new List<Group> { group1, group2, group5 }, allGroupsIncludeArchived);
		}
	}
}