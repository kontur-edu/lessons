using System.Collections.Immutable;
using System.IO;
using System.Linq;
using uLearn.Model.Blocks;
using Ulearn.Common.Extensions;

namespace uLearn.Model
{
	public class LessonSlideLoader : ISlideLoader
	{
		public string Extension => "lesson.xml";

		public Slide Load(FileInfo file, Unit unit, int slideIndex, string courseId, CourseSettings settings)
		{
			var lesson = file.DeserializeXml<Lesson>();
			lesson.Meta?.FixPaths(file);

			var context = new BuildUpContext(unit, settings, lesson, courseId, lesson.Title);
			var blocks = lesson.Blocks.SelectMany(b => b.BuildUp(context, ImmutableHashSet<string>.Empty)).ToList();
			var slideInfo = new SlideInfo(unit, file, slideIndex);

			if (blocks.OfType<ExerciseBlock>().Any())
				return new ExerciseSlide(blocks, slideInfo, lesson.Title, lesson.Id, lesson.Meta);
			return new Slide(blocks, slideInfo, lesson.Title, lesson.Id, lesson.Meta);
		}
	}
}