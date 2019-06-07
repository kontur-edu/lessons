using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;
using log4net;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides;

namespace Database.DataContexts
{
	public class StepikRepo
	{
		private readonly ULearnDb db;

		private static readonly ILog log = LogManager.GetLogger(typeof(StepikRepo));

		public StepikRepo(ULearnDb db)
		{
			this.db = db;
		}

		[CanBeNull]
		public string GetAccessToken(string userId)
		{
			return db.StepikAccessTokens.OrderByDescending(t => t.AddedTime).FirstOrDefault(t => t.UserId == userId)?.AccessToken;
		}

		public async Task SaveAccessToken(string userId, string accessToken)
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
			await db.SaveChangesAsync();
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
			await db.SaveChangesAsync();
			return process;
		}

		public async Task MarkExportProcessAsFinished(StepikExportProcess process, bool isSuccess, string log)
		{
			process.IsFinished = true;
			process.FinishTime = DateTime.Now;
			process.IsSuccess = isSuccess;
			process.Log = log;
			await db.SaveChangesAsync();
		}

		public List<StepikExportSlideAndStepMap> GetStepsExportedFromSlide(string ulearnCourseId, int stepikCourseId, Guid ulearnSlideId)
		{
			return db.StepikExportSlideAndStepMaps.Where(m => m.UlearnCourseId == ulearnCourseId && m.SlideId == ulearnSlideId && m.StepikCourseId == stepikCourseId).ToList();
		}

		public List<StepikExportSlideAndStepMap> GetStepsExportedFromCourse(string ulearnCourseId, int stepikCourseId)
		{
			return db.StepikExportSlideAndStepMaps.Where(m => m.UlearnCourseId == ulearnCourseId && m.StepikCourseId == stepikCourseId).ToList();
		}

		private async Task RemoveMapInfoAboutExportedSlide(string ulearnCourseId, int stepikCourseId, Guid ulearnSlideId)
		{
			var maps = db.StepikExportSlideAndStepMaps.Where(m => m.UlearnCourseId == ulearnCourseId && m.SlideId == ulearnSlideId && m.StepikCourseId == stepikCourseId).ToList();
			db.StepikExportSlideAndStepMaps.RemoveRange(maps);
			await db.SaveChangesAsync();
		}

		public async Task SetMapInfoAboutExportedSlide(string ulearnCourseId, int stepikCourseId, Slide slide, IEnumerable<int> stepsIds)
		{
			await RemoveMapInfoAboutExportedSlide(ulearnCourseId, stepikCourseId, slide.Id);

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
			await db.SaveChangesAsync();
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
