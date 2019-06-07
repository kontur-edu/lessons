using System.Xml.Serialization;
using Ulearn.Core.Courses.Slides.Blocks;

namespace Ulearn.Core.Courses.Slides.Quizzes.Blocks
{
	public abstract class AbstractQuestionBlock : SlideBlock
	{
		protected AbstractQuestionBlock()
		{
			/* Default max score */
			MaxScore = 1;
		}

		[XmlAttribute("id")]
		public string Id;

		[XmlAttribute("maxScore")]
		public int MaxScore;

		[XmlElement("text")]
		public string Text;

		[XmlIgnore]
		public int QuestionIndex;

		public override string TryGetText()
		{
			return Text;
		}

		public abstract bool HasEqualStructureWith(SlideBlock other);

		public bool IsScoring => MaxScore > 0;
	}
}