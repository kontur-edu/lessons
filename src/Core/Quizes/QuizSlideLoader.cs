using System.Collections.Immutable;
using System.IO;
using System.Linq;
using uLearn.Model;
using Ulearn.Common.Extensions;

namespace uLearn.Quizes
{
	public class QuizSlideLoader : ISlideLoader
	{
		public string Extension => ".quiz.xml";

		public Slide Load(FileInfo file, Unit unit, int slideIndex, string courseId, CourseSettings settings)
		{
			var quiz = file.DeserializeXml<Quiz>();
			quiz.Meta?.FixPaths(file);

			var scoringGroupsIds = settings.Scoring.Groups.Keys;
			if (!string.IsNullOrEmpty(quiz.ScoringGroup) && !scoringGroupsIds.Contains(quiz.ScoringGroup))
				throw new CourseLoadingException(
					$"Неизвестная группа оценки у теста {quiz.Title}: {quiz.ScoringGroup}\n" +
					"Возможные значения: " + string.Join(", ", scoringGroupsIds));

			if (string.IsNullOrEmpty(quiz.ScoringGroup))
				quiz.ScoringGroup = settings.Scoring.DefaultScoringGroupForQuiz;

			BuildUp(quiz, unit, courseId, settings);
			quiz.InitQuestionIndices();
			var slideInfo = new SlideInfo(unit, file, slideIndex);
			return new QuizSlide(slideInfo, quiz);
		}

		public static void BuildUp(Quiz quiz, Unit unit, string courseId, CourseSettings settings)
		{
			var context = new BuildUpContext(unit, settings, null, courseId, quiz.Title);
			var blocks = quiz.Blocks.SelectMany(b => b.BuildUp(context, ImmutableHashSet<string>.Empty));
			quiz.Blocks = blocks.ToArray();
		}
	}
}