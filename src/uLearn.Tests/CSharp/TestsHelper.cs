using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using NUnit.Framework;
using RunCsJob;
using test;
using uLearn.CourseTool.Validating;
using uLearn.Model.Blocks;

namespace uLearn.CSharp
{
	public static class TestsHelper
	{
		public static readonly string ProjSlideFolderPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "CSharp", "TestProject");
		public static readonly DirectoryInfo ProjSlideFolder = new DirectoryInfo(ProjSlideFolderPath);

		public static readonly string ProjExerciseFolderPath = Path.Combine(ProjSlideFolderPath, "ProjDir");
		public static readonly DirectoryInfo ProjExerciseFolder = new DirectoryInfo(ProjExerciseFolderPath);

		public static readonly string CsProjFilename = "test.csproj";
		public static readonly string CsProjFilePath = Path.Combine("ProjDir", CsProjFilename);
		public static readonly string UserCodeFileName = Path.Combine("inner-dir-1", "inner-dir-2", $"{nameof(MeaningOfLifeTask)}.cs");

		public static readonly string[] WrongAnswerNames =
		{
			$"{nameof(AnotherTask)}.WrongAnswer.88.cs",
			$"{nameof(MeaningOfLifeTask)}.WrongAnswer.27.cs",
			$"{nameof(MeaningOfLifeTask)}.WrongAnswer.Type.cs",
			$"{nameof(MeaningOfLifeTask)}.WrongAnswer.21.plus.21.cs"
		};

		public static readonly string[] SolutionNames =
		{
			$"{nameof(AnotherTask)}.Solution.cs",
			Path.Combine("inner-dir-1", "inner-dir-2", $"{nameof(MeaningOfLifeTask)}.Solution.cs")
		};

		public static string OrderedWrongAnswersAndSolutionNames => ToString(WrongAnswersAndSolutionNames.OrderBy(n => n));

		public static IEnumerable<string> WrongAnswersAndSolutionNames => WrongAnswerNames.Concat(SolutionNames);

		public static string WrongAnswerNamesToString() => ToString(WrongAnswerNames);

		public static string SolutionNamesToString() => ToString(SolutionNames);

		public static string ToString(IEnumerable<string> arr) => string.Join(", ", arr);

		public static void RecreateDirectory(string path)
		{
			if (FileSystem.DirectoryExists(path))
				FileSystem.DeleteDirectory(path, DeleteDirectoryOption.DeleteAllContents);
			FileSystem.CreateDirectory(path);
		}

		public static ProjectExerciseValidator BuildProjectExerciseValidator(ProjectExerciseBlock exBlock, StringBuilder valOut)
		{
			var slide = BuildSlide(exBlock);
			return new ProjectExerciseValidator(BuildValidator(slide, valOut), new SandboxRunnerSettings(), slide, exBlock);
		}

		public static CourseValidator BuildValidator(ExerciseSlide slide, StringBuilder valOut)
		{
			var v = new CourseValidator(new List<Slide> { slide }, new SandboxRunnerSettings());
			v.Warning += msg => { valOut.Append(msg); };
			v.Error += msg => { valOut.Append(msg); };
			return v;
		}

		public static ExerciseSlide BuildSlide(ExerciseBlock ex)
		{
			var unit = new Unit(new UnitSettings { Title = "UnitTitle" }, null);
			var slideInfo = new SlideInfo(unit, null, 0);
			return new ExerciseSlide(new List<SlideBlock> { ex }, slideInfo, "SlideTitle", Guid.Empty, meta: null);
		}

		public static string ValidateBlock(ProjectExerciseBlock exBlock)
		{
			var valOut = new StringBuilder();
			var val = BuildProjectExerciseValidator(exBlock, valOut);
			val.ValidateExercises();
			return valOut.ToString();
		}

		public static string ValidateBlock(ExerciseBlock exBlock)
		{
			var valOut = new StringBuilder();
			var val = BuildValidator(BuildSlide(exBlock), valOut);
			val.ValidateExercises();
			return valOut.ToString();
		}
	}
}