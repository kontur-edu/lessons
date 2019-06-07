using System.Web;

namespace uLearn.Web.Extensions
{
	public static class HtmlStringExtensions
	{
		/* Helper for converting new Asp.NET Core HtmlString into old Asp.NET 6 HtmlString */
		public static HtmlString ToLegacyHtmlString(this global::Microsoft.AspNetCore.Html.HtmlString aspNetCoreHtmlString)
		{
			return new HtmlString(aspNetCoreHtmlString.Value);
		}
	}
}