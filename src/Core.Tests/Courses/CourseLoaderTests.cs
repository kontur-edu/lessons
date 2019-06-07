using System.IO;
using NUnit.Framework;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Units;

namespace Ulearn.Core.Tests.Courses
{
	[TestFixture]
	public class CourseLoaderTests
	{
		private const string testDataDirectory = "Courses/TestData/";
		
		private CourseLoader loader;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			loader = new CourseLoader(new UnitLoader(new XmlSlideLoader()));
		}

		[SetUp]
		public void SetUp()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
		}

		private Course LoadCourseFromDirectory(string directory)
		{
			var courseDirectory = new DirectoryInfo(testDataDirectory).GetSubdirectory(directory);
			return loader.Load(courseDirectory);
		}

		[Test]
		public void LoadSimpleCourse()
		{
			var course = LoadCourseFromDirectory("SimpleCourse");
			
			Assert.AreEqual(2, course.Units.Count);
			Assert.AreEqual(Language.CSharp, course.Settings.DefaultLanguage);
			Assert.AreEqual("Simple Course", course.Title);
			CollectionAssert.AreEqual(new [] { new PreludeFile(Language.Html, "Prelude.html"), }, course.Settings.Preludes);
		}

		[Test]
		[Explicit("Для проверки загрузки конкретного курса")]
		[TestCase(@"..\..\..\..\..\..\Courses\Courses\Testing")]
		public void LoadCourseFromPath(string path)
		{
			LoadCourseFromDirectory(path);
		}
	}
}
