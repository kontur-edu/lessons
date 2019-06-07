using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Ulearn.Common.Extensions;
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

		public SlideBlock[] InnerBlocks { get; set; }

		public MarkdownBlock(string markdown)
		{
			if (markdown != null)
				Markdown = markdown.TrimEnd();
		}

		public MarkdownBlock()
		{
		}

		public string RenderMarkdown(string courseId, Guid slideId, string baseUrl)
		{
			return GetMarkdownWithReplacedLinksToStudentZips(courseId, slideId).RenderMarkdown(baseUrl);
		}
		
		public string RenderMarkdown(string courseId, Guid slideId, FileInfo sourceFile, string baseUrl="")
		{
			return GetMarkdownWithReplacedLinksToStudentZips(courseId, slideId).RenderMarkdown(sourceFile, baseUrl);
		}

		/* Replace links to (/Exercise/StudentZip) and to (ExerciseZip): automagically add courseId and slideId */
		private string GetMarkdownWithReplacedLinksToStudentZips(string courseId, Guid slideId)
		{
			if (string.IsNullOrEmpty(Markdown))
				return "";
			var studentZipFullPath = $"(/Exercise/StudentZip?courseId={courseId}&slideId={slideId})";
			return Markdown.Replace("(/Exercise/StudentZip)", studentZipFullPath).Replace("(ExerciseZip)", studentZipFullPath);
		}

		public override string ToString()
		{
			return $"Markdown {Markdown}";
		}

		public override Component ToEdxComponent(string displayName, string courseId, Slide slide, int componentIndex, string ulearnBaseUrl, DirectoryInfo coursePackageRoot)
		{
			var slideDirectory = slide.Info.SlideFile.Directory;
			var directoryRelativePath = "/Courses/" + courseId + "/" + slideDirectory.GetRelativePath(coursePackageRoot.FullName);
			var baseUrl = ulearnBaseUrl + directoryRelativePath.Replace('\\', '/');
			var html = RenderMarkdown(courseId, slide.Id, baseUrl);
			var urlName = slide.NormalizedGuid + componentIndex;
			return new HtmlComponent(urlName, displayName, urlName, html);
		}

		public Component ToEdxComponent(string urlName, string displayName, string directoryName)
		{
			var htmlWithUrls = Markdown.GetHtmlWithUrls("/static/" + urlName + "_");
			return new HtmlComponent(urlName, displayName, urlName, htmlWithUrls.Item1, directoryName, htmlWithUrls.Item2);
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
				Markdown = ((MarkdownBlock)blocks[0]).Markdown;
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