using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;
using log4net.Core;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides;

namespace Database.Repos
{
	/* TODO (andgein): This repo is not fully migrated to .NET Core and EF Core */
	public class StepikRepo : IStepikRepo
	{
		private readonly UlearnDb db;
		private readonly ILogger logger;

		public StepikRepo(UlearnDb db, ILogger logger)
		{
			this.db = db;
			this.logger = logger;
		}

		[CanBeNull]
		public string GetAccessToken(string userId)
		{
			return db.StepikAccessTokens.OrderByDescending(t => t.AddedTime).FirstOrDefault(t => t.UserId == userId)?.AccessToken;
		}

		public Task SaveAccessToken(string userId, string accessToken)
		{
			var newToken = new StepikAccessToken
			{
				AccessToken = accessToken,
				UserId = userId,
				AddedTime = DateTime.Now
			};
			var oldUsersTokens = db.StepikAccessTokens.Where(t => t.UserId == userId).ToList();
			db.StepikAccessTokens.RemoveRange(oldUsersTokens);
			db.StepikAccessTokens.Add(newToken);
			return db.SaveChangesAsync();
		}

		public async Task<StepikExportProcess> AddExportProcess(string ulearnCourseId, int stepikCourseId, string ownerId, bool isInitialExport)
		{
			var process = new StepikExportProcess
			{
				UlearnCourseId = ulearnCourseId,
				StepikCourseId = stepikCourseId,
				StepikCourseTitle = "",
				IsInitialExport = isInitialExport,
				Log = "",
				OwnerId = ownerId,
				IsFinished = false,
				StartTime = DateTime.Now,
				FinishTime = null,
			};
			db.StepikExportProcesses.Add(process);
			await db.SaveChangesAsync().ConfigureAwait(false);
			return process;
		}

		public Task MarkExportProcessAsFinished(StepikExportProcess process, bool isSuccess, string log)
		{
			process.IsFinished = true;
			process.FinishTime = DateTime.Now;
			process.IsSuccess = isSuccess;
			process.Log = log;
			return db.SaveChangesAsync();
		}

		public List<StepikExportSlideAndStepMap> GetStepsExportedFromSlide(string ulearnCourseId, int stepikCourseId, Guid ulearnSlideId)
		{
			return db.StepikExportSlideAndStepMaps.Where(m => m.UlearnCourseId == ulearnCourseId && m.SlideId == ulearnSlideId && m.StepikCourseId == stepikCourseId).ToList();
		}

		public List<StepikExportSlideAndStepMap> GetStepsExportedFromCourse(string ulearnCourseId, int stepikCourseId)
		{
			return db.StepikExportSlideAndStepMaps.Where(m => m.UlearnCourseId == ulearnCourseId && m.StepikCourseId == stepikCourseId).ToList();
		}

		private Task RemoveMapInfoAboutExportedSlide(string ulearnCourseId, int stepikCourseId, Guid ulearnSlideId)
		{
			var maps = db.StepikExportSlideAndStepMaps.Where(m => m.UlearnCourseId == ulearnCourseId && m.SlideId == ulearnSlideId && m.StepikCourseId == stepikCourseId).ToList();
			db.StepikExportSlideAndStepMaps.RemoveRange(maps);
			return db.SaveChangesAsync();
		}

		public async Task SetMapInfoAboutExportedSlide(string ulearnCourseId, int stepikCourseId, Slide slide, IEnumerable<int> stepsIds)
		{
			await RemoveMapInfoAboutExportedSlide(ulearnCourseId, stepikCourseId, slide.Id).ConfigureAwait(false);

			var exportedSlideXml = GetSlideXmlIndicatedChanges(slide);
			foreach (var stepId in stepsIds)
			{
				db.StepikExportSlideAndStepMaps.Add(new StepikExportSlideAndStepMap
				{
					UlearnCourseId = ulearnCourseId,
					StepikCourseId = stepikCourseId,
					SlideId = slide.Id,
					StepId = stepId,
					SlideXml = exportedSlideXml,
				});
			}
			await db.SaveChangesAsync().ConfigureAwait(false);
		}

		public string GetSlideXmlIndicatedChanges(Slide slide)
		{
			return slide.XmlSerialize(removeWhitespaces: true);
		}

		[CanBeNull]
		public StepikExportProcess FindExportProcess(int processId)
		{
			return db.StepikExportProcesses.Find(processId);
		}
	}
}
