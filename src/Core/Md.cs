using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using MarkdownDeep;
using Microsoft.AspNetCore.Html;

namespace uLearn
{
	public static class Md
	{
		public static string RenderMd(this string md, FileInfo sourceFile, string baseUrl="")
		{
			var relativeUrl = CourseUnitUtils.GetDirectoryRelativeWebPath(sourceFile);
			baseUrl = baseUrl + relativeUrl;
			return md.RenderMd(baseUrl);
		}

		public static string RenderMd(this string md, string baseUrlForRelativeLinks=null)
		{
			var texReplacer = new TexReplacer(md);

			var markdown = new Markdown
			{
				NewWindowForExternalLinks = true,
				ExtraMode = true,
				SafeMode = false,
				MarkdownInHtml = false,
				UrlBaseLocation = baseUrlForRelativeLinks,
			};

			markdown.FormatCodeBlock += FormatCodePrettyPrint;

			var html = markdown.Transform(texReplacer.ReplacedText);
			return texReplacer.PlaceTexInsertsBack(html);
		}

		public static HtmlString RenderTex(this string textWithTex)
		{
			var texReplacer = new TexReplacer(textWithTex);
			string html = HttpUtility.HtmlEncode(texReplacer.ReplacedText);
			return new HtmlString(texReplacer.PlaceTexInsertsBack(html));
		}

		public static readonly Regex rxExtractLanguage = new Regex("^({{(.+)}}[\r\n])", RegexOptions.Compiled);

		public static string FormatCodePrettyPrint(Markdown m, string code)
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