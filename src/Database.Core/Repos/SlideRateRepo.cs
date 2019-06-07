using System;
using System.Linq;
using System.Threading.Tasks;
using Database.Models;
using uLearn;
using Ulearn.Core;

namespace Database.Repos
{
	/* TODO (andgein): This repo is not fully migrated to .NET Core and EF Core */
	public class SlideRateRepo : ISlideRateRepo
	{
		private readonly UlearnDb db;

		public SlideRateRepo(UlearnDb db)
		{
			this.db = db;
		}

		public async Task<string> AddRate(string courseId, Guid slideId, string userId, SlideRates rate)
		{
			var lastRate = db.SlideRates.FirstOrDefault(x => x.SlideId == slideId && x.UserId == userId && x.CourseId == courseId);
			if (lastRate == null)
			{
				db.SlideRates.Add(new SlideRate
				{
					Rate = rate,
					UserId = userId,
					SlideId = slideId,
					CourseId = courseId
				});
				await db.SaveChangesAsync();
				return "success";
			}
			if (lastRate.Rate == rate)
			{
				lastRate.Rate = SlideRates.NotWatched;
				await db.SaveChangesAsync();
				return "cancel";
			}
			lastRate.Rate = rate;
			await db.SaveChangesAsync();
			return "success";
		}

		public string FindRate(string courseId, Guid slideId, string userId)
		{
			var lastRate = db.SlideRates.FirstOrDefault(
				x => x.CourseId == courseId && x.SlideId == slideId && x.UserId == userId
			);
			return lastRate?.Rate.ToString();
		}

		public Rates GetRates(Guid slideId, string courseId)
		{
			var rates = new Rates();
			var allRates = db.SlideRates.Where(x => x.CourseId == courseId && x.SlideId == slideId).ToList();
			foreach (var rate in allRates)
			{
				if (rate.Rate == SlideRates.Good)
					rates.AddGood();
				if (rate.Rate == SlideRates.NotUnderstand)
					rates.AddNotUnderstand();
				if (rate.Rate == SlideRates.NotWatched)
					rates.AddNotWatched();
				if (rate.Rate == SlideRates.Trivial)
					rates.AddTrivial();
			}
			return rates;
		}

		public string GetUserRate(string courseId, Guid slideId, string userId)
		{
			return ConvertMarkToPrettyString(db.SlideRates.FirstOrDefault(x => x.CourseId == courseId && x.SlideId == slideId && x.UserId == userId));
		}

		private string ConvertMarkToPrettyString(SlideRate slideRate)
		{
			return slideRate == null
				? null
				: slideRate.Rate == SlideRates.Good
					? "Хорошо"
					: slideRate.Rate == SlideRates.NotUnderstand
						? "Не понял"
						: slideRate.Rate == SlideRates.Trivial
							? "Тривиально"
							: null;
		}
	}
}