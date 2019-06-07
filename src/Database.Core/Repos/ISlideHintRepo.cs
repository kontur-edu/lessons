using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repos
{
	public interface ISlideHintRepo
	{
		Task AddHint(string userId, int hintId, string courseId, Guid slideId);
		IEnumerable<int> GetUsedHintId(string userId, string courseId, Guid slideId);
		int GetHintsCount(Guid slideId, string courseId);
		int GetHintsCountForUser(Guid slideId, string courseId, string userId);
		int GetHintUsedPercent(Guid slideId, string courseId, int hintsCountOnSlide, int usersCount);
		int GetHintUsedPercentForUser(string courseId, Guid slideId, string userId, int hintsCountOnSlide);
		Task<string> LikeHint(string courseId, Guid slideId, int hintId, string userId);
		HashSet<int> GetLikedHints(string courseId, Guid slideId, string userId);
		bool IsHintLiked(string courseId, Guid slideId, string userId, int hintId);
		int GetUsedHintsCount(string courseId, Guid slideId, string userId);
	}
}