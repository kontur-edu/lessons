using AntiPlagiarism.Web.CodeAnalyzing;
using NUnit.Framework;

namespace AntiPlagiarism.Tests.CodeAnalyzing
{
	[TestFixture]
	public class StatisticsParametersFinder_should
	{
		[Test]
		public void ReturnCorrectTauCoefficient()
		{
			Assert.AreEqual(1.7984, StatisticsParametersFinder.GetTauCoefficient(10), 1e-6);
			Assert.AreEqual(1.928, StatisticsParametersFinder.GetTauCoefficient(45), 1e-3);
			Assert.AreEqual(1.9459, StatisticsParametersFinder.GetTauCoefficient(100), 1e-6);
		}
	}
}