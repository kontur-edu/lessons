namespace uLearn.Web.Models
{
	public enum QuizStatus
	{
		ReadyToSend,
		Sent,
		
		WaitsForManualChecking,
		IsCheckingByInstructor,
	}

	public class QuizState
	{
		public QuizStatus Status { get; set; }
		
		public int UsedAttemptsCount { get; set; }
		
		public int Score { get; set; }
		
		public int MaxScore { get; set; }

		public QuizState(QuizStatus status, int usedAttemptsCount, int score, int maxScore)
		{
			Status = status;
			UsedAttemptsCount = usedAttemptsCount;
			Score = score;
			MaxScore = maxScore;
		}

		public bool IsScoredMaximum => Score == MaxScore;
	}
}