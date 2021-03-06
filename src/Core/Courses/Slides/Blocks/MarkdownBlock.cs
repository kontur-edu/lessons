using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Model.Edx.EdxComponents;

namespace Ulearn.Core.Courses.Slides.Blocks
{
	// [XmlType("markdown")]
	// [XmlRoot("markdown", Namespace = "https://ulearn.me/schema/v2")]
	public class MarkdownBlock : SlideBlock, IXmlSerializable
	{
		private string markdown;

		[XmlText]
		public string Markdown
		{
			get => markdown;
			set => markdown = value.RemoveCommonNesting();
		}

		public SlideBlock[] InnerBlocks { get; set; } // может содержать MarkdownBlock или CodeBlock

		public MarkdownBlock(string markdown)
		{
			if (markdown != null)
				Markdown = markdown.TrimEnd();
		}

		public MarkdownBlock()
		{
		}

		public string RenderMarkdown(Slide slide, MarkdownRenderContext context)
		{
			return GetMarkdownWithReplacedLinksToStudentZips(context.CourseId, slide, context.BaseUrlApi).RenderMarkdown(context);
		}

		/* Replace links to (/Exercise/StudentZip) and to (ExerciseZip): automagically add courseId and slideId */
		private string GetMarkdownWithReplacedLinksToStudentZips(string courseId, Slide slide, string baseUrlApi)
		{
			if (string.IsNullOrEmpty(Markdown))
				return "";
			var exerciseSlide = slide as ExerciseSlide;
			if (exerciseSlide == null)
				return Markdown;
			if (!(Markdown.Contains("(/Exercise/StudentZip)") || Markdown.Contains("(ExerciseZip)")))
				return Markdown;
			var studentZipName = (exerciseSlide.Exercise as CsProjectExerciseBlock)?.CsprojFileName ?? new DirectoryInfo((exerciseSlide.Exercise as UniversalExerciseBlock).ExerciseDirPath).Name;
			var studentZipFullPath = CourseUrlHelper.GetAbsoluteUrlToStudentZip(baseUrlApi, courseId, slide.Id, $"{studentZipName}.zip");
			return Markdown.Replace("(/Exercise/StudentZip)", $"({studentZipFullPath})").Replace("(ExerciseZip)", $"({studentZipFullPath})");
		}

		public override string ToString()
		{
			return $"Markdown {Markdown}";
		}

		public override Component ToEdxComponent(EdxComponentBuilderContext context)
		{
			var html = RenderMarkdown(context.Slide, new MarkdownRenderContext(context.UlearnBaseUrlApi, context.UlearnBaseUrlWeb, context.CourseId, context.Slide.Unit.UnitDirectoryRelativeToCourse));
			var urlName = context.Slide.NormalizedGuid + context.ComponentIndex;
			return new HtmlComponent(urlName, context.DisplayName, urlName, html);
		}

		public Component ToEdxComponent(string urlName, string displayName, string courseDirectory, string unitDirectoryRelativeToCourse)
		{
			var htmlWithUrls = Markdown.GetHtmlWithUrls("/static/" + urlName + "_");
			return new HtmlComponent(urlName, displayName, urlName, htmlWithUrls.Item1, courseDirectory, unitDirectoryRelativeToCourse, htmlWithUrls.Item2);
		}

		public override IEnumerable<SlideBlock> BuildUp(SlideBuildingContext context, IImmutableSet<string> filesInProgress)
		{
			return InnerBlocks?.SelectMany(b => b.BuildUp(context, filesInProgress)) ?? new[] { this };
		}

		public override string TryGetText()
		{
			return Markdown;
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();
			Hide = reader.GetAttribute("hide").IsOneOf("true", "1");
			var blocks = ReadBlocks(Hide, reader).ToArray();
			if (blocks.Length == 1 && blocks[0].GetType() == typeof(MarkdownBlock))
			{
				var mb = (MarkdownBlock)blocks[0];
				Markdown = mb.Markdown;
				Hide = mb.Hide;
			}
			else
				InnerBlocks = blocks;
		}

		private IEnumerable<SlideBlock> ReadBlocks(bool hide, XmlReader reader)
		{
			var tagName = reader.Name;
			if (reader.IsEmptyElement)
			{
				reader.Read();
				yield break;
			}

			reader.Read();
			while (!(reader.NodeType == XmlNodeType.EndElement && reader.Name == tagName))
			{
				if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA)
				{
					yield return new MarkdownBlock(reader.ReadContentAsString()) { Hide = hide };
				}
				else if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == "note")
					{
						yield return new MarkdownBlock
						{
							Hide = true,
							Markdown = reader.ReadElementContentAsString()
						};
					}
					else if (reader.LocalName == "code")
					{
						var languageAttribute = reader.GetAttribute("language");
						Language? language = null;
						if (!string.IsNullOrEmpty(languageAttribute))
							language = LanguageHelpers.ParseFromXml(languageAttribute);
						yield return new CodeBlock(reader.ReadElementContentAsString(), language) { Hide = hide };
					}
					else
						throw new NotSupportedException(
							$"Invalid tag inside of <markdown>: {reader.LocalName}. Supported tags inside <markdown> are <note> and <code>."
						);
				}
				else
					reader.Read();
			}

			reader.Read();
		}

		public void WriteXml(XmlWriter writer)
		{
			if (Hide)
				writer.WriteAttributeString("hide", "true");
			writer.WriteString(markdown);
		}
	}
}