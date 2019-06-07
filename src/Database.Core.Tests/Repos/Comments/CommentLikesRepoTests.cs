using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using Database.Repos;
using Database.Repos.Comments;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Database.Core.Tests.Repos.Comments
{
	[TestFixture]
	public class CommentLikesRepoTests : BaseRepoTests
	{
		private ICommentLikesRepo commentsLikesRepo;
		private ICommentsRepo commentsRepo;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();

			userManager = serviceProvider.GetService<UlearnUserManager>();
			commentsRepo = serviceProvider.GetService<ICommentsRepo>();
			commentsLikesRepo = serviceProvider.GetService<ICommentLikesRepo>();
		}

		[Test]
		public async Task LikeComment()
		{
			await commentsLikesRepo.LikeAsync(100, "user-id").ConfigureAwait(false);
			Assert.AreEqual(1, await commentsLikesRepo.GetLikesCountAsync(100).ConfigureAwait(false));
			Assert.AreEqual(0, await commentsLikesRepo.GetLikesCountAsync(200).ConfigureAwait(false));
		}

		[Test]
		public async Task RepeatLike()
		{
			await commentsLikesRepo.LikeAsync(100, "user-id").ConfigureAwait(false);
			Assert.AreEqual(1, await commentsLikesRepo.GetLikesCountAsync(100).ConfigureAwait(false));

			for (var i = 0; i < 10; i++)
			{
				await commentsLikesRepo.LikeAsync(100, "user-id").ConfigureAwait(false);
				Assert.AreEqual(1, await commentsLikesRepo.GetLikesCountAsync(100).ConfigureAwait(false));
			}
		}

		[Test]
		public async Task LikesFromDifferentUsers()
		{
			for (var i = 0; i < 10; i++)
			{
				await commentsLikesRepo.LikeAsync(100, Guid.NewGuid().ToString()).ConfigureAwait(false);
				Assert.AreEqual(i + 1, await commentsLikesRepo.GetLikesCountAsync(100).ConfigureAwait(false));
			}
		}

		[Test]
		public async Task LikesForDifferentComments()
		{
			for (var i = 0; i < 10; i++)
			{
				await commentsLikesRepo.LikeAsync(i, "user-id").ConfigureAwait(false);
				Assert.AreEqual(1, await commentsLikesRepo.GetLikesCountAsync(i).ConfigureAwait(false));
			}
		}
		
		[Test]
		public async Task LikeAndUnlikeComment()
		{
			await commentsLikesRepo.LikeAsync(100, "user-id").ConfigureAwait(false);
			await commentsLikesRepo.UnlikeAsync(100, "user-id").ConfigureAwait(false);
			Assert.AreEqual(0, await commentsLikesRepo.GetLikesCountAsync(100).ConfigureAwait(false));
		}

		[Test]
		public async Task UnlikeMultipleTimes()
		{
			await commentsLikesRepo.LikeAsync(100, "user-id").ConfigureAwait(false);
			await commentsLikesRepo.UnlikeAsync(100, "user-id").ConfigureAwait(false);
			await commentsLikesRepo.UnlikeAsync(100, "user-id").ConfigureAwait(false);
			await commentsLikesRepo.UnlikeAsync(100, "user-id").ConfigureAwait(false);
			await commentsLikesRepo.UnlikeAsync(100, "user-id").ConfigureAwait(false);
			Assert.AreEqual(0, await commentsLikesRepo.GetLikesCountAsync(100).ConfigureAwait(false));
		}

		[Test]
		public async Task GetLikes()
		{
			var user1 = await CreateUserAsync("user1").ConfigureAwait(false);
			var user2 = await CreateUserAsync("user2").ConfigureAwait(false);
			
			await commentsLikesRepo.LikeAsync(100, user1.Id).ConfigureAwait(false);
			await commentsLikesRepo.LikeAsync(100, user2.Id).ConfigureAwait(false);
			var likes = await commentsLikesRepo.GetLikesAsync(100).ConfigureAwait(false);
			Assert.AreEqual(2, likes.Count);
			CollectionAssert.AreEquivalent(new List<string> { user1.Id, user2.Id }, likes.Select(like => like.UserId));
		}

		[Test]
		public async Task GetLikesReturnsOrderedLikes()
		{
			const int usersCount = 10;
			const int commentId = 100;
			
			var users = new List<ApplicationUser>();
			for (var i = 0; i < usersCount; i++)
			{
				var user = await CreateUserAsync($"user{i}").ConfigureAwait(false);
				users.Add(user);
				await commentsLikesRepo.LikeAsync(commentId, user.Id).ConfigureAwait(false);
				await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
			}

			var likes = await commentsLikesRepo.GetLikesAsync(commentId).ConfigureAwait(false);
			Assert.AreEqual(usersCount, likes.Count);
			CollectionAssert.AreEqual(users.Select(u => u.Id), likes.Select(like => like.UserId));
		}
	}
}