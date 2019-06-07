using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Ulearn.Common.Extensions;

namespace Ulearn.Core
{
	public enum Language : short
	{
		[XmlEnum("csharp")]
		CSharp = 1,
		
		[XmlEnum("python2")]
		Python2 = 2,
		
		[XmlEnum("python3")]
		Python3 = 3,
		
		[XmlEnum("java")]
		Java = 4,
		
		[XmlEnum("javascript")]
		JavaScript = 5,

		[XmlEnum("html")]
		Html = 6,
		
		[XmlEnum("typescript")]
		TypeScript = 7,
		
		[XmlEnum("css")]
		Css = 8,
		
		[XmlEnum("text")]
		Text = 100,
	}

	public static class LanguageHelpers
	{
		private static readonly Dictionary<string, Language> extensions = new Dictionary<string, Language>
		{
			{".cs", Language.CSharp},
			
			{".py", Language.Python3},
			{".py2", Language.Python2},
			{".py3", Language.Python3},
			
			{".html", Language.Html},
			{".css", Language.Css},
			
			{".js", Language.JavaScript},
			{".ts", Language.TypeScript},
			
			{".java", Language.Java},
			
			{".txt", Language.Text},
		};
		
		public static Language GuessByExtension(string extension)
		{
			if (extensions.ContainsKey(extension))
				return extensions[extension];
			
			throw new ArgumentException(
				$"Can't guess programming language by file extension. Unknown file extension: {extension}\n" + 
				$"Known extensions are {string.Join(", ", extensions.Keys)}."
			);
		}

		public static Language GuessByExtension(FileInfo file)
		{
			return GuessByExtension(file.Extension);
		}

		public static Language ParseFromXml(string language)
		{
			var serializer = new XmlSerializer(typeof(Language));
			
			/* XmlSerializer expects XML so wrap what you got in xml tags */
			var xmlBytes = Encoding.ASCII.GetBytes($"<Language>{language.EscapeHtml()}</Language>");
			using (var ms = new MemoryStream(xmlBytes))
				return (Language) serializer.Deserialize(ms);
		}

		public static Language ParseByName(string language)
		{
			return ParseFromXml(language);
		}

		public static bool TryParseByName(string language, out Language value)
		{
			try
			{
				value = ParseByName(language);
				return true;
			}
			catch (Exception e)
			{
				value = Language.Text;
				return false;
			}
		}
	}

	public static class LanguageExtensions
	{
		public static bool HasAutomaticChecking(this Language language)
		{
			/* For a while only C# has automatic checking on ulearn */
			return language == Language.CSharp;
		}

		public static string GetName(this Language language)
		{
			return language.GetXmlEnumName();
		}

		public static string GetName(this Language? language)
		{
			if (language.HasValue)
				return language.Value.GetName();
			return "";
		}
	}
}