using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ulearn.Core.Helpers
{
	/* See https://stackoverflow.com/questions/398518/how-to-implement-glob-in-c-sharp for details */
	public static class GlobSearcher
	{
		static readonly char DirSep = Path.DirectorySeparatorChar;
		
		/// <summary>
		/// return a list of files that matches some wildcard pattern, e.g. 
		/// C:\p4\software\dotnet\tools\*\*.sln to get all tool solution files
		/// </summary>
		/// <param name="glob">pattern to match</param>
		/// <returns>all matching paths</returns>
		public static IEnumerable<string> Glob(string glob)
		{
			return Glob(PathHead(glob) + DirSep, PathTail(glob));
		}
	
		/// <summary>
		/// Uses 'head' and 'tail' — 'head' has already been pattern-expanded
		/// and 'tail' has not.
		/// </summary>
		/// <param name="head">wildcard-expanded</param>
		/// <param name="tail">not yet wildcard-expanded</param>
		/// <returns></returns>
		public static IEnumerable<string> Glob(string head, string tail)
		{
			if (PathTail(tail) == tail)
				foreach (var path in Directory.GetFiles(head, tail).OrderBy(s => s))
					yield return path;
			else
				foreach (var dir in Directory.GetDirectories(head, PathHead(tail)).OrderBy(s => s))
					foreach (var path in Glob(Path.Combine(head, dir), PathTail(tail)))
						yield return path;
		}
	
		/// <summary>
		/// Return the first element of a file path
		/// </summary>
		/// <param name="path">file path</param>
		/// <returns>first logical unit</returns>
		static string PathHead(string path)
		{
			// handle case of \\share\vol\foo\bar — return \\share\vol as 'head'
			// because the dir stuff won't let you interrogate a server for its share list
			// TODO check behavior on Linux to see if this blows up — I don't think so
			if (path.StartsWith("" + DirSep + DirSep))
				return path.Substring(0, 2) + path.Substring(2).Split(DirSep)[0] + DirSep + path.Substring(2).Split(DirSep)[1];
			
			if (!path.Contains(DirSep))
				return ".";
	
			return path.Split(DirSep)[0];
		}
	
		/// <summary>
		/// Return everything but the first element of a file path
		/// e.g. PathTail("C:\TEMP\foo.txt") = "TEMP\foo.txt"
		/// </summary>
		/// <param name="path">file path</param>
		/// <returns>all but the first logical unit</returns>
		static string PathTail(string path)
		{
			if (!path.Contains(DirSep))
				return path;
	
			return path.Substring(1 + PathHead(path).Length);
		}
	}
}