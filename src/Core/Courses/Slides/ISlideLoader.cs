using System.IO;
using Ulearn.Core.Courses.Units;

namespace Ulearn.Core.Courses.Slides
{
	public interface ISlideLoader
	{
		Slide Load(SlideLoadingContext context);
	}
}