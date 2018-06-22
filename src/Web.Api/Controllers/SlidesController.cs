using System;
using Database;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using uLearn;

namespace Ulearn.Web.Api.Controllers
{
	[Route("/slides")]
	public class SlidesController : BaseController
	{
		public SlidesController(ILogger logger, WebCourseManager courseManager, UlearnDb db)
			: base(logger, courseManager, db)
		{
		}

		[HttpGet("{courseId}/{slideId}")]
		public IActionResult SlideInfo(Course course, Guid slideId)
		{
			var slide = course?.FindSlideById(slideId);
			if (slide == null)
				return Json(new { status = "error", message = "Course or slide not found" });

			return Json(BuildSlideInfo(course.Id, slide));
		}
	}
}