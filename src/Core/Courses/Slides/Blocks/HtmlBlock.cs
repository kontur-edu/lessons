using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Model.Edx.EdxComponents;

namespace Ulearn.Core.Courses.Slides.Blocks
{
	//[XmlType("html")]
	public class HtmlBlock : SlideBlock, IXmlSerializable
	{
		public string Content { get; private set; } = "";

		public HtmlBlock()
		{
		}
		
		public HtmlBlock(string content)
		{
			Content = content;
		}

		public override string ToString()
		{
			return $"Html {Content.Substring(0, 50)}";
		}

		public override Component ToEdxComponent(string displayName, string courseId, Slide slide, int componentIndex, string ulearnBaseUrl, DirectoryInfo coursePackageRoot)
		{
			throw new NotSupportedException();
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();
			
			if (reader.IsEmptyElement)
			{
				reader.Read();
				return;
			}

			var innerXml = reader.ReadInnerXml();
			
			Content = RemoveXmlNamespacesAndAutoExpandEmptyTags(innerXml.RemoveCommonNesting());
		}

		/* We need to remove xml namespaces (which are inherited from ulearn's xml) and
		   to expand empty tags (i.e. replace auto-collapsed <iframe ... /> to <iframe ...></iframe>) */
		private string RemoveXmlNamespacesAndAutoExpandEmptyTags(string innerXmlContent)
		{
			/* Add outer tag, otherwise XML is not correct and XmlUtils can't parse it */
			var xml = $"<node>{innerXmlContent}</node>";
			
			var xmlWithoutNs = XmlUtils.RemoveAllNamespaces(xml);
			var resultXml = XmlUtils.ExpandEmptyTags(xmlWithoutNs);
			
			/* Delete outer tag */
			if (resultXml.StartsWith("<node>"))
				resultXml = resultXml.Remove(0, "<node>".Length);
			if (resultXml.EndsWith("</node>"))
				resultXml = resultXml.Remove(resultXml.Length - "</node>".Length);

			return resultXml;
		}

		public void WriteXml(XmlWriter writer)
		{
			if (Hide)
				writer.WriteAttributeString("hide", "true");
			writer.WriteRaw(Content);
		}
	}
}