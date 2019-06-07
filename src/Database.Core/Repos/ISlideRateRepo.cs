using System;
using System.Threading.Tasks;
using Database.Models;
using uLearn;
using Ulearn.Core;

namespace Database.Repos
{
	public interface ISlideRateRepo
	{
		Task<string> AddRate(string courseId, Guid slideId, string userId, SlideRates rate);
		string FindRate(string courseId, Guid slideId, string userId);
		Rates GetRates(Guid slideId, string courseId);
		string GetUserRate(string courseId, Guid slideId, string userId);
	}
}