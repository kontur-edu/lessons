using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using uLearn;
using Ulearn.Common;
using Ulearn.Core;

namespace Database.Repos
{
	/* TODO (andgein): This repo is not fully migrated to .NET Core and EF Core */
	public class XQueueRepo : IXQueueRepo
	{
		private readonly UlearnDb db;
		private readonly UlearnUserManager userManager;
		private readonly IUserSolutionsRepo userSolutionsRepo;

		public XQueueRepo(UlearnDb db, UlearnUserManager userManager, IUserSolutionsRepo userSolutionsRepo)
		{
			this.db = db;
			this.userManager = userManager;
			this.userSolutionsRepo = userSolutionsRepo;
		}

		public async Task AddXQueueWatcher(string name, string baseUrl, string queueName, string username, string password)
		{
			var user = new ApplicationUser { UserName = $"__xqueue_watcher_{new Guid().GetNormalizedGuid()}__" };
			var userPassword = StringUtils.GenerateSecureAlphanumericString(10);
			await userManager.CreateAsync(user, userPassword);
			var watcher = new XQueueWatcher
			{
				Name = name,
				BaseUrl = baseUrl,
				QueueName = queueName,
				UserName = username,
				Password = password,
				IsEnabled = true,
				User = user,
			};
			db.XQueueWatchers.Add(watcher);

			await db.SaveChangesAsync();
		}

		public List<XQueueWatcher> GetXQueueWatchers()
		{
			return db.XQueueWatchers.Where(w => w.IsEnabled).ToList();
		}

		[CanBeNull]
		public XQueueExerciseSubmission FindXQueueSubmission(UserExerciseSubmission submission)
		{
			return db.XQueueExerciseSubmissions.FirstOrDefault(s => s.SubmissionId == submission.Id);
		}

		public async Task AddXQueueSubmission(XQueueWatcher watcher, string xQueueHeader, string courseId, Guid slideId, string code)
		{
			var submission = await userSolutionsRepo.AddUserExerciseSubmission(
				courseId, slideId, code, null, null, watcher.UserId,
				"uLearn", $"XQueue watcher {watcher.Name}"
			).ConfigureAwait(false);
			db.XQueueExerciseSubmissions.Add(new XQueueExerciseSubmission
			{
				SubmissionId = submission.Id,
				WatcherId = watcher.Id,
				XQueueHeader = xQueueHeader,
				IsResultSent = false,
			});

			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public async Task MarkXQueueSubmissionThatResultIsSent(XQueueExerciseSubmission submission)
		{
			submission.IsResultSent = true;
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public List<XQueueExerciseSubmission> GetXQueueSubmissionsReadyToSentResults(XQueueWatcher watcher)
		{
			return db.XQueueExerciseSubmissions
					.Include(s => s.Submission.AutomaticChecking)
					.Where(s => s.WatcherId == watcher.Id && !s.IsResultSent && s.Submission.AutomaticChecking.Status == AutomaticExerciseCheckingStatus.Done)
					.ToList();
		}
	}
}