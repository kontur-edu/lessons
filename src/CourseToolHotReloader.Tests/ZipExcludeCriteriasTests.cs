using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Ulearn.Common;

namespace CourseToolHotReloader.Tests
{
	[TestFixture]
	public class ZipExcludeCriteriasTests
	{
		[Test]
		[Explicit]
		public void Test()
		{
			var excludeCriterias = new List<string> {".vs/", ".idea/", "/*/*.cs", "obj/"};
			using (var ms = ZipUtils.CreateZipFromDirectory(new List<string>{"D://zip/test"}, excludeCriterias, null, Encoding.UTF8))
			using (var fileStream = new FileStream("D://zip/result.zip", FileMode.Create, FileAccess.Write))
				ms.CopyTo(fileStream);
		}
	}
}