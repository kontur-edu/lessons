using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;

namespace Database.DataContexts
{
	public class SlideHintRepo
	{
		private readonly ULearnDb db;

		public SlideHintRepo(ULearnDb db)
		{
			this.db = db;
		}

		public async Task AddHint(string userId, int hintId, string courseId, Guid slideId)
		{
			if (db.Hints.Any(x => x.UserId == userId && x.HintId == hintId && x.SlideId == slideId && x.CourseId == courseId))
				return;
			db.Hints.Add(new SlideHint
			{
				UserId = userId,
				HintId = hintId,
				CourseId = courseId,
				SlideId = slideId
			});
			await db.SaveChangesAsync();
		}

		public IEnumerable<int> GetUsedHintId(string userId, string courseId, Guid slideId)
		{
			return db.Hints.Where(x => x.SlideId == slideId && x.UserId == userId && x.CourseId == courseId).Select(x => x.HintId);
		}

		public int GetHintsCount(Guid slideId, string courseId)
		{
			return db.Hints.Count(x => x.SlideId == slideId && x.CourseId == courseId);
		}

		public int GetHintsCountForUser(Guid slideId, string courseId, string userId)
		{
			return db.Hints.Count(x => x.SlideId == slideId && x.UserId == userId && x.CourseId == courseId);
		}

		public int GetHintUsedPercent(Guid slideId, string courseId, int hintsCountOnSlide, int usersCount)
		{
			var hintsCount = GetHintsCount(slideId, courseId);
			var maxPossibleHintsCount = hintsCountOnSlide * usersCount;
			return (int)(100 * (double)hintsCount / maxPossibleHintsCount);
		}

		public int GetHintUsedPercentForUser(string courseId, Guid slideId, string userId, int hintsCountOnSlide)
		{
			var hintsCount = GetHintsCountForUser(slideId, courseId, userId);
			return (int)(100 * (double)hintsCount / hintsCountOnSlide);
		}

		public async Task<string> LikeHint(string courseId, Guid slideId, int hintId, string userId)
		{
			var hint = db.Hints.FirstOrDefault(x => x.CourseId == courseId && x.SlideId == slideId && x.UserId == userId && x.HintId == hintId);
			if (hint == null)
				return "error";
			if (hint.IsHintHelped)
			{
				hint.IsHintHelped = false;
				await db.SaveChangesAsync();
				return "cancel";
			}
			hint.IsHintHelped = true;
			await db.SaveChangesAsync();
			return "success";
		}

		public HashSet<int> GetLikedHints(string courseId, Guid slideId, string userId)
		{
			return
				new HashSet<int>(db.Hints
					.Where(x => x.CourseId == courseId && x.SlideId == slideId && x.UserId == userId && x.IsHintHelped)
					.Select(x => x.HintId));
		}

		public bool IsHintLiked(string courseId, Guid slideId, string userId, int hintId)
		{
			return db.Hints.Any(
				x => x.CourseId == courseId && x.SlideId == slideId && x.UserId == userId && x.HintId == hintId && x.IsHintHelped
			);
		}

		public int GetUsedHintsCount(string courseId, Guid slideId, string userId)
		{
			return db.Hints.Count(x => x.CourseId == courseId && x.SlideId == slideId && x.UserId == userId );
		}
	}
}