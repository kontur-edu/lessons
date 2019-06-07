using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models.Comments;
using Database.Repos.Comments;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Database.Core.Tests.Repos.Comments
{
	public class CommentsRepoTests : BaseRepoTests
	{
		private ICommentsRepo commentsRepo;
		
		[SetUp]
		public override void SetUp()
		{
			base.SetUp();

			commentsRepo = serviceProvider.GetService<ICommentsRepo>();
		}

		[Test]
		public async Task AddComment()
		{
			var userId = Guid.NewGuid().ToString();
			
			var comment = await commentsRepo.AddCommentAsync(userId, "courseId", Guid.NewGuid(), -1, false, "Comment text").ConfigureAwait(false);
			
			Assert.AreEqual("Comment text", comment.Text);
			Assert.AreEqual(-1, comment.ParentCommentId);
			Assert.IsTrue(comment.IsTopLevel);
			Assert.IsFalse(comment.IsPinnedToTop);
			Assert.IsFalse(comment.IsDeleted);
			Assert.IsFalse(comment.IsCorrectAnswer);
			Assert.AreEqual(userId, comment.AuthorId);
		}

		[Test]
		public async Task AddAndFindComment()
		{
			var user = await CreateUserAsync("test").ConfigureAwait(false);
			
			var comment = await commentsRepo.AddCommentAsync(user.Id, "courseId", Guid.NewGuid(), -1, false, "Comment text").ConfigureAwait(false);
			var foundComment = await commentsRepo.FindCommentByIdAsync(comment.Id).ConfigureAwait(false);
			
			Assert.AreEqual(comment, foundComment);
		}

		[Test]
		public async Task AddAndGetSlideComment()
		{
			var user = await CreateUserAsync("test").ConfigureAwait(false);
			var slideId = Guid.NewGuid();
			
			var comment = await commentsRepo.AddCommentAsync(user.Id, "courseId", slideId, -1, false, "Comment text").ConfigureAwait(false);
			var comments = await commentsRepo.GetSlideCommentsAsync("courseId", slideId).ConfigureAwait(false);
			
			Assert.AreEqual(1, comments.Count);
			CollectionAssert.AreEqual(new List<Comment> { comment }, comments);
		}
		
		[Test]
		public async Task AddAndGetMultipleCommentsFromOneSlide()
		{
			var slideId = Guid.NewGuid();

			const int commentsCount = 10;
			var comments = new List<Comment>();
			for (var i = 0; i < commentsCount; i++)
			{
				var user = await CreateUserAsync($"test{i}").ConfigureAwait(false);
				var comment = await commentsRepo.AddCommentAsync(user.Id, "courseId", slideId, -1, false, "Comment text").ConfigureAwait(false);
				comments.Add(comment);
			}
			var foundComments = await commentsRepo.GetSlideCommentsAsync("courseId", slideId).ConfigureAwait(false);

			Assert.AreEqual(commentsCount, foundComments.Count);
			CollectionAssert.AreEqual(comments, foundComments);
		}
		
		[Test]
		public async Task AddAndGetMultipleCommentsFromDifferentSlides()
		{
			const int commentsCount = 10;
			var comments = new List<Comment>();
			for (var i = 0; i < commentsCount; i++)
			{
				var user = await CreateUserAsync($"test{i}").ConfigureAwait(false);
				var slideId = Guid.NewGuid();
				var comment = await commentsRepo.AddCommentAsync(user.Id, "courseId", slideId, -1, false, "Comment text").ConfigureAwait(false);
				comments.Add(comment);
			}
			var foundComments = await commentsRepo.GetCourseCommentsAsync("courseId").ConfigureAwait(false);

			Assert.AreEqual(commentsCount, foundComments.Count);
			CollectionAssert.AreEqual(comments, foundComments);
		}
	}
}