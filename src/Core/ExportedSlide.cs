using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using uLearn.Model.Blocks;
using uLearn.Quizes;

namespace uLearn
{
	[XmlType("exported-slide")]
	public class ExportedSlide
	{
		public ExportedSlide()
		{
			
		}

		public ExportedSlide(Slide slide)
		{
			Title = slide.Title;
			Blocks = slide.Blocks.ToList();
			MaxScore = slide.MaxScore;
		}

		[XmlAttribute("title")]
		public string Title;

		[XmlAttribute("max-score")]
		public int MaxScore;

		[XmlElement("md", Type = typeof(MdBlock))]
		[XmlElement(typeof(CodeBlock))]
		[XmlElement(typeof(TexBlock))]
		[XmlElement(typeof(IncludeCodeBlock))]
		[XmlElement("isTrue", Type = typeof(IsTrueBlock))]
		[XmlElement("choice", Type = typeof(ChoiceBlock))]
		[XmlElement("fillIn", Type = typeof(FillInBlock))]
		[XmlElement("ordering", Type = typeof(OrderingBlock))]
		[XmlElement("matching", Type = typeof(MatchingBlock))]
		[XmlElement(typeof(SingleFileExerciseBlock))]
		[XmlElement(typeof(ProjectExerciseBlock))]
		[XmlElement(typeof(ZipExerciseBlock))]
		[XmlElement(typeof(ImageGaleryBlock))]
		[XmlElement(typeof(YoutubeBlock))]
		[XmlElement(typeof(IncludeMdBlock))]
		[XmlElement(typeof(IncludeImageGalleryBlock))]
		public List<SlideBlock> Blocks;
	}
}