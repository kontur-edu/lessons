namespace Ulearn.Core
{
	public class AnalyticsTableInfo
	{
		public int VisitersCount { get; set; }
		public int SolversPercent { get; set; }
		public Rates Rates { get; set; }
		public bool IsExercise { get; set; }
		public bool IsQuiz { get; set; }
		public int TotalHintCount { get; set; }
		public int HintUsedPercent { get; set; }
		public int SuccessQuizPercentage { get; set; }
	}

	public class Rates
	{
		public int Good { get; private set; }
		public int NotUnderstand { get; private set; }
		public int Trivial { get; private set; }
		public int NotWatched { get; private set; }

		public int Count
		{
			get { return Good + NotUnderstand + NotWatched + Trivial; }
		}

		public Rates()
		{
			Good = 0;
			NotUnderstand = 0;
			NotWatched = 0;
			Trivial = 0;
		}

		public void AddGood()
		{
			Good++;
		}

		public void AddNotUnderstand()
		{
			NotUnderstand++;
		}

		public void AddTrivial()
		{
			Trivial++;
		}

		public void AddNotWatched()
		{
			NotWatched++;
		}
	}
}