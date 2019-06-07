using System;
using System.IO;

namespace Ulearn.Core.Courses
{
	public static class CourseUnitUtils
	{
		public static string GetDirectoryRelativeWebPath(FileInfo file)
		{
			var path = "/";
			var d = file.Directory;
			while (d != null)
			{
				path = $"/{d.Name}{path}";
				if (d.Name.Equals("Courses", StringComparison.OrdinalIgnoreCase))
					return path;
				d = d.Parent;
			}
			throw new FileNotFoundException("Can't find file's relative web path because root courses folder (Courses/) " +
											$"not found in parent directories of file {file.FullName}", file.FullName);
		}
	}
}