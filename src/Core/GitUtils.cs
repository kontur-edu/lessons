using System;

namespace Ulearn.Core
{
	public class GitUtils
	{
		public static string RepoUrlToCommitLink(string repoUrl, string hash)
		{
			return RepoUrlToRepoLink(repoUrl) + $"/commit/{hash}";
		}

		public static string GetSlideEditLink(string repoUrl, string courseXmlPath, string filePathRelativeCourseXml)
		{
			var unsafeUrl = RepoUrlToRepoLink(repoUrl) + "/edit/master/" + (courseXmlPath + "/" + filePathRelativeCourseXml).Replace('\\', '/');
			return Uri.EscapeUriString(unsafeUrl);
		}
		
		private static string RepoUrlToRepoLink(string repoUrl)
		{
			var link = repoUrl.Substring(4);
			link = link.Substring(0, link.Length - 4);
			link = link.Replace(':', '/');
			return "https://" + link;
		}
	}
}