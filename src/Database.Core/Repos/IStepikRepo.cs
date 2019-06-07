using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;
using Ulearn.Core.Courses.Slides;

namespace Database.Repos
{
	public interface IStepikRepo
	{
		string GetAccessToken(string userId);
		Task SaveAccessToken(string userId, string accessToken);
		Task<StepikExportProcess> AddExportProcess(string ulearnCourseId, int stepikCourseId, string ownerId, bool isInitialExport);
		Task MarkExportProcessAsFinished(StepikExportProcess process, bool isSuccess, string log);
		List<StepikExportSlideAndStepMap> GetStepsExportedFromSlide(string ulearnCourseId, int stepikCourseId, Guid ulearnSlideId);
		List<StepikExportSlideAndStepMap> GetStepsExportedFromCourse(string ulearnCourseId, int stepikCourseId);
		Task SetMapInfoAboutExportedSlide(string ulearnCourseId, int stepikCourseId, Slide slide, IEnumerable<int> stepsIds);
		string GetSlideXmlIndicatedChanges(Slide slide);
		StepikExportProcess FindExportProcess(int processId);
	}
}