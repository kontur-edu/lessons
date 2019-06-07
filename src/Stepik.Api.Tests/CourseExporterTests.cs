using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using log4net.Config;
using NUnit.Framework;
using Ulearn.Core.Courses;

namespace Stepik.Api.Tests
{
	[TestFixture]
	[Explicit("Be careful. This test deletes old content and fully rewrites your course on Stepik")]
	public class CourseExporterTests : StepikApiTests
	{
		private const int stepikCourseId = 2599;
		private const string stepikXQueueName = "urfu_cs1_testing";

		private CourseExporter courseExporter;

		[SetUp]
		public async Task SetUp()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			BasicConfigurator.Configure();

			await InitializeClient();

			courseExporter = new CourseExporter(client.AccessToken);
		}
		
		[Test]
		//[TestCase(@"..\..\..\..\courses\Linq")]
		[TestCase(@"..\..\..\..\..\courses\BasicProgramming\OOP\OOP\Slides\")]
		public async Task TestExportCourseFromDirectory(string coursePath)
		{
			var courseLoader = new CourseLoader();
			var stubCourse = courseLoader.Load(new DirectoryInfo(coursePath));
			await courseExporter.InitialExportCourse(stubCourse, new CourseInitialExportOptions(stepikCourseId, stepikXQueueName, new List<Guid>()));
		}
	}
}
