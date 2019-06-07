using System.Collections.Immutable;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualBasic.FileIO;
using NUnit.Framework;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Courses.Units;

namespace uLearn.CSharp
{
	[TestFixture]
	public class CourseValidator_ReportWarning_should
	{
		private static readonly string tempSlideFolderPath = Path.Combine(
			TestContext.CurrentContext.TestDirectory,
			"ReportWarningTests_Temp_SlideFolder"
		);

		private static readonly DirectoryInfo tempSlideFolder = new DirectoryInfo(tempSlideFolderPath);

		private static readonly CsProjectExerciseBlock exerciseBlock = new CsProjectExerciseBlock
		{
			StartupObject = "test.Program",
			UserCodeFilePath = TestsHelper.UserCodeFileName,
			SlideFolderPath = tempSlideFolder,
			CsProjFilePath = TestsHelper.CsProjFilePath
		};

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			TestsHelper.RecreateDirectory(tempSlideFolderPath);
			FileSystem.CopyDirectory(TestsHelper.ProjSlideFolderPath, tempSlideFolderPath);

			string studentZipFilepath = Path.Combine(tempSlideFolderPath, "ProjDir.exercise.zip");
			if (File.Exists(studentZipFilepath))
				File.Delete(studentZipFilepath);

			var ctx = new SlideBuildingContext("Test", new Unit(null, exerciseBlock.SlideFolderPath), CourseSettings.DefaultSettings, null);
			// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
			exerciseBlock.BuildUp(ctx, ImmutableHashSet<string>.Empty).ToList();
		}

		[SetUp]
		public void SetUp()
		{
			FileSystem.CopyDirectory(TestsHelper.ProjSlideFolderPath, tempSlideFolderPath, true);
		}

		[Test]
		public void ReportWarning_If_ExerciseFolder_DoesntContain_SolutionFile()
		{
			FileSystem.DeleteFile(exerciseBlock.CorrectSolutionFile.FullName);

			var validatorOut = TestsHelper.ValidateBlock(exerciseBlock);

			validatorOut
				.Should().Contain($"Exercise directory doesn't contain {exerciseBlock.CorrectSolutionFileName}");
		}

		[Test]
		public void ReportWarning_If_WrongAnswers_Have_Errors()
		{
			var validatorOut = TestsHelper.ValidateBlock(exerciseBlock);

			validatorOut
				.Should()
				.Contain(
					$"Code verdict of file with wrong answer ({exerciseBlock.UserCodeFileNameWithoutExt}.WrongAnswer.Type.cs) is not OK.")
				.And
				.Contain("Verdict: CompilationError");
			validatorOut
				.Should().Contain(
					$"Code of file with wrong answer ({exerciseBlock.UserCodeFileNameWithoutExt}.WrongAnswer.21.plus.21.cs) is solution!");
			validatorOut
				.Should().NotContain($"{exerciseBlock.UserCodeFileNameWithoutExt}.WrongAnswer.27.cs");
		}

		[Test]
		public void Not_Report_Indentation_Warning_On_Ethalon_Solution_Of_SingleFileExercise()
		{
			var exerciseXmlFile = TestsHelper.ProjSlideFolder.GetFile("S055 - Упражнение на параметры по умолчанию.lesson.xml");

			var courseSettings = new CourseSettings(CourseSettings.DefaultSettings)
			{
				Preludes = new[] { new PreludeFile(Language.CSharp, TestsHelper.ProjSlideFolder.GetFile("Prelude.cs").FullName) }
			};

			var unit = new Unit(UnitSettings.CreateByTitle("Unit title", courseSettings), TestsHelper.ProjSlideFolder);
			var slideLoadingContext = new SlideLoadingContext("Test", unit, courseSettings, exerciseXmlFile, 1);
			var exerciseSlide = (ExerciseSlide) new XmlSlideLoader().Load(slideLoadingContext);
			
			var validatorOut = TestsHelper.ValidateExerciseSlide(exerciseSlide);
			
			validatorOut.Should().BeNullOrEmpty();
		}
	}
}