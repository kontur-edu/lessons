using Ulearn.Core.Configuration;

namespace AntiPlagiarism.Web.Configuration
{
	public class AntiPlagiarismConfiguration : AbstractConfiguration
	{
		public int SnippetTokensCount { get; set; }
		
		public int MaxCodeLength { get; set; }
		
		public StatisticsAnalyzingConfiguration StatisticsAnalyzing { get; set; }
		
		public PlagiarismDetectorConfiguration PlagiarismDetector { get; set; }
		
		public ActionsConfiguration Actions { get; set; }
	}

	public class StatisticsAnalyzingConfiguration
	{
		public int CountOfLastAuthorsForCalculatingMeanAndDeviation { get; set; }
		
		public int RecalculateStatisticsAfterSubmissionsCount { get; set; }
		
		public int FaintSuspicionCoefficient { get; set; }
		
		public int StrongSuspicionCoefficient { get; set; }
		
		public double MinFaintSuspicionLevel { get; set; }
		
		public double MinStrongSuspicionLevel { get; set; }
		
		public double MaxFaintSuspicionLevel { get; set; }
		
		public double MaxStrongSuspicionLevel { get; set; }
	}

	public class PlagiarismDetectorConfiguration
	{
		public int CountOfColdestSnippetsUsedToFirstSearch { get; set; }
		
		public int CountOfColdestSnippetsUsedToSecondSearch { get; set; }
		
		public int MaxSubmissionsAfterFirstSearch { get; set; }

		public int SnippetAuthorsCountThreshold { get; set; }		
	}

	public class ActionsConfiguration
	{
		public GetAuthorPlagiarismsConfiguration GetAuthorPlagiarisms { get; set; }
	}
	
	public class GetAuthorPlagiarismsConfiguration
	{
		public int MaxLastSubmissionsCount { get; set; }
	}
}