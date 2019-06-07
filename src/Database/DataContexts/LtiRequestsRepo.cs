using System;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using EntityFramework.Functions;
using LtiLibrary.Core.Lti1;
using Newtonsoft.Json;
using Ulearn.Common;

namespace Database.DataContexts
{
	public class LtiRequestsRepo
	{
		private readonly ULearnDb db;
		private readonly JsonSerializer serializer;

		public LtiRequestsRepo(ULearnDb db)
		{
			this.db = db;
			serializer = new JsonSerializer();
		}

		public async Task Update(string courseId, string userId, Guid slideId, string ltiRequestJson)
		{
			await FuncUtils.TrySeveralTimesAsync(() => TryUpdate(courseId, slideId, userId, ltiRequestJson), 3);
		}

		private async Task TryUpdate(string courseId, Guid slideId, string userId, string ltiRequestJson)
		{
			var ltiRequestModel = FindElement(courseId, slideId, userId);

			if (ltiRequestModel == null)
			{
				ltiRequestModel = new LtiSlideRequest
				{
					CourseId = courseId,
					SlideId = slideId,
					UserId = userId,
					Request = ltiRequestJson
				};
			}
			else
				ltiRequestModel.Request = ltiRequestJson;

			db.LtiRequests.AddOrUpdate(ltiRequestModel);
			await db.ObjectContext().SaveChangesAsync(SaveOptions.DetectChangesBeforeSave);
			db.ObjectContext().AcceptAllChanges();
		}

		public LtiRequest Find(string courseId, string userId, Guid slideId)
		{
			var ltiRequestModel = FindElement(courseId, slideId, userId);
			if (ltiRequestModel == null)
				return null;

			return serializer.Deserialize<LtiRequest>(new JsonTextReader(new StringReader(ltiRequestModel.Request)));
		}

		private LtiSlideRequest FindElement(string courseId, Guid slideId, string userId)
		{
			return db.LtiRequests.FirstOrDefault(
				request => request.CourseId == courseId && request.UserId == userId && request.SlideId == slideId
			);
		}
	}
}