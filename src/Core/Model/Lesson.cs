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
		ProjectExerciseBlock,

		[XmlEnum("zip-exercise")]
		ZipExerciseBlock,
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
		public string DefaultIncludeFile { get; set; }

		[XmlElement("default-include-code-file")]
		public string DefaultIncludeCodeFile
		{
			get => DefaultIncludeFile;
			set => DefaultIncludeFile = value;
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
		[XmlElement(typeof(ZipExerciseBlock))]
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
				case ZipExerciseBlock _: return BlockType.ZipExerciseBlock;
				case SingleFileExerciseBlock _: return BlockType.SingleFileExerciseBlock;
				case TexBlock _: return BlockType.Tex; 
				default: throw new Exception("Unknown slide block " + b);
			}
		}
	}
}