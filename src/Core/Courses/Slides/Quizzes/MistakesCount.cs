using System.Xml.Serialization;

namespace Ulearn.Core.Courses.Slides.Quizzes
{
	public class MistakesCount
	{
		public MistakesCount()
			: this(0, 0)
		{
		}
		
		public MistakesCount(int checkedUnnecessary, int notCheckedNecessary)
		{
			CheckedUnnecessary = checkedUnnecessary;
			NotCheckedNecessary = notCheckedNecessary;
		}
		
		[XmlAttribute("checkedUnnecessary")]
		public int CheckedUnnecessary { get; set; }
		
		[XmlAttribute("notCheckedNecessary")]
		public int NotCheckedNecessary { get; set; }

		public bool HasAtLeastOneMistake()
		{
			return CheckedUnnecessary > 0 || NotCheckedNecessary > 0;
		}

		public bool HasNotMoreThatAllowed(MistakesCount allowedMistakesCount)
		{
			return CheckedUnnecessary <= allowedMistakesCount.CheckedUnnecessary && NotCheckedNecessary <= allowedMistakesCount.NotCheckedNecessary;
		}
	}
}