using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace uLearn.Web
{
	public static class HtmlExtensions
	{
		public static string EncodeMultiLineText(this HtmlHelper helper, string text)
		{
			if (string.IsNullOrEmpty(text))
				return string.Empty;
			return Regex.Replace(helper.Encode(text), "\n", "<br />").Replace("\r", "");
		}
	}
}