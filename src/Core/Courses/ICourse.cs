using System;
using System.Collections.Generic;
using System.IO;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Units;

namespace Ulearn.Core.Courses
{
	public interface ICourse
	{
		string Id { get; set; }
		string Title { get; }
		CourseSettings Settings { get; }
		DirectoryInfo CourseXmlDirectory { get; }
		List<Unit> Units { get; }
		List<Slide> Slides { get; }
		
		Slide FindSlideById(Guid slideId);
		Slide GetSlideById(Guid slideId);
		Slide FindSlideByIndex(int index);
		Unit FindUnitById(Guid unitId);
		Unit GetUnitById(Guid unitId);
		Unit FindUnitBySlideId(Guid slideId);
	}
}