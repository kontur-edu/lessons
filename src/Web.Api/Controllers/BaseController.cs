using System.Linq;
using Database;
using Database.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using uLearn;
using uLearn.Quizes;
using Ulearn.Web.Api.Models.Common;
using SlideInfo = Ulearn.Web.Api.Models.Common.SlideInfo;

namespace Ulearn.Web.Api.Controllers
{
	public class BaseController : Controller
	{
		protected readonly ILogger logger;
		protected readonly WebCourseManager courseManager;

		public BaseController(ILogger logger, WebCourseManager courseManager)
		{
			this.logger = logger;
			this.courseManager = courseManager;
		}
		
		protected UnitInfo BuildUnitInfo(string courseId, Unit unit)
		{
			return new UnitInfo
			{
				Id = unit.Id,
				Title = unit.Title,
				Slides = unit.Slides.Select(slide => BuildSlideInfo(courseId, slide)).ToList()
			};
		}

		protected SlideInfo BuildSlideInfo(string courseId, Slide slide)
		{
			return new SlideInfo
			{
				Id = slide.Id,
				Title = slide.Title,
				Slug = slide.Url,
				ApiUrl = Url.Action(nameof(SlidesController.SlideInfo), "Slides", new { courseId = courseId, slideId = slide.Id }),
				MaxScore = slide.MaxScore,
				Type = GetSlideType(slide)
			};
		}

		protected static SlideType GetSlideType(Slide slide)
		{
			switch (slide)
			{
				case ExerciseSlide _:
					return SlideType.Exercise;
				case QuizSlide _:
					return SlideType.Quiz;
				default:
					return SlideType.Lesson;
			}
		}

		protected ShortUserInfo BuildShortUserInfo(ApplicationUser user)
		{
			return new ShortUserInfo
			{
				Id = user.Id,
				Login = user.UserName,
				FirstName = user.FirstName ?? "",
				LastName = user.LastName ?? "",
				VisibleName = user.VisibleName,
			};
		}
	}
}