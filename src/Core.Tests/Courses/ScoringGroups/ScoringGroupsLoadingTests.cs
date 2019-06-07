using System.IO;
using System.Linq;
using NUnit.Framework;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Units;

namespace Ulearn.Core.Tests.Courses.ScoringGroups
{
	[TestFixture]
	public class ScoringGroupsLoadingTests
	{
		private const string testDataDirectory = "Courses/ScoringGroups/TestData/";
		
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
		public void TestSetAdditionalScoreInUnit()
		{
			var course = LoadCourseFromDirectory("SetAdditionalScoreInUnit");
			var unit1 = course.Units.First();
			var unit2 = course.Units.Last();
			
			Assert.AreEqual(false, course.Settings.Scoring.Groups["ScoringGroup1"].IsMaxAdditionalScoreSpecified);
			Assert.AreEqual(false, course.Settings.Scoring.Groups["ScoringGroup1"].CanBeSetByInstructor);
			
			Assert.AreEqual(true, unit1.Settings.Scoring.Groups["ScoringGroup1"].IsMaxAdditionalScoreSpecified);
			Assert.AreEqual(10, unit1.Settings.Scoring.Groups["ScoringGroup1"].MaxAdditionalScore);
			Assert.AreEqual(true, unit1.Settings.Scoring.Groups["ScoringGroup1"].CanBeSetByInstructor);
			
			Assert.AreEqual(false, unit2.Settings.Scoring.Groups["ScoringGroup1"].IsMaxAdditionalScoreSpecified);
			Assert.AreEqual(0, unit2.Settings.Scoring.Groups["ScoringGroup1"].MaxAdditionalScore);
			Assert.AreEqual(false, unit2.Settings.Scoring.Groups["ScoringGroup1"].CanBeSetByInstructor);
		}
		
		[Test]
		public void TestInheritAdditionalScore()
		{
			var course = LoadCourseFromDirectory("InheritAdditionalScore");
			var unit1 = course.Units[0];
			var unit2 = course.Units[1];
			var unit3 = course.Units[2];
			
			Assert.AreEqual(10, course.Settings.Scoring.Groups["ScoringGroup1"].MaxAdditionalScore);
			Assert.AreEqual(true, course.Settings.Scoring.Groups["ScoringGroup1"].CanBeSetByInstructor);
			
			Assert.AreEqual(false, unit1.Settings.Scoring.Groups["ScoringGroup1"].CanBeSetByInstructor);
			
			Assert.AreEqual(true, unit2.Settings.Scoring.Groups["ScoringGroup1"].CanBeSetByInstructor);
			Assert.AreEqual(20, unit2.Settings.Scoring.Groups["ScoringGroup1"].MaxAdditionalScore);
			
			Assert.AreEqual(true, unit3.Settings.Scoring.Groups["ScoringGroup1"].CanBeSetByInstructor);
			Assert.AreEqual(10, unit3.Settings.Scoring.Groups["ScoringGroup1"].MaxAdditionalScore);
		}
	}
}