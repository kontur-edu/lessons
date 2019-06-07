using System.IO;
using NUnit.Framework;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Units;

namespace Ulearn.Core.Tests.Courses.Units
{
	[TestFixture]
	public class UnitLoaderTests
	{
		private const string testDataDirectory = "Courses/Units/TestData/";
		
		private UnitLoader loader;
		private CourseSettings courseSettings;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			loader = new UnitLoader(new XmlSlideLoader());
			courseSettings = new CourseSettings(CourseSettings.DefaultSettings);
			courseSettings.Scoring.Groups.Add("ScoringGroup1", new ScoringGroup
			{
				Id = "ScoringGroup1"
			});
		}

		[SetUp]
		public void SetUp()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
		}

		private Unit LoadUnitFromDirectory(string directory)
		{
			var unitFile = new DirectoryInfo(testDataDirectory).GetSubdirectory(directory).GetFile("unit.xml");
			var courseLoadingContext = new CourseLoadingContext("CourseId", courseSettings, new DirectoryInfo("."), new FileInfo("course.xml"));
			return loader.Load(unitFile, courseLoadingContext, 0);
		}

		[Test]
		public void LoadSimpleUnit()
		{
			var unit = LoadUnitFromDirectory("SimpleUnit");
			
			Assert.AreEqual("default include code file", unit.Settings.DefaultIncludeCodeFile);
			Assert.AreEqual(10, unit.Scoring.Groups["ScoringGroup1"].MaxAdditionalScore);
			Assert.AreEqual(0, unit.Scoring.Groups["ScoringGroup1"].MaxNotAdditionalScore);
		}

		[Test]
		public void LoadUnitWithSimpleSlides()
		{
			var unit = LoadUnitFromDirectory("UnitWithSimpleSlides");
			
			Assert.AreEqual(2, unit.Slides.Count);
		}
	}
}
