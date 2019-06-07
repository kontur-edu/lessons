using System;
using System.Threading.Tasks;
using LtiLibrary.NetCore.Lti.v1;

namespace Database.Repos
{
	public interface ILtiRequestsRepo
	{
		Task Update(string courseId, string userId, Guid slideId, string ltiRequestJson);
		LtiRequest Find(string courseId, string userId, Guid slideId);
	}
}