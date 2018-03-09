using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using JetBrains.Annotations;
using log4net;
using Microsoft.EntityFrameworkCore;
using uLearn;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace Database.Repos
{
	public class GradersRepo
	{
		private readonly UlearnDb db;
		private readonly ULearnUserManager userManager;

		private static readonly ILog log = LogManager.GetLogger(typeof(GradersRepo));

		public GradersRepo(UlearnDb db, ULearnUserManager userManager)
		{
			this.db = db;
			this.userManager = userManager;
		}

		[CanBeNull]
		public GraderClient FindGraderClient(string courseId, Guid clientId)
		{
			var client = db.GraderClients.Find(clientId);
			if (client == null || ! client.CourseId.EqualsIgnoreCase(courseId))
				return null;
			return client;
		}

		public List<GraderClient> GetGraderClients(string courseId)
		{
			return db.GraderClients.Where(c => c.CourseId == courseId).ToList();
		}

		public async Task<GraderClient> AddGraderClient(string courseId, string name)
		{
			var clientId = Guid.NewGuid();
			var user = new ApplicationUser { UserName = $"__grader_client_{clientId.GetNormalizedGuid()}__" };
			var password = StringUtils.GenerateSecureAlphanumericString(10);
			await userManager.CreateAsync(user, password);

			var client = new GraderClient
			{
				Id = clientId,
				CourseId = courseId,
				Name = name,
				User = user,
			};
			db.GraderClients.Add(client);
			await db.SaveChangesAsync();

			return client;
		}

		public async Task<ExerciseSolutionByGrader> AddSolutionFromGraderClient(Guid clientId, int submissionId, string clientUserId)
		{
			var exerciseSolutionByGrader = new ExerciseSolutionByGrader
			{
				ClientId = clientId,
				SubmissionId = submissionId,
				ClientUserId = clientUserId,
			};
			db.ExerciseSolutionsByGrader.Add(exerciseSolutionByGrader);
			await db.SaveChangesAsync();

			return exerciseSolutionByGrader;
		}

		public ExerciseSolutionByGrader FindSolutionFromGraderClient(int solutionId)
		{
			return db.ExerciseSolutionsByGrader.Find(solutionId);
		}

		public List<ExerciseSolutionByGrader> GetClientSolutions(GraderClient client, string search, int count, int offset = 0)
		{
			var solutions = db.ExerciseSolutionsByGrader.Where(s => s.ClientId == client.Id);
			if (!string.IsNullOrEmpty(search))
			{
				var solutionIds = GetGraderSolutionByClientUserId(search).Select(w => w.Id);
				solutions = solutions.Where(s => solutionIds.Contains(s.Id));
			}

			return solutions
				.OrderByDescending(s => s.Id)
				.Skip(offset)
				.Take(count)
				.ToList();
		}

		private IQueryable<SubmissionIdWrapper> GetGraderSolutionByClientUserId(string clientUserId)
		{
			if (string.IsNullOrEmpty(clientUserId))
				return db.ExerciseSolutionsByGrader.Select(s => new SubmissionIdWrapper(s.Id));

			var splittedUserId = clientUserId.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			var query = string.Join(" & ", splittedUserId.Select(s => "\"" + s.Trim().Replace("\"", "\\\"") + "*\""));
			return db.ExerciseSolutionsByGrader
				.FromSql("SELECT * FROM dbo.ExerciseSolutionByGraders WHERE CONTAINS([ClientUserId], {0})", query)
				.Select(s => new SubmissionIdWrapper(s.Id));
		}
	}

	/* System.String is not available for table-valued functions so we need to create ComplexTyped wrapper */
	[ComplexType]
	public class SubmissionIdWrapper
	{
		public SubmissionIdWrapper(int submissionId)
		{
			Id = submissionId;
		}

		public int Id { get; set; }
	}
}