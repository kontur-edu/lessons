using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using RunCsJob;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Extensions;

namespace uLearn.CourseTool.Validating
{
	public class CourseValidator : BaseValidator
	{
		private readonly List<Slide> slides;
		private readonly SandboxRunnerSettings settings;

		public CourseValidator(List<Slide> slides, SandboxRunnerSettings settings)
		{
			this.slides = slides;
			this.settings = settings;
		}

		public void ValidateExercises() // todo логирование log4net в файл (ошибки отдельно) и на консоль
		{
			foreach (var slide in slides.OfType<ExerciseSlide>())
			{
				LogSlideProcessing("Validate exercise", slide);

				if (slide.Exercise is CsProjectExerciseBlock exercise)
				{
					new ProjectExerciseValidator(this, settings, slide, exercise).ValidateExercises();
				}
				else
					ReportIfEthalonSolutionHasErrorsOrIssues(slide);
			}
		}

		private void LogSlideProcessing(string prefix, Slide slide)
		{
			LogInfoMessage(prefix + " " + slide.Info.Unit.Title + " - " + slide.Title);
		}

		public void ValidateVideos()
		{
			var videos = GetVideos().ToLookup(d => d.Item2, d => d.Item1);
			foreach (var g in videos.Where(g => g.Count() > 1))
				ReportError("Duplicate videos on slides " + string.Join(", ", g));
			foreach (var g in videos)
			{
				var slide = g.First();
				LogSlideProcessing("Validate video", slide);
				var url = "https://www.youtube.com/oembed?format=json&url=http://www.youtube.com/watch?v=" + g.Key;
				try
				{
					new WebClient().DownloadData(url);
				}
				catch (Exception e)
				{
					ReportSlideError(slide, "Slide contains not accessible video. " + e.Message);
				}
			}
		}

		public IEnumerable<Tuple<Slide, string>> GetVideos()
		{
			return slides
				.SelectMany(slide =>
					slide.Blocks.OfType<YoutubeBlock>()
						.Select(b => Tuple.Create(slide, b.VideoId)));
		}

		private void ReportIfEthalonSolutionHasErrorsOrIssues(ExerciseSlide slide)
		{
			var exercise = (SingleFileExerciseBlock)slide.Exercise;
			var ethalon = exercise.EthalonSolution.RemoveCommonNesting();
			var solution = exercise.BuildSolution(ethalon);
			if (solution.HasErrors)
			{
				FailOnError(slide, solution, ethalon);
				return;
			}
			if (solution.HasStyleErrors)
			{
				var errorMessages = string.Join("\n", solution.StyleErrors.Select(e => e.GetMessageWithPositions()));
				ReportSlideWarning(slide, "Style issue(s): " + errorMessages);
			}

			var result = SandboxRunner.Run(exercise.CreateSubmission(slide.Id.ToString(), ethalon), settings);

			var output = result.GetOutput().NormalizeEoln();

			var isRightAnswer = output.NormalizeEoln().Equals(slide.Exercise.ExpectedOutput.NormalizeEoln());
			if (!isRightAnswer)
			{
				ReportSlideError(slide,
					"Ethalon solution does not provide right answer\n" +
					"ActualOutput: " + output.NormalizeEoln() + "\n" +
					"ExpectedOutput: " + slide.Exercise.ExpectedOutput.NormalizeEoln() + "\n" +
					"CompilationError: " + result.CompilationOutput + "\n" +
					"SourceCode: " + solution.SourceCode + "\n\n");
			}
		}

		private void FailOnError(ExerciseSlide slide, SolutionBuildResult solution, string ethalonSolution)
		{
			ReportSlideError(slide, $@"ETHALON SOLUTION:
{ethalonSolution}
SOURCE CODE: 
{solution.SourceCode}
ERROR:
{solution.ErrorMessage}");
		}

		public void ValidateSpelling(Course course)
		{
			LogInfoMessage("Spell checking...");
			var sw = Stopwatch.StartNew();
			var errors = course.SpellCheck();
			foreach (string error in errors)
			{
				ReportError("Spelling: " + error);
			}
			LogInfoMessage($"Spell checking done in {sw.ElapsedMilliseconds} ms");
		}
	}
}