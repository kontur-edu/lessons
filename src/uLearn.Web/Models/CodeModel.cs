using System;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;

namespace uLearn.Web.Models
{
	public class CodeModel
	{
		public AbstractExerciseBlock ExerciseBlock;
		public string CourseId;
		public BlockRenderContext Context;
		public Guid SlideId;
	}
}