using System;
using System.Linq;
using uLearn.Model;

namespace uLearn.Quizes
{
	public class QuizSlide : Slide
	{
		public QuizSlide(SlideInfo slideInfo, Quiz quiz)
			: base(quiz.Blocks, slideInfo, quiz.Title, Guid.Parse(quiz.Id), quiz.Meta)
		{
			MaxDropCount = quiz.MaxDropCount;
			MaxScore = quiz.MaxScore;
			ManualChecking = quiz.ManualChecking;
			QuizNormalizedXml = quiz.NormalizedXml;
			ScoringGroup = quiz.ScoringGroup ?? "";
		}

		public override bool ShouldBeSolved => true;

		public int MaxDropCount { get; private set; }

		public bool ManualChecking { get; private set; }

		public string QuizNormalizedXml { get; private set; }

		public AbstractQuestionBlock GetBlockById(string id)
		{
			return Blocks.OfType<AbstractQuestionBlock>().FirstOrDefault(block => block.Id == id);
		}
	}
}