using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Html;
using Ulearn.Core.Courses;

namespace Ulearn.Core
{
	public static class Markdown
	{
		public static string RenderMarkdown(this string markdown, FileInfo sourceFile, string baseUrl="")
		{
			var relativeUrl = CourseUnitUtils.GetDirectoryRelativeWebPath(sourceFile);
			baseUrl = baseUrl + relativeUrl;
			return markdown.RenderMarkdown(baseUrl);
		}

		public static string RenderMarkdown(this string markdown, string baseUrlForRelativeLinks=null)
		{
			var texReplacer = new TexReplacer(markdown);

			var markdownObject = new MarkdownDeep.Markdown
			{
				NewWindowForExternalLinks = true,
				ExtraMode = true,
				SafeMode = false,
				MarkdownInHtml = false,
				UrlBaseLocation = baseUrlForRelativeLinks,
			};

			markdownObject.FormatCodeBlock += FormatCodePrettyPrint;

			var html = markdownObject.Transform(texReplacer.ReplacedText);
			return texReplacer.PlaceTexInsertsBack(html);
		}

		public static HtmlString RenderTex(this string textWithTex)
		{
			var texReplacer = new TexReplacer(textWithTex);
			string html = WebUtility.HtmlEncode(texReplacer.ReplacedText);
			return new HtmlString(texReplacer.PlaceTexInsertsBack(html));
		}

		public static readonly Regex rxExtractLanguage = new Regex("^({{(.+)}}[\r\n])", RegexOptions.Compiled);

		public static string FormatCodePrettyPrint(MarkdownDeep.Markdown m, string code)
		{
			// Try to extract the language from the first line
			var match = rxExtractLanguage.Match(code);
			string language = null;

			if (match.Success)
			{
				// Save the language
				var g = match.Groups[2];
				language = g.ToString();

				// Remove the first line
				code = code.Substring(match.Groups[1].Length);
			}

			// If not specified, look for a link definition called "default_syntax" and
			// grab the language from its title
			if (language == null)
			{
				var d = m.GetLinkDefinition("default_syntax");
				if (d != null)
					language = d.title;
			}

			// Common replacements
			if (language == "C#")
				language = "csharp";
			if (language == "C++")
				language = "cpp";

			// Wrap code in pre/code tags and add PrettyPrint attributes if necessary
			if (string.IsNullOrEmpty(language))
				return $"<pre><code>{code}</code></pre>\n";
			else
				return $"<textarea class=\"code code-sample\" data-lang=\"{language.ToLowerInvariant()}\">{code}</textarea>\n";
		}

		public static Tuple<string, List<string>> GetHtmlWithUrls(this string md, string baseUrl = null)
		{
			var texReplacer = new EdxTexReplacer(md);

			var markdown = new Markdown2(baseUrl, false)
			{
				NewWindowForExternalLinks = true,
				ExtraMode = true,
				SafeMode = false,
				MarkdownInHtml = false,
			};

			var relativeUrls = new List<string>();
			markdown.RelativeUrl += relativeUrls.Add;

			var html = markdown.Transform(texReplacer.ReplacedText);

			return Tuple.Create(texReplacer.PlaceTexInsertsBack(html), relativeUrls);
		}
	}
}