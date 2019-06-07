using System.IO;
using NUnit.Framework;
using Ulearn.Core.Courses;

namespace uLearn
{
	[TestFixture]
	public class CourseLoader_should
	{
		[Test, Explicit("Для профилирования загрузки курса")]
		public void LoadCourse()
		{
			new CourseLoader().Load(new DirectoryInfo(@"c:\work\edu\BasicProgramming\Part01\BasicProgramming\Slides"));
		}
	}
}