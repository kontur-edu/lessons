using Ulearn.Common;

namespace uLearn.Web.Models
{
	public class QuizBlockData
	{
		public QuizModel QuizModel { get; private set; }
		public int BlockIndex { get; private set; }
		public QuizState QuizState { get; private set; }
		public DefaultDictionary<string, int> QuestionAnswersFrequency { get; private set; }
		public readonly bool IsInstructor;
		public readonly bool IsCourseAdmin;
		public readonly bool DebugView;

		public QuizBlockData(QuizModel model, int index, QuizState quizState, DefaultDictionary<string, int> questionAnswersFrequency=null, bool isInstructor=false, bool debugView=false, bool isCourseAdmin=false)
		{
			QuizModel = model;
			BlockIndex = index;
			QuizState = quizState;
			QuestionAnswersFrequency = questionAnswersFrequency ?? new DefaultDictionary<string, int>();
			IsCourseAdmin = isCourseAdmin;
			IsInstructor = isInstructor;
			DebugView = debugView;
		}

		private bool AttemptsLimitExceeded => QuizModel.QuizState.UsedAttemptsCount + 1 > QuizModel.MaxAttemptsCount; 

		public bool ShowCorrectAnswers
		{
			get
			{
				if (QuizModel.Slide.ManualChecking)
					return false;
				if (QuizState.Status == QuizStatus.Sent)
					return QuizState.IsScoredMaximum || AttemptsLimitExceeded || IsInstructor;
				return false;
			}
		}

		public bool ShowUserAnswers
		{
			get
			{
				if (QuizState.Status == QuizStatus.ReadyToSend)
					/* Show previous user's answers in slides with enabled manual checking */
					return QuizModel.Slide.ManualChecking;
				
				return true;
			}
		}

		public bool ShowExplanations => ShowCorrectAnswers;
		public bool ShowQuestionStatistics => !DebugView && IsCourseAdmin;
	}
}