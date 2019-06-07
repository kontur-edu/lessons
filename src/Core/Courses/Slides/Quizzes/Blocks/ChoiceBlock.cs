using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Model.Edx.EdxComponents;

namespace Ulearn.Core.Courses.Slides.Quizzes.Blocks
{
	[XmlType("question.choice")]
	public class ChoiceBlock : AbstractQuestionBlock
	{
		[XmlAttribute("multiple")]
		public bool Multiple;

		[XmlAttribute("shuffle")]
		public bool Shuffle;

		[XmlElement("item")]
		public ChoiceItem[] Items;
		
		[XmlElement("allowedMistakes")]
		public MistakesCount AllowedMistakesCount { get; set; } = new MistakesCount(0, 0);

		public ChoiceItem[] ShuffledItems()
		{
			return Shuffle ? Items.Shuffle().ToArray() : Items;
		}

		public override void Validate(SlideBuildingContext slideBuildingContext)
		{
			if (Items.DistinctBy(i => i.Id).Count() != Items.Length)
				throw new FormatException("Duplicate choice id in quizBlock " + Id);
			if (!Multiple && Items.Count(i => i.IsCorrect == ChoiceItemCorrectness.True) != 1)
				throw new FormatException("Should be exaclty one correct item for non-multiple choice. BlockId=" + Id);
			if (!Multiple && Items.Count(i => i.IsCorrect == ChoiceItemCorrectness.Maybe) != 0)
				throw new FormatException("'Maybe' items are not allowed for for non-multiple choice. BlockId=" + Id);
		}

		public override Component ToEdxComponent(string displayName, string courseId, Slide slide, int componentIndex, string ulearnBaseUrl, DirectoryInfo coursePackageRoot)
		{
			var items = Items.Select(x => new Choice
			{
				Correct = x.IsCorrect.IsTrueOrMaybe(),
				Text = EdxTexReplacer.ReplaceTex(x.Description)
			}).ToArray();
			ChoiceResponse cr;
			if (Multiple)
			{
				var cg = new CheckboxGroup { Label = Text, Direction = "vertical", Choices = items };
				cr = new ChoiceResponse { ChoiceGroup = cg };
			}
			else
			{
				var cg = new MultipleChoiceGroup { Label = Text, Type = "MultipleChoice", Choices = items };
				cr = new MultipleChoiceResponse { ChoiceGroup = cg };
			}
			return new MultipleChoiceComponent
			{
				UrlName = slide.NormalizedGuid + componentIndex,
				ChoiceResponse = cr,
				Title = EdxTexReplacer.ReplaceTex(Text)
			};
		}

		public override string TryGetText()
		{
			return Text + '\n' + string.Join("\n", Items.Select(item => item.GetText()));
		}

		public override bool HasEqualStructureWith(SlideBlock other)
		{
			var block = other as ChoiceBlock;
			if (block == null)
				return false;
			if (Items.Length != block.Items.Length || Multiple != block.Multiple)
				return false;

			var idsSet = Items.Select(i => i.Id).ToImmutableHashSet();
			var blockIdsSet = block.Items.Select(i => i.Id).ToImmutableHashSet();
			return idsSet.SetEquals(blockIdsSet);
		}
	}
}