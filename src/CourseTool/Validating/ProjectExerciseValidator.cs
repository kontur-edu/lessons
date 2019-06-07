using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.VisualBasic.FileIO;
using RunCsJob;
using RunCsJob.Api;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Helpers;
using SearchOption = Microsoft.VisualBasic.FileIO.SearchOption;

namespace uLearn.CourseTool.Validating
{
	public class ProjectExerciseValidator : BaseValidator
	{
		private readonly CsProjectExerciseBlock ex;
		private readonly ExerciseSlide slide;
		private readonly SandboxRunnerSettings settings;
		private readonly ExerciseStudentZipBuilder exerciseStudentZipBuilder = new ExerciseStudentZipBuilder();

		public ProjectExerciseValidator(BaseValidator baseValidator, SandboxRunnerSettings settings, ExerciseSlide slide, CsProjectExerciseBlock exercise)
			: base(baseValidator)
		{
			this.settings = settings;
			this.slide = slide;
			ex = exercise;
		}

		public void ValidateExercises()
		{
			if (ReportErrorIfExerciseFolderMissesRequiredFiles())
				return;

			ReportWarningIfWrongAnswersAreSolutionsOrNotOk();

			if (!ReportWarningIfExerciseFolderDoesntContainSolutionFile())
				ReportErrorIfSolutionNotBuildingOrNotPassesTests();

			ReportErrorIfStudentsZipHasErrors();

			ReportErrorIfInitialCodeIsSolutionOrVerdictNotOk();
		}

		private bool ReportWarningIfExerciseFolderDoesntContainSolutionFile()
		{
			if (ExerciseDirectoryContainsSolutionFile())
				return false;
			ReportSlideWarning(slide, $"Exercise directory doesn't contain {ex.CorrectSolutionFileName}");
			return true;
		}

		private bool ReportErrorIfExerciseFolderMissesRequiredFiles()
		{
			var exerciseFilesRelativePaths = FileSystem.GetFiles(ex.ExerciseFolder.FullName, SearchOption.SearchAllSubDirectories)
				.Select(path => new FileInfo(path).GetRelativePath(ex.ExerciseFolder.FullName))
				.ToList();

			return ReportErrorIfMissingCsproj() | ReportErrorIfMissingUserCodeFile();

			bool ReportErrorIfMissingUserCodeFile() => ReportErrorIfMissingFile(ex.UserCodeFilePath);
			bool ReportErrorIfMissingCsproj() => ReportErrorIfMissingFile(ex.CsprojFileName);

			bool ReportErrorIfMissingFile(string path)
			{
				if (exerciseFilesRelativePaths.Any(p => p.Equals(path, StringComparison.InvariantCultureIgnoreCase)))
					return false;
				ReportSlideError(slide, $"Exercise folder ({ex.ExerciseFolder.Name}) doesn't contain ({path})");
				return true;
			}
		}

		private bool ExerciseDirectoryContainsSolutionFile()
			=> ex.UserCodeFileParentDirectory.GetFiles().Any(f => f.Name.Equals(ex.CorrectSolutionFileName, StringComparison.InvariantCultureIgnoreCase));

		private void ReportErrorIfSolutionNotBuildingOrNotPassesTests()
		{
			var solutionCode = ex.CorrectSolutionFile.ContentAsUtf8();
			var submission = ex.CreateSubmission(ex.CsprojFileName, solutionCode);
			var result = SandboxRunner.Run(submission, new SandboxRunnerSettings());

			if (!IsCompiledAndExecuted(result))
				ReportSlideError(slide, $"Correct solution file {ex.CorrectSolutionFileName} verdict is not OK. RunResult = {result}");

			if (!ex.IsCorrectRunResult(result))
				ReportSlideError(slide, $"Correct solution file {ex.CorrectSolutionFileName} is not solution. RunResult = {result}. " +
										$"ExpectedOutput = {ex.ExpectedOutput.NormalizeEoln()} " +
										$"RealOutput = {result.GetOutput().NormalizeEoln()}");
			var buildResult = ex.BuildSolution(solutionCode);

			if (buildResult.HasStyleErrors)
			{
				var errorMessage = string.Join("\n", buildResult.StyleErrors.Select(e => e.GetMessageWithPositions()));
				ReportSlideWarning(slide, $"Correct solution file {ex.CorrectSolutionFileName} has style issues. {errorMessage}");
			}
		}

		private void ReportWarningIfWrongAnswersAreSolutionsOrNotOk()
		{
			var filesWithWrongAnswer = FileSystem.GetFiles(ex.ExerciseFolder.FullName, SearchOption.SearchAllSubDirectories)
				.Select(name => new FileInfo(name))
				.Where(f => ex.IsWrongAnswer(f.Name));

			foreach (var waFile in filesWithWrongAnswer)
			{
				var result = SandboxRunner.Run(ex.CreateSubmission(waFile.Name, waFile.ContentAsUtf8()));

				ReportWarningIfWrongAnswerVerdictIsNotOk(waFile.Name, result);
				ReportWarningIfWrongAnswerIsSolution(waFile.Name, result);
			}
		}

		private void ReportWarningIfWrongAnswerVerdictIsNotOk(string waFileName, RunningResults waResult)
		{
			if (!IsCompiledAndExecuted(waResult))
				ReportSlideWarning(slide, $"Code verdict of file with wrong answer ({waFileName}) is not OK. RunResult = " + waResult);
		}

		private void ReportWarningIfWrongAnswerIsSolution(string waFileName, RunningResults waResult)
		{
			if (ex.IsCorrectRunResult(waResult))
				ReportSlideWarning(slide, $"Code of file with wrong answer ({waFileName}) is solution!");
		}

		private void ReportErrorIfInitialCodeIsSolutionOrVerdictNotOk()
		{
			var initialCode = ex.UserCodeFile.ContentAsUtf8();
			var submission = ex.CreateSubmission(ex.CsprojFileName, initialCode);
			var result = SandboxRunner.Run(submission);

			if (ex.StudentZipIsCompilable)
				ReportErrorIfInitialCodeVerdictIsNotOk(result);

			ReportErrorIfInitialCodeIsSolution(result);
		}

		private void ReportErrorIfInitialCodeVerdictIsNotOk(RunningResults result)
		{
			if (!IsCompiledAndExecuted(result))
				ReportSlideError(slide, "Exercise initial code verdict is not OK. RunResult = " + result);
		}

		private void ReportErrorIfInitialCodeIsSolution(RunningResults result)
		{
			if (ex.IsCorrectRunResult(result))
				ReportSlideError(slide, "Exercise initial code (available to students) is solution!");
		}

		private void ReportErrorIfStudentsZipHasErrors()
		{
			var tempExZipFilePath = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).GetFile($"{slide.Id}.exercise.zip");
			var tempExFolder = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExerciseFolder_From_StudentZip"));
			
			exerciseStudentZipBuilder.BuildStudentZip(slide, tempExZipFilePath);
			Utils.UnpackZip(tempExZipFilePath.ReadAllContent(), tempExFolder.FullName);
			try
			{
				ReportErrorIfStudentsZipHasWrongAnswerOrSolutionFiles(tempExFolder);

				var csprojFile = tempExFolder.GetFile(ex.CsprojFileName);
				FuncUtils.Using(
					new ProjectCollection(),
					projectCollection =>
					{
						var csproj = new Project(csprojFile.FullName, null, null, projectCollection);
						ReportErrorIfCsprojHasUserCodeOfNotCompileType(tempExFolder, csproj);
						ReportErrorIfCsprojHasWrongAnswerOrSolutionItems(tempExFolder, csproj);
					}, 
					projectCollection => projectCollection.UnloadAllProjects());

				if (!ex.StudentZipIsCompilable)
					return;

				var buildResult = MsBuildRunner.BuildProject(settings.MsBuildSettings, ex.CsprojFile.Name, tempExFolder);
				ReportErrorIfStudentsZipNotBuilding(buildResult);
			}
			finally
			{
				tempExFolder.Delete(true);
				tempExZipFilePath.Delete();
			}
		}

		private void ReportErrorIfStudentsZipNotBuilding(MSbuildResult res)
		{
			if (!res.Success)
				ReportSlideError(slide, ex.CsprojFileName + " not building! " + res);
		}

		private void ReportErrorIfStudentsZipHasWrongAnswerOrSolutionFiles(DirectoryInfo unpackedZipDir)
		{
			var wrongAnswersOrSolution = GetOrderedFileNames(unpackedZipDir.GetRelativePathsOfFiles(), ExerciseStudentZipBuilder.IsAnyWrongAnswerOrAnySolution);

			if (wrongAnswersOrSolution.Any())
				ReportSlideError(slide, $"Student zip exercise directory has 'wrong answer' and/or solution files ({string.Join(", ", wrongAnswersOrSolution)})");
		}

		private string[] GetOrderedFileNames(IEnumerable<string> names, Func<string, bool> predicate) => names.Where(predicate).OrderBy(n => n).ToArray();

		private void ReportErrorIfCsprojHasUserCodeOfNotCompileType(DirectoryInfo unpackedZipDir, Project csproj)
		{
			var userCode = csproj.Items.Single(i => i.UnevaluatedInclude.Equals(ex.UserCodeFilePath, StringComparison.InvariantCultureIgnoreCase));

			if (!userCode.ItemType.Equals("Compile", StringComparison.InvariantCultureIgnoreCase))
				ReportSlideError(slide, $"Student's csproj has user code item ({userCode.UnevaluatedInclude}) of not compile type");
		}

		private void ReportErrorIfCsprojHasWrongAnswerOrSolutionItems(DirectoryInfo unpackedZipDir, Project csproj)
		{
			var csProjItems = csproj.Items.Select(i => i.UnevaluatedInclude);

			var wrongAnswersOrSolution = GetOrderedFileNames(csProjItems, ExerciseStudentZipBuilder.IsAnyWrongAnswerOrAnySolution);

			if (wrongAnswersOrSolution.Any())
				ReportSlideError(slide, $"Student's csproj has 'wrong answer' and/or solution items ({string.Join(", ", wrongAnswersOrSolution)})");
		}
	}
}