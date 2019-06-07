using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Xml.Serialization;
using RunCsJob.Api;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Model;

namespace Ulearn.Core.Courses.Slides.Exercises.Blocks
{
	[XmlType("exercise.file")]
	public class SingleFileExerciseBlock : AbstractExerciseBlock
	{
		[XmlElement("solution")]
		public Label SolutionLabel { get; set; }

		[XmlElement("remove")]
		public Label[] RemovedLabels { get; set; }

		[XmlElement("prelude")]
		public string PreludeFile { get; set; }
		
		[XmlAttribute("file")]
		public string CodeFile { get; set; }

		public override IEnumerable<SlideBlock> BuildUp(SlideBuildingContext context, IImmutableSet<string> filesInProgress)
		{
			CodeFile = CodeFile ?? context.Slide.DefaultIncludeCodeFile ?? context.Unit.Settings?.DefaultIncludeCodeFile;
			if (CodeFile == null)
				throw new CourseLoadingException($"У блока <exercise.file> не указан атрибут file.");
			
			if (ExerciseInitialCode == null)
				throw new CourseLoadingException($"У блока <exercise.file> не указан код, который надо показывать пользователю перед началом работы. Укажите его в тэге <initialCode>");
			
			if (!Language.HasValue)
				Language = LanguageHelpers.GuessByExtension(new FileInfo(CodeFile));
			
			RemovedLabels = RemovedLabels ?? new Label[0];
			if (PreludeFile == null)
			{
				PreludeFile = context.CourseSettings.GetPrelude(Language);
				if (PreludeFile != null)
					PreludeFile = Path.Combine("..", PreludeFile);
			}

			var code = context.UnitDirectory.GetContent(CodeFile);
			var regionRemover = new RegionRemover(Language);
			var extractor = context.GetExtractor(CodeFile, Language, code);

			var prelude = "";
			if (PreludeFile != null)
				prelude = context.UnitDirectory.GetContent(PreludeFile);

			var exerciseCode = regionRemover.Prepare(code);
			exerciseCode = regionRemover.Remove(exerciseCode, RemovedLabels, out var _);
			exerciseCode = regionRemover.RemoveSolution(exerciseCode, SolutionLabel, out var index);
			if (index < 0)
				index = 0;
			index += prelude.Length;

			ExerciseInitialCode = ExerciseInitialCode.RemoveCommonNesting();
			ExerciseCode = prelude + exerciseCode;
			IndexToInsertSolution = index;
			EthalonSolution = extractor.GetRegion(SolutionLabel);
			Validator.ValidatorName = string.Join(" ", Language.GetName(), Validator.ValidatorName ?? "");

			yield return this;
		}

		// То, что будет выполняться для проверки задания
		[XmlIgnore]
		public string ExerciseCode { get; set; }

		// Индекс внутри ExerciseCode, куда нужно вставить код пользователя.
		[XmlIgnore]
		public int IndexToInsertSolution { get; set; }

		// Если это вставить в ExerciseCode по индексу IndexToInsertSolution и выполнить полученный код, он должен вывести ExpectedOutput
		[XmlIgnore]
		public string EthalonSolution { get; set; }

		// Временно создано для конвертации .cs-слайдов в .xml-слайды. Когда .cs-слайдов не останется, можно удалить
		[XmlIgnore]
		public List<string> ExcludedFromSolution { get; set; } = new List<string>();

		public override string GetSourceCode(string code)
		{
			return BuildSolution(code).SourceCode;
		}

		public override SolutionBuildResult BuildSolution(string userWrittenCode)
		{
			return new SolutionBuilder(IndexToInsertSolution, ExerciseCode, Validator).BuildSolution(userWrittenCode);
		}

		public override RunnerSubmission CreateSubmission(string submissionId, string code)
		{
			return new FileRunnerSubmission
			{
				Id = submissionId,
				Code = GetSourceCode(code),
				Input = "",
				NeedRun = true
			};
		}
	}
}