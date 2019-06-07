using System.Collections.Generic;
using System.Web;
using Microsoft.Owin;
using Ulearn.Core.Extensions;

namespace uLearn.Web.Extensions
{
	public static class RequestExtensions
	{
		private const string xSchemeHeaderName = "X-Scheme";

		public static string GetRealRequestScheme(this IOwinRequest request)
		{
			var scheme = request.Scheme;
			if (request.Headers.ContainsKey(xSchemeHeaderName) &&
				(request.Headers[xSchemeHeaderName] == "http" || request.Headers[xSchemeHeaderName] == "https"))
				scheme = request.Headers[xSchemeHeaderName];
			return scheme;
		}

		public static int GetRealRequestPort(this IOwinRequest request)
		{
			if (request.Scheme == "http" && request.LocalPort == 80 && request.GetRealRequestScheme() == "https")
				return 443;
			return request.LocalPort ?? 80;
		}

		public static List<string> GetMultipleValuesFromQueryString(this HttpRequestBase request, string key, bool splitCommaSeparated = true)
		{
			return request.QueryString.GetMultipleValues(key, splitCommaSeparated);
		}
	}
}