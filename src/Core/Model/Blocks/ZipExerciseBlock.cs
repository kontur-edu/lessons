using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Xml.Serialization;
using RunCsJob.Api;
using uLearn.Helpers;
using Ulearn.Common.Extensions;

namespace uLearn.Model.Blocks
{
	[XmlType("zip-exercise")]
	public class ZipExerciseBlock : ExerciseBlock
	{
		[XmlIgnore]
		public DirectoryInfo SlideFolderPath { get; set; }

		[XmlElement("exercise-dir-name")]
		public string ExerciseDirName { get; set; }

		[XmlElement("user-code-file-path")]
		public string UserCodeFilePath { get; set; }

		public DirectoryInfo ExerciseFolder => new DirectoryInfo(Path.Combine(SlideFolderPath.FullName, ExerciseDirName));

		public FileInfo UserCodeFile => ExerciseFolder.GetFile(UserCodeFilePath);

		public override IEnumerable<SlideBlock> BuildUp(BuildUpContext context, IImmutableSet<string> filesInProgress)
		{
			FillProperties(context);
			ExerciseInitialCode = ExerciseInitialCode ?? "// Вставьте сюда финальное содержимое файла " + UserCodeFilePath;
			ExpectedOutput = ExpectedOutput ?? "";
			Validator.ValidatorName = string.Join(" ", LangId, Validator.ValidatorName ?? "");
			SlideFolderPath = context.Dir;

			CheckScoringGroup(context.SlideTitle, context.CourseSettings.Scoring);

			yield return this;

			yield break;

			if (UserCodeFile.Exists)
			{
				yield return new MdBlock("### Решение") { Hide = true };
				yield return new CodeBlock(UserCodeFile.ContentAsUtf8(), LangId, LangVer) { Hide = true };
			}
		}

		public override string GetSourceCode(string code)
		{
			return code;
		}

		public override SolutionBuildResult BuildSolution(string userWrittenCode)
		{
			return new SolutionBuildResult(userWrittenCode);
		}

		public override RunnerSubmission CreateSubmission(string submissionId, string code)
		{
			return new ZipRunnerSubmission
			{
				Id = submissionId,
				ZipFileData = SubmissionPacker.Pack(SubmissionLanguageHelpers.ByLangId(LangId),
					ExerciseFolder, UserCodeFilePath, code),
				LangId = LangId,
				Input = "",
				NeedRun = true
			};
		}
	}
}