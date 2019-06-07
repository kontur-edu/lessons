using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using RunCsJob.Api;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Model.Edx.EdxComponents;
using Component = Ulearn.Core.Model.Edx.EdxComponents.Component;

namespace Ulearn.Core.Courses.Slides.Exercises.Blocks
{
	public abstract class AbstractExerciseBlock : SlideBlock
	{
		protected AbstractExerciseBlock()
		{
			Validator = new ValidatorDescription();
		}

		[XmlAttribute("type")]
		public ExerciseType ExerciseType { get; set; } = ExerciseType.CheckOutput;
		
		/* .NET XML Serializer doesn't understand nullable fields, so we use this hack to make Language? field */
		[XmlIgnore]
		public Language? Language { get; set; }

		#region NullableLanguageHack
		
		[XmlAttribute("language")]
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public Language LanguageSerialized
		{
			get
			{
				Debug.Assert(Language != null, nameof(Language) + " != null");
				return Language.Value;
			}
			set => Language = value;
		}

		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeLanguageSerialized()
		{
			return Language.HasValue;
		}
		#endregion

		[XmlElement("initialCode")]
		public string ExerciseInitialCode { get; set; }
		
		[XmlElement("hint")]
		public List<string> Hints { get; set; }

		[XmlElement("comment")]
		public string CommentAfterExerciseIsSolved { get; set; }

		[XmlElement("expected")]
		// Ожидаемый корректный вывод программы
		public string ExpectedOutput { get; set; }

		[XmlElement("hideExpectedOutput")]
		public bool HideExpectedOutputOnError { get; set; }

		[XmlElement("validator")]
		public ValidatorDescription Validator { get; set; }

		[XmlElement("texts")]
		public ExerciseTexts Texts { get; set; } = new ExerciseTexts();

		[XmlElement("checkForPlagiarism")]
		public bool CheckForPlagiarism { get; set; } = true;

		[XmlElement("hideSolutions")]
		public bool HideShowSolutionsButton { get; set; }

		[XmlIgnore]
		public List<string> HintsMd
		{
			get
			{
				return Hints = Hints?.Select(h => h.RemoveCommonNesting()).ToList() ?? new List<string>();
			}
		}

		public abstract string GetSourceCode(string code);

		public abstract SolutionBuildResult BuildSolution(string userWrittenCode);

		public abstract RunnerSubmission CreateSubmission(string submissionId, string code);

		#region equals

		private bool Equals(AbstractExerciseBlock other)
		{
			return Equals(ExerciseInitialCode, other.ExerciseInitialCode) && Equals(ExpectedOutput, other.ExpectedOutput) && Equals(HintsMd, other.HintsMd);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((AbstractExerciseBlock)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = ExerciseInitialCode?.GetHashCode() ?? 0;
				hashCode = (hashCode * 397) ^ (ExpectedOutput?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (HintsMd?.GetHashCode() ?? 0);
				return hashCode;
			}
		}

		#endregion
		
		public override string ToString()
		{
			return $"Exercise: {ExerciseInitialCode}, Hints: {string.Join("; ", HintsMd)}";
		}

		public override Component ToEdxComponent(string displayName, string courseId, Slide slide, int componentIndex, string ulearnBaseUrl, DirectoryInfo coursePackageRoot)
		{
			throw new NotSupportedException();
		}

		public override string TryGetText()
		{
			return (ExerciseInitialCode ?? "") + '\n'
					+ string.Join("\n", HintsMd) + '\n'
					+ (CommentAfterExerciseIsSolved ?? "");
		}

		public bool IsCorrectRunResult(RunningResults result)
		{
			if (result.Verdict != Verdict.Ok)
				return false;

			if (ExerciseType == ExerciseType.CheckExitCode)
				return true;

			if (ExerciseType == ExerciseType.CheckOutput)
			{
				var expectedOutput = ExpectedOutput.NormalizeEoln();
				return result.Output.NormalizeEoln().Equals(expectedOutput);
			}

			throw new InvalidOperationException($"Unknown exercise type for checking: {ExerciseType}");
		}
	}

	public class ExerciseTexts
	{
		// «Все тесты пройдены». Показывается тем, у кого потенциально бывает код-ревью, но это решение не отправлено на него.
		// Например, потому что есть более новое решение.
		[XmlElement("allTestsPassed")]
		public string AllTestsPassed { get; set; }
		
		// «Все тесты пройдены, задача сдана». Показывается тем, у кого не бывает код-ревью. Например, вольнослушателям.
		[XmlElement("allTestsPassedWithoutReview")]
		public string AllTestsPassedWithoutReview { get; set; }
		
		// «Все тесты пройдены, за&nbsp;код-ревью {0} +{1}». Вместо {0} и {1} подставляются слова «получено»/«получен» и «X балл/балла/баллов». 
		[XmlElement("codeReviewPassed")]
		public string CodeReviewPassed { get; set; }
		
		// «Все тесты пройдены, за&nbsp;<a href=\"{0}\" title=\"Отредактировать код-ревью\">код-ревью</a> {1} +{2}».
		// Аналогично предыдущему, только со ссылкой на редактирование код-ревью для преподавателя.
		[XmlElement("codeReviewPassedInstructorView")]
		public string CodeReviewPassedInstructorView { get; set; }
		
		// «Все тесты пройдены, решение ожидает код-ревью». Показывается студенту.
		[XmlElement("waitingForCodeReview")]
		public string WaitingForCodeReview { get; set; }
		
		// «Все тесты пройдены, решение ожидает <a href=\"{0}\" title=\"Перейти к код-ревью\">код-ревью</a>». Показывается преподавателю.
		// Вместо {0} подставляется ссылка на код-ревью. 
		[XmlElement("waitingForCodeReviewInstructorView")]
		public string WaitingForCodeReviewInstructorView { get; set; }
	}

	public enum ExerciseType
	{
		[XmlEnum("check-exit-code")]
		CheckExitCode,
		
		[XmlEnum("check-output")]
		CheckOutput
	}

	public class ValidatorDescription
	{
		public ValidatorDescription()
		{
			ValidatorName = "";
		}
		
		[XmlAttribute("removeDefaults")]
		[DefaultValue(false)]
		public bool RemoveDefaults { get; set; }

		[XmlText]
		public string ValidatorName { get; set; }
	}
}