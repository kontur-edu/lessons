using System;
using System.Linq;
using System.Xml.Serialization;
using uLearn.Model.Blocks;

namespace uLearn.Model
{
	[XmlType(IncludeInSchema = false)]
	public enum BlockType
	{
		[XmlEnum("youtube")]
		YouTube,

		[XmlEnum("md")]
		Md,

		[XmlEnum("code")]
		Code,

		[XmlEnum("tex")]
		Tex,

		[XmlEnum("gallery-images")]
		GalleryImages,

		[XmlEnum("include-code")]
		IncludeCode,

		[XmlEnum("include-md")]
		IncludeMd,

		[XmlEnum("gallery")]
		IncludeImageGalleryBlock,

		[XmlEnum("exercise")]
		SingleFileExerciseBlok,

		[XmlEnum("execirse")]
		SingleFileExerciseBlo,

		[XmlEnum("single-file-exercise")]
		SingleFileExerciseBlock,

		[XmlEnum("proj-exercise")]
		ProjectExerciseBlock
	}

	[XmlRoot("Lesson", IsNullable = false, Namespace = "https://ulearn.azurewebsites.net/lesson")]
	public class Lesson
	{
		[XmlElement("title")]
		public string Title;

		[XmlElement("id")]
		public Guid Id;

		[XmlElement("meta")]
		public SlideMetaDescription Meta { get; set; }

		[XmlElement("default-include-file")]
		public string DefaultInclideFile;

		[XmlElement("default-include-code-file")]
		public string DefaultIncludeCodeFile
		{
			get => DefaultInclideFile;
			set => DefaultInclideFile = value;
		}

		[XmlElement(typeof(YoutubeBlock))]
		[XmlElement("md", typeof(MdBlock))]
		[XmlElement(typeof(CodeBlock))]
		[XmlElement(typeof(TexBlock))]
		[XmlElement(typeof(ImageGaleryBlock))]
		[XmlElement(typeof(IncludeCodeBlock))]
		[XmlElement(typeof(IncludeMdBlock))]
		[XmlElement(typeof(IncludeImageGalleryBlock))]
		[XmlElement(typeof(ProjectExerciseBlock))]
		[XmlElement(typeof(SingleFileExerciseBlock))]
		[XmlElement("exercise", typeof(SingleFileExerciseBlock))]
		[XmlElement("execirse", typeof(SingleFileExerciseBlock))]
		[XmlChoiceIdentifier("DefineBlockType")]
		public SlideBlock[] Blocks;

		[XmlIgnore]
		public BlockType[] DefineBlockType;

		public Lesson()
		{
		}

		public Lesson(string title, Guid id, params SlideBlock[] blocks)
		{
			Title = title;
			Id = id;
			Blocks = blocks;
			DefineBlockType = blocks.Select(GetBlockType).ToArray();
		}

		private BlockType GetBlockType(SlideBlock b)
		{
			switch (b)
			{
				case YoutubeBlock _: return BlockType.YouTube;
				case CodeBlock _: return BlockType.Code;
				case ImageGaleryBlock _: return BlockType.GalleryImages;
				case IncludeCodeBlock _: return BlockType.IncludeCode;
				case IncludeImageGalleryBlock _: return BlockType.IncludeImageGalleryBlock;
				case IncludeMdBlock _: return BlockType.IncludeMd;
				case MdBlock _: return BlockType.Md;
				case ProjectExerciseBlock _: return BlockType.ProjectExerciseBlock;
				case SingleFileExerciseBlock _: return BlockType.SingleFileExerciseBlock;
				default: throw new Exception("unknown slide block " + b);
			}
		}
	}
}