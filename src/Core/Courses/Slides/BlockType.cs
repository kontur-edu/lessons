using System;
using System.Xml.Serialization;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Courses.Slides.Quizzes.Blocks;

namespace Ulearn.Core.Courses.Slides
{
	[XmlType(IncludeInSchema = false)]
	public enum BlockType
	{
		[XmlEnum("youtube")]
		YouTube,

		[XmlEnum("markdown")]
		Markdown,

		[XmlEnum("code")]
		Code,

		[XmlEnum("tex")]
		Tex,

		[XmlEnum("galleryImages")]
		GalleryImages,

		[XmlEnum("includeCode")]
		IncludeCode,

		[XmlEnum("includeMarkdown")]
		IncludeMarkdown,
		
		[XmlEnum("includeBlocks")]
		IncludeBlocks,

		[XmlEnum("gallery")]
		IncludeImageGallery,
		
		[XmlEnum("html")]
		Html,
		
		[XmlEnum("spoiler")]
		Spoiler,

		[XmlEnum("exercise.file")]
		SingleFileExercise,

		[XmlEnum("exercise.csproj")]
		CsProjectExercise,

		[XmlEnum("question.isTrue")]
		IsTrueQuestion,
		
		[XmlEnum("question.choice")]
		ChoiceQuestion,
		
		[XmlEnum("question.text")]
		TextQuestion,
		
		[XmlEnum("question.order")]
		OrderQuestion,
		
		[XmlEnum("question.match")]
		MatchQuestion,
	}

	public static class BlockTypeHelpers
	{
		public static BlockType GetBlockType(SlideBlock block)
		{
			switch (block)
			{
				case YoutubeBlock _: return BlockType.YouTube;
				case CodeBlock _: return BlockType.Code;
				case ImageGalleryBlock _: return BlockType.GalleryImages;
				case IncludeCodeBlock _: return BlockType.IncludeCode;
				case IncludeImageGalleryBlock _: return BlockType.IncludeImageGallery;
				case IncludeMarkdownBlock _: return BlockType.IncludeMarkdown;
				case MarkdownBlock _: return BlockType.Markdown;
				case TexBlock _: return BlockType.Tex; 
				case HtmlBlock _: return BlockType.Html;
				case SpoilerBlock _: return BlockType.Spoiler;
				
				case FillInBlock _: return BlockType.TextQuestion;
				case ChoiceBlock _: return BlockType.ChoiceQuestion;
				case MatchingBlock _: return BlockType.MatchQuestion;
				case OrderingBlock _: return BlockType.OrderQuestion;
				case IsTrueBlock _: return BlockType.IsTrueQuestion;
				
				case CsProjectExerciseBlock _: return BlockType.CsProjectExercise;
				case SingleFileExerciseBlock _: return BlockType.SingleFileExercise;
				
				default: throw new Exception("Unknown slide block " + block);
			}
		}
	}
}