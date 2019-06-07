using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Models;

namespace Database.Repos
{
	public interface IGradersRepo
	{
		GraderClient FindGraderClient(string courseId, Guid clientId);
		List<GraderClient> GetGraderClients(string courseId);
		Task<GraderClient> AddGraderClient(string courseId, string name);
		Task<ExerciseSolutionByGrader> AddSolutionFromGraderClient(Guid clientId, int submissionId, string clientUserId);
		ExerciseSolutionByGrader FindSolutionFromGraderClient(int solutionId);
		List<ExerciseSolutionByGrader> GetClientSolutions(GraderClient client, string search, int count, int offset = 0);
	}
}