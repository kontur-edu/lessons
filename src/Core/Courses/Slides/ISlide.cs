using System;

namespace Ulearn.Core.Courses.Slides
{
	public interface ISlide
	{
		Guid Id { get; }
		
		string Title { get; }
		
		SlideBlock[] Blocks { get; }
	}
}