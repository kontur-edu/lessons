using System;
using System.Reflection;
using System.Xml.Serialization;
using Ulearn.Core.Courses.Slides.Blocks;

namespace Ulearn.Core.Extensions
{
	public static class TypeExtensions
	{
		public static string GetXmlType(this Type type)
		{
			/*
			Exception for MarkdownBlock. It's here because MarkdownBlock is IXmlSerializable and can not have [XmlType()].
			It's bad, so we should make MarkdownBlock not-IXmlSerializable.
			*/
			if (type.IsEquivalentTo(typeof(MarkdownBlock)))
				return "markdown";
				
			var xmlTypeAttribute = type.GetCustomAttribute<XmlTypeAttribute>();
			return xmlTypeAttribute?.TypeName;
		}
	}
}