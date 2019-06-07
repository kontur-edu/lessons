namespace Ulearn.Core
{
	public static class Urls
	{
		public static string BasePath = ".";

		public static string Resolve(string url)
		{
			if (url.StartsWith("~"))
				return BasePath + url.Substring(1);
			return url;
		}
	}
}