using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;

namespace Database.DataContexts
{
	public class AdditionalScoresRepo
	{
		private readonly ULearnDb db;

		public AdditionalScoresRepo(ULearnDb db)
		{
			this.db = db;
		}

		/// <summary>
		/// Returns new AdditionalScore and old score (or null if not exists)
		/// </summary>
		public async Task<(AdditionalScore, int?)> SetAdditionalScore(string courseId, Guid unitId, string userId, string scoringGroupId, int score, string instructorId)
		{
			int? oldScore = null;
			using (var transaction = db.Database.BeginTransaction())
			{
				var scores = db.AdditionalScores.Where(s => s.CourseId == courseId && s.UnitId == unitId && s.UserId == userId && s.ScoringGroupId == scoringGroupId).ToList();
				if (scores.Any())
					oldScore = scores.First().Score;
				db.AdditionalScores.RemoveRange(scores);

				var additionalScore = new AdditionalScore
				{
					CourseId = courseId,
					UnitId = unitId,
					UserId = userId,
					ScoringGroupId = scoringGroupId,
					Score = score,
					InstructorId = instructorId,
					Timestamp = DateTime.Now,
				};
				db.AdditionalScores.Add(additionalScore);

				transaction.Commit();
				await db.SaveChangesAsync();

				return (additionalScore, oldScore);
			}
		}

		/* Dictionary<(unitId, scoringGroupId), additionalScore> */
		public Dictionary<Tuple<Guid, string>, int> GetAdditionalScoresForUser(string courseId, string userId)
		{
			return db.AdditionalScores
				.Where(s => s.CourseId == courseId && s.UserId == userId)
				.ToDictionary(s => Tuple.Create(s.UnitId, s.ScoringGroupId), s => s.Score);
		}

		public Dictionary<string, AdditionalScore> GetAdditionalScoresForUser(string courseId, Guid unitId, string userId)
		{
			return db.AdditionalScores
				.Where(s => s.CourseId == courseId && s.UnitId == unitId && s.UserId == userId)
				.ToDictionary(s => s.ScoringGroupId, s => s);
		}

		/* Dictionary<(userId, scoringGroupId), additionalScore> */
		public Dictionary<Tuple<string, string>, AdditionalScore> GetAdditionalScoresForUsers(string courseId, Guid unitId, IEnumerable<string> usersIds)
		{
			return db.AdditionalScores
				.Where(s => s.CourseId == courseId && s.UnitId == unitId && usersIds.Contains(s.UserId))
				.ToDictionary(s => Tuple.Create(s.UserId, s.ScoringGroupId), s => s);
		}

		/* Dictionary<(userId, unitId, scoringGroupId), additionalScore> */
		public Dictionary<Tuple<string, Guid, string>, AdditionalScore> GetAdditionalScoresForUsers(string courseId, IEnumerable<string> usersIds)
		{
			return db.AdditionalScores
				.Where(s => s.CourseId == courseId && usersIds.Contains(s.UserId))
				.ToDictionary(s => Tuple.Create(s.UserId, s.UnitId, s.ScoringGroupId), s => s);
		}

		public async Task RemoveAdditionalScores(string courseId, Guid unitId, string userId, string scoringGroupId)
		{
			var scores = db.AdditionalScores.Where(s => s.CourseId == courseId && s.UnitId == unitId && s.UserId == userId && s.ScoringGroupId == scoringGroupId);
			db.AdditionalScores.RemoveRange(scores);
			await db.SaveChangesAsync();
		}
	}
}