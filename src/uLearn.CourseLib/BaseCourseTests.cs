using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using RunCsJob;
using RunCsJob.Api;
using uLearn.Model.Blocks;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeMadeStatic.Global

namespace uLearn
{
	[TestFixture]
	public abstract class SlideTestBase
	{
		[Test]
		public void Test()
		{
			BaseCourseTests.TestExercise(GetType());
		}
	}

	[TestFixture]
	public class BaseCourseTests
	{
		private readonly Type someSlideClass;

		public BaseCourseTests(Type someSlideClass)
		{
			this.someSlideClass = someSlideClass;
		}

		[Test]
		public static void NoDuplicateVideos()
		{
			var videos = GetVideos().ToLookup(d => d.Arguments[1], d => d.Arguments[0]);
			foreach (var g in videos.Where(g => g.Count() > 1))
				Assert.Fail("Duplicate videos on slides " + string.Join(", ", g));
		}

		[TestCaseSource(nameof(GetExerciseSlidesTestCases))]
		public void Slide(Type slideType)
		{
			Assert.IsTrue(typeof(SlideTestBase).IsAssignableFrom(slideType), slideType + " does not inherit from SlideTestBase");
		}

		public IEnumerable<TestCaseData> GetExerciseSlidesTestCases()
		{
			return GetSlideTypes()
				.Select(typeAttr => typeAttr.Item1)
				.Where(type => GetExpectedOutputAttributes(type).Any())
				.Select(type => new TestCaseData(type).SetName(type.Name + ".Main"));
		}

		public IEnumerable<Tuple<Type, SlideAttribute>> GetSlideTypes()
		{
			return someSlideClass.Assembly
				.GetTypes()
				.Select(t => Tuple.Create(t, t.GetCustomAttributes(typeof(SlideAttribute)).Cast<SlideAttribute>().FirstOrDefault()))
				.Where(t => t.Item2 != null);
		}

		public static void TestExercise(Type slide)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			var newOut = new StringWriter();
			var oldOut = Console.Out;
			var methodInfo = slide.GetMethod("Main", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (methodInfo == null)
				return;
			Console.SetOut(newOut);
			try
			{
				methodInfo.Invoke(null, null);
			}
			finally
			{
				Console.SetOut(oldOut);
			}
			var declaringType = methodInfo.DeclaringType;
			if (declaringType == null)
				throw new Exception("should be!");
			var expectedOutput = string.Join("\n", GetExpectedOutputAttributes(declaringType).Select(a => a.Output));
			Console.WriteLine(newOut.ToString());
			Assert.AreEqual(PrepareOutput(expectedOutput), PrepareOutput(newOut.ToString()));
		}

		private static string PrepareOutput(string output)
		{
			return string.Join("\n", output.Trim().SplitToLines());
		}

		public static IEnumerable<ExpectedOutputAttribute> GetExpectedOutputAttributes(Type declaringType)
		{
			return declaringType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
				.SelectMany(m => m.GetCustomAttributes(typeof(ExpectedOutputAttribute)))
				.Cast<ExpectedOutputAttribute>();
		}

		public static IEnumerable<TestCaseData> GetVideos()
		{
			var course = new CourseLoader().LoadCourse(new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\Slides")));
			Assert.That(course.Slides.Count, Is.GreaterThan(0));
			return course.Slides
				.SelectMany(slide =>
					slide.Blocks.OfType<YoutubeBlock>()
						.Select(b => new TestCaseData(slide.Info.SlideFile.Name, b.VideoId)));
		}

		[TestCaseSource(nameof(GetVideos))]
		[Category("Long")]
		[Explicit]
		public void CheckAllYoutubeVideos(string slideName, string videoId)
		{
			var url = "https://www.youtube.com/oembed?format=json&url=http://www.youtube.com/watch?v=" + videoId;
			new WebClient().DownloadData(url);
		}

		private static void InitialCodeIsNotSolutionForProjExercise(ExerciseSlide slide)
		{
			var exercise = slide.Exercise as ProjectExerciseBlock;
			var directoryName = Path.Combine(exercise.SlideFolderPath.FullName, exercise.ExerciseDir);
			var excluded = (exercise.PathsToExcludeForChecker ?? new string[0]).Concat(new[] { "bin/*", "obj/*" }).ToList();
			var exerciseDir = new DirectoryInfo(directoryName);
			var bytes = exerciseDir.ToZip(excluded, new[]
			{
				new FileContent
				{
					Path = exercise.CsprojFileName,
					Data = ProjModifier.ModifyCsproj(exerciseDir.GetFile(exercise.CsprojFileName),
						proj => ProjModifier.PrepareForChecking(proj, exercise, excluded))
				}
			});
			var result = SandboxRunner.Run(new ProjRunnerSubmission
			{
				Id = slide.Id.ToString(),
				ZipFileData = bytes,
				ProjectFileName = exercise.CsprojFileName,
				Input = "",
				NeedRun = true
			});

			Console.WriteLine("Result = " + result);
			Assert.AreEqual(Verdict.Ok, result.Verdict);

			Assert.AreNotEqual("", result.Output);
		}

		private static void EthalonSolutionForSingleFileExercises(ExerciseSlide slide)
		{
			var exercise = (SingleFileExerciseBlock)slide.Exercise;
			var solution = exercise.BuildSolution(exercise.EthalonSolution);
			if (solution.HasErrors)
			{
				FailOnError(slide, solution, exercise.EthalonSolution);
				return;
			}

			var result = SandboxRunner.Run(exercise.CreateSubmition(
				slide.Id.ToString(),
				exercise.EthalonSolution), new SandboxRunnerSettings());

			var output = result.GetOutput().NormalizeEoln();

			var isRightAnswer = output.NormalizeEoln().Equals(slide.Exercise.ExpectedOutput.NormalizeEoln());
			if (!isRightAnswer)
			{
				Assert.Fail("mistake in: " + slide.Info.Unit.Title + " - " + slide.Title + "\n" +
							"\tActualOutput: " + output.NormalizeEoln() + "\n" +
							"\tExpectedOutput: " + slide.Exercise.ExpectedOutput.NormalizeEoln() + "\n" +
							"\tCompilationError: " + result.CompilationOutput + "\n" +
							"\tSourceCode: " + solution.SourceCode + "\n\n");
			}
		}

		[TestCaseSource(nameof(GetSlidesTestCases))]
		public void EthalonSolutionsForExercises(ExerciseSlide slide)
		{
			if (slide.Exercise is ProjectExerciseBlock)
				InitialCodeIsNotSolutionForProjExercise(slide);
			else
				EthalonSolutionForSingleFileExercises(slide);
		}

		private static void FailOnError(ExerciseSlide slide, SolutionBuildResult solution, string ethalonSolution)
		{
			Assert.Fail($@"Template solution: {ethalonSolution}
  
  source code: {solution.SourceCode}
  
  solution has error in: {slide.Info.Unit.Title} - {slide.Title}
  
  error: {solution.ErrorMessage}");
		}

		public static IEnumerable<TestCaseData> GetSlidesTestCases()
		{
			var course = new CourseLoader().LoadCourse(new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\Slides")));
			return
				from slide in course.Slides.OfType<ExerciseSlide>()
				select new TestCaseData(slide).SetName(course.Id + " - " + slide.Info.Unit.Title + " - " + slide.Title);
		}
	}
}