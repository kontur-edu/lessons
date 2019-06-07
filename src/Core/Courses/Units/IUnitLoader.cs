using System.IO;

namespace Ulearn.Core.Courses.Units
{
	public interface IUnitLoader
	{
		Unit Load(FileInfo unitFile, CourseLoadingContext context, int firstSlideIndex);
	}
}