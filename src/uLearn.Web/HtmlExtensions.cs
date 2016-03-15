using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace uLearn.Web
{
	public static class HtmlExtensions
	{
		private const string urlRegexS = @"(
											(?<protocol>(https?|ftps?)://|mailto:)?
											(?<usernameAndPassword>
											 [-a-z0-9_.+]+
											 (:[-a-z0-9_.]+)?
											 @h
											)?
											(?<hostnameOrIpAddress>
											 (([-a-z0-9_]{2,}\.)+[a-z]{2,})
											 |
											 ((?:(?:25[0-5]|2[0-4]\d|[01]\d\d|\d?\d)(?(\.?\d)\.)){4})
											)
											(?<port>:\d+)?
											(?<relativePath>
											 [/?]
											 (
											  [-a-z0-9._?,;'/\+&%$#=~!@]*
											  (?<pathInPairedBrackets>
											   \(
												[-a-z0-9._?,;'/\+&%$#=~!@]*
											   \)
											  )?
											  [-a-z0-9._?,;'/\+&%$#=~!@]*
											  # not all chars can be last in url
											  (?<=[-a-z0-9_'/\+&%$#=~@)]|&[a-z]+;)
											 )?
											)?
										   )
										   ";
		private static readonly Regex urlRegex = new Regex(urlRegexS, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

		public static string EncodeMultiLineText(this HtmlHelper helper, string text)
		{
			if (string.IsNullOrEmpty(text))
				return string.Empty;
			return helper.Encode(text).Replace("\n", "<br />").Replace("\r", "");
		}

		private static bool IsEmailUrl(string url)
		{
			return url.Contains("@") && !url.Contains("/");
		}

		public static string HighlightLink(this HtmlHelper helper, string url)
		{
			var fullUrl = url;
			if (!url.Contains("://") && !url.StartsWith("mailto:"))
			{
				if (IsEmailUrl(url))
					fullUrl = "mailto:" + url;
				else
					fullUrl = "http://" + url;
			}
			return string.Format("<a href=\"{0}\">{1}</a>", helper.AttributeEncode(HttpUtility.HtmlDecode(fullUrl)), url);
		}

		/// <summary>
		/// Find urls in html-encoded text and replace it with links
		/// </summary>
		/// <param name="helper"></param>
		/// <param name="htmlEncodedText">HTML-encoded text</param>
		/// <returns>HTML string with links</returns>
		public static string HighlightLinks(this HtmlHelper helper, string htmlEncodedText)
		{
			return urlRegex.Replace(htmlEncodedText, match => helper.HighlightLink(match.Groups[0].Value));
		}
	}
}