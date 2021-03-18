using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Helpers;
using Ulearn.Core.Model;
using Ulearn.Core.RunCheckerJobApi;

namespace Ulearn.Core.Courses.Slides.Exercises.Blocks
{
	[XmlType("exercise.universal")]
	public class UniversalExerciseBlock : AbstractExerciseBlock, IExerciseCheckerZipBuilder
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(UniversalExerciseBlock));

		[XmlAttribute("exerciseDirName")] // Путь до директории с упражнением
		public string ExerciseDirPath { get; set; }

		[XmlElement("userCodeFile")]
		public string UserCodeFilePath { get; set; }

		[XmlElement("solutionFilePath")]
		public string SolutionFilePath { get; set; } // По умолчанию используется UserCodeFilePath

		[XmlElement("region")]
		public virtual string Region { get; set; } // Студент видит в консоли и редактирует регион с этим именем. <prefix>region label <...> <prefix>rendregion label. Должен быть как в решении, так и в файле заглушке.

		[XmlElement("excludePathForChecker")]
		public virtual string[] PathsToExcludeForChecker { get; set; }

		[CanBeNull]
		[XmlElement("includePathForChecker")]
		public string[] PathsToIncludeForChecker { get; set; } // Пути до директорий относительно директории с Unit, чьё содержимое нужно включить в архив. Перетирают файлы из ExerciseDir в архиве

		[XmlElement("excludePathForStudent")]
		public string[] PathsToExcludeForStudent { get; set; } // Шаблоны путей до файлов внутри ExerciseDir.

		[XmlElement("checkInitialSolution")]
		public virtual bool CheckInitialSolution { get; set; } = true;

		[XmlAttribute("noStudentZip")] // Не отдавать zip студенту
		public virtual bool NoStudentZip { get; set; }

		[XmlElement("dockerImageName")] // см. DockerImageNameRegex
		public virtual string DockerImageName { get; set; }

		[XmlIgnore]
		public static readonly Regex DockerImageNameRegex = new Regex("^[-_a-z.]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		[XmlElement("run")]
		public string RunCommand { get; set; } // см. RunCommandRegex

		[XmlIgnore]
		public static readonly Regex RunCommandRegex = new Regex("^[-_a-zA-Z.0-9 ;,|>]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		[XmlIgnore]
		public DirectoryInfo UnitDirectory { get; set; }

		[XmlIgnore]
		public DirectoryInfo ExerciseDirectory => new DirectoryInfo(Path.Combine(UnitDirectory.FullName, ExerciseDirPath));

		[XmlIgnore]
		public DirectoryInfo CourseDirectory { get; set; }

		[XmlIgnore]
		public FileInfo UserCodeFile => ExerciseDirectory.GetFile(UserCodeFilePath);

		[XmlIgnore]
		public string InitialUserCodeFilePath
		{
			get
			{
				var parts = UserCodeFilePath.Split('.');
				return string.Join(".", parts.Take(parts.Length - 1).Concat(new[] { "Initial", parts.Last() }));
			}
		}

		[XmlIgnore]
		public FileInfo InitialUserCodeFile => ExerciseDirectory.GetFile(InitialUserCodeFilePath);

		[XmlIgnore]
		public FileInfo CorrectSolutionFile => SolutionFilePath == null ? UserCodeFile : ExerciseDirectory.GetFile(SolutionFilePath);

		[XmlIgnore]
		public string CorrectSolutionFilePath => SolutionFilePath ?? UserCodeFilePath;

		[XmlIgnore]
		private static readonly string[] wrongAnswerPatterns = { "*.WrongAnswer.*", "*.WA.*" };

		[XmlIgnore]
		private static readonly string[] initialPatterns = { "*.Initial.*" };

		[XmlIgnore]
		private static readonly string[] solutionPatterns = { "*.Solution.*" };

		[XmlIgnore]
		public List<FileInfo> WrongAnswerFiles =>
			wrongAnswerPatterns.SelectMany(p => ExerciseDirectory.GetFiles(p, SearchOption.AllDirectories))
				.ToList();

		[XmlIgnore]
		private List<FileInfo> InitialFiles =>
			initialPatterns.SelectMany(p => ExerciseDirectory.GetFiles(p, SearchOption.AllDirectories))
				.ToList();

		private string InitialFilePathToNotInitial(string path)
		{
			var notInitial = path;
			initialPatterns.ForEach(p => notInitial = notInitial.Replace(p.Replace(".*", "").Replace("*", ""), ""));
			return notInitial;
		}

		[XmlIgnore]
		public Lazy<string> SolutionRegionContent;

		[XmlIgnore]
		public Lazy<string> InitialRegionContent;

		[XmlIgnore]
		public Slide Slide { get; private set; }

		[XmlIgnore]
		public string CourseId { get; private set; }

		public override string GetSourceCode(string code)
		{
			return code;
		}

		public override bool HasAutomaticChecking() => true;

		public override IEnumerable<SlideBlock> BuildUp(SlideBuildingContext context, IImmutableSet<string> filesInProgress)
		{
			if (!Language.HasValue)
				Language = LanguageHelpers.GuessByExtension(new FileInfo(UserCodeFilePath));
			Slide = context.Slide;
			CourseId = context.CourseId;
			UnitDirectory = context.UnitDirectory;
			CourseDirectory = context.CourseDirectory;
			ExpectedOutput = ExpectedOutput ?? "";
			SolutionRegionContent = new Lazy<string>(() => GetRegionContent(CorrectSolutionFile));
			InitialRegionContent = new Lazy<string>(() => GetRegionContent(InitialUserCodeFile));
			ExerciseInitialCode = NoStudentZip
				? GetNoStudentZipInitialCode()
				: ExerciseInitialCode ?? "// Вставьте сюда финальное содержимое файла " + UserCodeFilePath;
			yield return this;

			var correctSolution = GetCorrectSolution();
			if (correctSolution != null)
			{
				yield return new MarkdownBlock("### Решение") { Hide = true };
				yield return new CodeBlock(correctSolution, Language) { Hide = true };
			}
		}

		public string GetCorrectSolution()
		{
			return SolutionRegionContent.Value ?? (CorrectSolutionFile.Exists ? CorrectSolutionFile.ContentAsUtf8() : null);
		}

		public string GetInitialCode()
		{
			return NoStudentZip ? GetNoStudentZipInitialCode() : InitialUserCodeFile.ContentAsUtf8();
		}

		private string GetNoStudentZipInitialCode()
		{
			return ExerciseInitialCode
				= InitialRegionContent.Value
				?? (InitialUserCodeFile.Exists ? InitialUserCodeFile.ContentAsUtf8() : null)
				?? ExerciseInitialCode
				?? "";
		}

		public string GetRegionContent(FileInfo file)
		{
			return Region == null || !file.Exists
				? null
				: new CommonSingleRegionExtractor(file.ContentAsUtf8()).GetRegion(new Label { Name = Region });
		}

		public override SolutionBuildResult BuildSolution(string userWrittenCode)
		{
			return new SolutionBuildResult(userWrittenCode);
		}

		public override RunnerSubmission CreateSubmission(string submissionId, string code)
		{
			using (var stream = GetZipForChecker(code))
			{
				return new CommandRunnerSubmission
				{
					Id = submissionId,
					ZipFileData = stream.ToArray(),
					// ReSharper disable once PossibleInvalidOperationException
					Language = Language.Value,
					DockerImageName = DockerImageName,
					RunCommand = RunCommand,
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
				.Concat(initialPatterns)
				.Concat(wrongAnswerPatterns)
				.Concat(solutionPatterns)
				.Concat(new[] { "/bin/", "/obj/", ".idea/", ".vs/" })
				.ToList();

			var toUpdateDirectories = PathsToIncludeForChecker
				.EmptyIfNull()
				.Select(pathToInclude => new DirectoryInfo(Path.Combine(UnitDirectory.FullName, pathToInclude)))
				.Select(d => d.FullName);
			var directoriesToInclude = toUpdateDirectories.Append(ExerciseDirectory.FullName).ToList();

			var ms = ZipUtils.CreateZipFromDirectory(directoriesToInclude, excluded, null, Encoding.UTF8);
			log.Info($"Собираю zip-архив для проверки: zip-архив собран, {ms.Length} байтов");
			return ms;
		}

		private FileContent GetCodeFile(string code)
		{
			if (Region == null)
				return new FileContent { Path = UserCodeFilePath, Data = Encoding.UTF8.GetBytes(code) };
			var fullCode = new CommonSingleRegionExtractor((InitialUserCodeFile.Exists ? InitialUserCodeFile : UserCodeFile).ContentAsUtf8())
				.ReplaceRegionContent(new Label { Name = Region }, code);
			return new FileContent { Path = UserCodeFilePath, Data = Encoding.UTF8.GetBytes(fullCode) };
		}

		public MemoryStream GetZipMemoryStreamForStudent()
		{
			var excluded = (PathsToExcludeForStudent ?? new string[0])
				.Concat(initialPatterns)
				.Concat(wrongAnswerPatterns)
				.Concat(solutionPatterns)
				.Concat(new[] { "/checking/", "/bin/", "/obj/", ".idea/", ".vs/" })
				.ToList();

			var toUpdate = ReplaceWithInitialFiles().ToList();

			var zipMemoryStream = ZipUtils.CreateZipFromDirectory(new List<string> { ExerciseDirectory.FullName }, excluded, toUpdate, Encoding.UTF8);

			log.Info($"Собираю zip-архив для студента: zip-архив собран, {zipMemoryStream.Length} байтов");
			return zipMemoryStream;
		}

		private IEnumerable<FileContent> ReplaceWithInitialFiles()
		{
			foreach (var initialFile in InitialFiles)
			{
				var relativeFilePath = initialFile.GetRelativePath(ExerciseDirectory.FullName);
				var notInitial = InitialFilePathToNotInitial(relativeFilePath);
				yield return new FileContent { Path = notInitial, Data = initialFile.ReadAllContent() };
			}
		}
	}
}