using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using LtiLibrary.NetCore.Lti.v1;
using Newtonsoft.Json;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace Database.Repos
{
	/* TODO (andgein): This repo is not fully migrated to .NET Core and EF Core */
	public class LtiRequestsRepo : ILtiRequestsRepo
	{
		private readonly UlearnDb db;
		private readonly JsonSerializer serializer;

		public LtiRequestsRepo(UlearnDb db)
		{
			this.db = db;
			serializer = new JsonSerializer();
		}
		
		public Task Update(string courseId, string userId, Guid slideId, string ltiRequestJson)
		{
			return FuncUtils.TrySeveralTimesAsync(() => TryUpdate(courseId, slideId, userId, ltiRequestJson), 3);
		}

		private Task TryUpdate(string courseId, Guid slideId, string userId, string ltiRequestJson)
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

			db.AddOrUpdate(ltiRequestModel, r => r.RequestId == ltiRequestModel.RequestId);
			return db.SaveChangesAsync();
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