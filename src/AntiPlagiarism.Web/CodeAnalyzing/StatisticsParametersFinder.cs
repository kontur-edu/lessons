using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntiPlagiarism.Web.Database.Models;
using AntiPlagiarism.Web.Extensions;
using Serilog;

namespace AntiPlagiarism.Web.CodeAnalyzing
{
	public class StatisticsParametersFinder
	{
		private readonly ILogger logger;
		private readonly PlagiarismDetector plagiarismDetector;

		public StatisticsParametersFinder(ILogger logger, PlagiarismDetector plagiarismDetector)
		{
			this.logger = logger;
			this.plagiarismDetector = plagiarismDetector;
		}

		public async Task<(List<double>, TaskStatisticsParameters)> FindStatisticsParametersAsync(List<Submission> submissions)
		{
			var pairs = submissions.SelectMany(s => submissions, Tuple.Create).Where(pair => pair.Item1.Id < pair.Item2.Id).ToList();
			
			var weights = new List<double>();
			var pairIndex = 0;
			foreach (var (firstSubmission, secondSubmission) in pairs)
				weights.Add(await GetLinkWeightAsync(firstSubmission, secondSubmission, pairIndex++, pairs.Count).ConfigureAwait(false));
			
			/* Remove all 1's from weights list.
			   Ones are weights for pairs of absolutely identical solutions (i.e. copied from the internet or shared between students).
			   They are negatively affect to the mean and standard deviation of weights. So we decided just remove them from the array before 
			   calculation mean and deviation. See https://yt.skbkontur.ru/issue/ULEARN-78 for details. */
			weights = weights.Where(w => w < 1 - 1e-6).ToList();
			
			logger.Information($"Пересчитываю статистические параметры задачи (TaskStatisticsParameters) по следующему набору весов: [{string.Join(", ", weights)}]");
			
			var mean = weights.Mean();
			var deviation = weights.Deviation(mean);
			var tauCoefficient = GetTauCoefficient(submissions.Count);
			return (weights, new TaskStatisticsParameters
			{
				Mean = mean,
				Deviation = deviation,
				//TauCoefficient = tauCoefficient,
			});
		}

		public static double GetTauCoefficient(int count)
		{
			foreach (var (elementsCount, coefficient) in tauCoefficients)
				if (elementsCount >= count)
					return coefficient;
			return 2;
		}

		private Task<double> GetLinkWeightAsync(Submission first, Submission second, int index, int totalCount)
		{
			logger.Information($"Вычисляю коэффициент похожести решения #{first.Id} и #{second.Id} ({index} из {totalCount})");
			return plagiarismDetector.GetWeightAsync(first, second);
		}
		
		private static readonly List<(int, double)> tauCoefficients = new List<(int, double)>
		{
			(3, 1.1511), 
			(4, 1.4250), 
			(5, 1.5712), 
			(6, 1.6563), 
			(7, 1.7110), 
			(8, 1.7491), 
			(9, 1.7770), 
			(10, 1.7984), 
			(11, 1.8153), 
			(12, 1.8290), 
			(13, 1.8403), 
			(14, 1.8498), 
			(15, 1.8579), 
			(16, 1.8649), 
			(17, 1.8710), 
			(18, 1.8764), 
			(19, 1.8811), 
			(20, 1.8853), 
			(21, 1.8891), 
			(22, 1.8926), 
			(23, 1.8957), 
			(24, 1.8985), 
			(25, 1.9011), 
			(26, 1.9035), 
			(27, 1.9057), 
			(28, 1.9078), 
			(29, 1.9096), 
			(30, 1.9114), 
			(31, 1.9130), 
			(32, 1.9146), 
			(33, 1.9160), 
			(34, 1.9174), 
			(35, 1.9186), 
			(36, 1.9198), 
			(37, 1.9209), 
			(38, 1.9220), 
			(40, 1.9240), 
			(42, 1.9257), 
			(44, 1.9273), 
			(46, 1.9288), 
			(48, 1.9301), 
			(50, 1.9314), 
			(55, 1.9340), 
			(60, 1.9362), 
			(65, 1.9381), 
			(70, 1.9397), 
			(80, 1.9423), 
			(90, 1.9443), 
			(100, 1.9459), 
			(200, 1.9530), 
			(500, 1.9572), 
			(1000, 1.9586), 
			(5000, 1.9597),
		};
	}
}