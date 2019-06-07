using System.IO;
using Ulearn.Core.Courses.Units;

namespace Ulearn.Core.Courses.Slides
{
	public class SlideLoadingContext
	{
		public string CourseId { get; private set; }
		public CourseSettings CourseSettings { get; private set; }
		
		public DirectoryInfo UnitDirectory { get; }
		public FileInfo SlideFile { get; }
		public int SlideIndex { get; private set; }
		
		public Unit Unit { get; }

		public SlideLoadingContext(string courseId, Unit unit, CourseSettings courseSettings, FileInfo slideFile, int slideIndex)
		{
			CourseId = courseId;
			
			Unit = unit;
			UnitDirectory = unit.Directory;
			
			CourseSettings = courseSettings;
			SlideFile = slideFile;
			SlideIndex = slideIndex;
		}

		public SlideLoadingContext(CourseLoadingContext courseLoadingContext, Unit unit, FileInfo slideFile, int slideIndex)
			: this(courseLoadingContext.CourseId, unit, courseLoadingContext.CourseSettings, slideFile, slideIndex)
		{
			
		}
	}
}