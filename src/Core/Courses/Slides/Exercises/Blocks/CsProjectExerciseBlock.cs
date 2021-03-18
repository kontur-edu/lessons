using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Microsoft.Build.Evaluation;
using Vostok.Logging.Abstractions;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Extensions;
using Ulearn.Core.Helpers;
using Ulearn.Core.NUnitTestRunning;
using Ulearn.Core.Properties;
using Ulearn.Core.RunCheckerJobApi;

namespace Ulearn.Core.Courses.Slides.Exercises.Blocks
{
	[XmlType("exercise.csproj")]
	public class CsProjectExerciseBlock : AbstractExerciseBlock, IExerciseCheckerZipBuilder
	{
		public const string BuildingTargetFrameworkVersion = "4.7.2";
		public const string BuildingTargetNetCoreFrameworkVersion = "3.1";
		public const string BuildingToolsVersion = null;

		private static ILog log => LogProvider.Get().ForContext(typeof(CsProjectExerciseBlock));

		public static string SolutionFilepathToUserCodeFilepath(string solutionFilepath)
		{
			// cut .solution.cs
			var userCodeFilenameWithoutExt = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(solutionFilepath));
			var userCodeFilename = $"{userCodeFilenameWithoutExt}.cs";
			var path = Path.GetDirectoryName(solutionFilepath);
			return path == null ? userCodeFilename : Path.Combine(path, userCodeFilename);
		}

		public CsProjectExerciseBlock()
		{
			HideExpectedOutputOnError = true;
			HideShowSolutionsButton = true;
			BuildEnvironmentOptions = new BuildEnvironmentOptions
			{
				TargetFrameworkVersion = BuildingTargetFrameworkVersion,
				TargetNetCoreFrameworkVersion = BuildingTargetNetCoreFrameworkVersion,
				ToolsVersion = BuildingToolsVersion,
			};
		}

		[XmlAttribute("csproj")]
		public string CsProjFilePath { get; set; }

		[XmlElement("startupObject")]
		public string StartupObject { get; set; } = "checking.CheckerRunner";

		[XmlElement("userCodeFile")]
		public string UserCodeFilePath { get; set; }

		[XmlElement("solutionFile")]
		public string SolutionFilePath { get; set; } // По умолчанию userCodeFile.solution.cs

		[XmlElement("excludePathForChecker")]
		public string[] PathsToExcludeForChecker { get; set; }

		[XmlElement("nunitTestClass")]
		public string[] NUnitTestClasses { get; set; }

		[XmlElement("excludePathForStudent")]
		public string[] PathsToExcludeForStudent { get; set; }

		[XmlElement("studentZipIsCompilable")]
		public bool StudentZipIsCompilable { get; set; } = true;

		public string ExerciseDirName => Path.GetDirectoryName(CsProjFilePath).EnsureNotNull("csproj должен быть в поддиректории");

		public string CsprojFileName => Path.GetFileName(CsProjFilePath);

		public FileInfo CsprojFile => ExerciseFolder.GetFile(CsprojFileName);

		public FileInfo UserCodeFile => ExerciseFolder.GetFile(UserCodeFilePath);

		public DirectoryInfo UserCodeFileParentDirectory => UserCodeFile.Directory;

		// В отличие от CorrectFullSolutionFile, здесь может содержаться не полное, а промежуточное решение серии задач, состоящей из доработток одного файла.
		public FileInfo CorrectSolutionFile => SolutionFilePath == null ? CorrectFullSolutionFile : ExerciseFolder.GetFile(SolutionFilePath);

		public string CorrectSolutionFileName => CorrectSolutionFile.Name;

		public FileInfo CorrectFullSolutionFile => UserCodeFileParentDirectory.GetFile(CorrectFullSolutionFileName);

		public DirectoryInfo ExerciseFolder => new DirectoryInfo(Path.Combine(SlideFolderPath.FullName, ExerciseDirName));

		public string UserCodeFileNameWithoutExt => Path.GetFileNameWithoutExtension(UserCodeFilePath);

		public string CorrectFullSolutionFileName => $"{UserCodeFileNameWithoutExt}.Solution.cs";

		public string CorrectFullSolutionPath => CorrectFullSolutionFile.GetRelativePath(ExerciseFolder.FullName);

		private Regex WrongAnswersAndSolutionNameRegex => new Regex(new Regex("^") + UserCodeFileNameWithoutExt + new Regex("\\.(.+)\\.cs"), RegexOptions.IgnoreCase);

		[XmlIgnore]
		public DirectoryInfo SlideFolderPath { get; set; }

		//public FileInfo StudentsZip => SlideFolderPath.GetFile(ExerciseDirName + ".exercise.zip");

		public bool IsWrongAnswer(string name) => WrongAnswersAndSolutionNameRegex.IsMatch(name) && !name.Contains(".Solution.");

		public BuildEnvironmentOptions BuildEnvironmentOptions { get; set; }
		
		[XmlIgnore]
		public Slide Slide { get; private set; }

		[XmlIgnore]
		public string CourseId { get; private set; }

		public override bool HasAutomaticChecking() => Language == Common.Language.CSharp;

		public override IEnumerable<SlideBlock> BuildUp(SlideBuildingContext context, IImmutableSet<string> filesInProgress)
		{
			if (!Language.HasValue)
				Language = context.CourseSettings.DefaultLanguage;
			Slide = context.Slide;
			CourseId = context.CourseId;
			ExerciseInitialCode = ExerciseInitialCode ?? "// Вставьте сюда финальное содержимое файла " + UserCodeFilePath;
			ExpectedOutput = ExpectedOutput ?? "";
			Validator.ValidatorName = string.Join(" ", Language.GetName(), Validator.ValidatorName ?? "");
			SlideFolderPath = context.UnitDirectory;

			ReplaceStartupObjectForNUnitExercises();

			yield return this;

			if (CorrectSolutionFile.Exists)
			{
				yield return new MarkdownBlock("### Решение") { Hide = true };
				yield return new CodeBlock(CorrectSolutionFile.ContentAsUtf8(), Language) { Hide = true };
			}
		}

		public void ReplaceStartupObjectForNUnitExercises()
		{
			/* Replace StartupObject if exercise uses NUnit tests. It should be after CreateZipForStudent() call */
			var useNUnitLauncher = NUnitTestClasses != null;
			StartupObject = useNUnitLauncher ? typeof(NUnitTestRunner).FullName : StartupObject;
		}

		public override string GetSourceCode(string code)
		{
			return code;
		}

		public override SolutionBuildResult BuildSolution(string userWrittenCodeFile)
		{
			var validator = ValidatorsRepository.Get(Validator);
			return validator.ValidateSolution(userWrittenCodeFile, userWrittenCodeFile);
		}

		public override RunnerSubmission CreateSubmission(string submissionId, string code)
		{
			using (var stream = GetZipForChecker(code))
			{
				return new ProjRunnerSubmission
				{
					Id = submissionId,
					ZipFileData = stream.ToArray(),
					ProjectFileName = CsprojFileName,
					Input = "",
					NeedRun = true,
					TimeLimit = TimeLimit
				};
			}
		}

		private MemoryStream GetZipForChecker(string code)
		{
			var codeFile = GetCodeFile(code);
			return ExerciseCheckerZipsCache.GetZip(this, codeFile.Path, codeFile.Data);
		}

		public MemoryStream GetZipForChecker()
		{
			log.Info($"Собираю zip-архив для проверки: курс {CourseId}, слайд «{Slide?.Title}» ({Slide?.Id})");
			var excluded = (PathsToExcludeForChecker ?? new string[0])
				.Concat(new[] { "/bin/", "/obj/", ".idea/", ".vs/" })
				.ToList();

			var toUpdate = GetAdditionalFiles(excluded).ToList();
			log.Info($"Собираю zip-архив для проверки: дополнительные файлы [{string.Join(", ", toUpdate.Select(c => c.Path))}]");

			var ms = ZipUtils.CreateZipFromDirectory(new List<string> { ExerciseFolder.FullName }, excluded, toUpdate, Encoding.UTF8);
			log.Info($"Собираю zip-архив для проверки: zip-архив собран, {ms.Length} байтов");
			return ms;
		}

		private FileContent GetCodeFile(string code)
		{
			return new FileContent { Path = UserCodeFilePath, Data = Encoding.UTF8.GetBytes(code) };
		}

		private IEnumerable<FileContent> GetAdditionalFiles(List<string> excluded)
		{
			var useNUnitLauncher = NUnitTestClasses != null;

			using (var stream = ProjModifier.ModifyCsproj(CsprojFile, ModifyCsproj(excluded, useNUnitLauncher), toolsVersion: BuildEnvironmentOptions.ToolsVersion))
				yield return new FileContent
				{
					Path = CsprojFileName,
					Data = stream.ToArray()
				};

			if (useNUnitLauncher)
			{
				yield return new FileContent { Path = GetNUnitTestRunnerFilename(), Data = CreateTestLauncherFile() };
			}

			foreach (var fileContent in ExerciseStudentZipBuilder.ResolveCsprojLinks(this))
				yield return fileContent;
		}

		private static string GetNUnitTestRunnerFilename()
		{
			return nameof(NUnitTestRunner) + ".cs";
		}

		private Action<Project> ModifyCsproj(List<string> excluded, bool addNUnitLauncher)
		{
			return proj =>
			{
				ProjModifier.PrepareForCheckingUserCode(proj, this, excluded);
				if (addNUnitLauncher)
					proj.AddItem("Compile", GetNUnitTestRunnerFilename());

				ProjModifier.SetBuildEnvironmentOptions(proj, BuildEnvironmentOptions);
			};
		}

		private byte[] CreateTestLauncherFile()
		{
			var data = Resources.NUnitTestRunner;

			var oldTestFilter = "\"SHOULD_BE_REPLACED\"";
			var newTestFilter = string.Join(",", NUnitTestClasses.Select(x => $"\"{x}\""));
			var newData = data.Replace(oldTestFilter, newTestFilter);

			newData = newData.Replace("WillBeMain", "Main");

			return Encoding.UTF8.GetBytes(newData);
		}
	}
}