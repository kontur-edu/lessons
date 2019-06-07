using System.IO;
using System.Xml.Serialization;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Model.Edx.EdxComponents;

namespace Ulearn.Core.Courses.Slides.Quizzes.Blocks
{
	[XmlType("question.isTrue")]
	public class IsTrueBlock : AbstractQuestionBlock
	{
		[XmlAttribute("answer")]
		public bool Answer;

		[XmlAttribute("explanation")]
		public string Explanation;

		public bool IsRight(string text)
		{
			return text.ToLower() == Answer.ToString().ToLower();
		}

		public override void Validate(SlideBuildingContext slideBuildingContext)
		{
		}

		public override Component ToEdxComponent(string displayName, string courseId, Slide slide, int componentIndex, string ulearnBaseUrl, DirectoryInfo coursePackageRoot)
		{
			var items = new[]
			{
				new Choice { Correct = Answer, Text = "true" },
				new Choice { Correct = !Answer, Text = "false" }
			};
			var cg = new MultipleChoiceGroup { Label = Text, Type = "MultipleChoice", Choices = items };
			return new MultipleChoiceComponent
			{
				UrlName = slide.NormalizedGuid + componentIndex,
				ChoiceResponse = new MultipleChoiceResponse { ChoiceGroup = cg },
				Title = EdxTexReplacer.ReplaceTex(Text),
				Solution = new Solution(Explanation)
			};
		}

		public override string TryGetText()
		{
			return Text + '\n' + Explanation;
		}

		public override bool HasEqualStructureWith(SlideBlock other)
		{
			var block = other as IsTrueBlock;
			if (block == null)
				return false;
			return Answer == block.Answer;
		}
	}
}