using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Xml.Serialization;
using RunCsJob.Api;
using Ulearn.Common.Extensions;

namespace uLearn.Model.Blocks
{
	[XmlType("single-file-exercise")]
	public class SingleFileExerciseBlock : ExerciseBlock
	{
		[XmlElement("solution")]
		public Label SolutionLabel { get; set; }

		[XmlElement("remove")]
		public Label[] RemovedLabels { get; set; }

		[XmlElement("prelude")]
		public string PreludeFile { get; set; }

		public override IEnumerable<SlideBlock> BuildUp(BuildUpContext context, IImmutableSet<string> filesInProgress)
		{
			FillProperties(context);
			RemovedLabels = RemovedLabels ?? new Label[0];
			if (PreludeFile == null)
			{
				PreludeFile = context.CourseSettings.GetPrelude(LangId);
				if (PreludeFile != null)
					PreludeFile = Path.Combine("..", PreludeFile);
			}

			var code = context.Dir.GetContent(CodeFile);
			var regionRemover = new RegionRemover(LangId);
			var extractor = context.GetExtractor(CodeFile, LangId, code);

			var prelude = "";
			if (PreludeFile != null)
				prelude = context.Dir.GetContent(PreludeFile);

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
			Validator.ValidatorName = string.Join(" ", LangId, Validator.ValidatorName ?? "");

			CheckScoringGroup(context.SlideTitle, context.CourseSettings.Scoring);

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