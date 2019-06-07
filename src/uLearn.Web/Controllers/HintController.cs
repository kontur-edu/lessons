using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Database;
using Database.DataContexts;
using Microsoft.AspNet.Identity;
using uLearn.Web.FilterAttributes;
using uLearn.Web.Models;
using Ulearn.Core;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;

namespace uLearn.Web.Controllers
{
	[ULearnAuthorize]
	public class HintController : Controller
	{
		private readonly CourseManager courseManager;
		private readonly SlideHintRepo slideHintRepo;

		public HintController()
			: this(WebCourseManager.Instance, new SlideHintRepo(new ULearnDb()))
		{
		}

		public HintController(CourseManager courseManager, SlideHintRepo slideHintRepo)
		{
			this.courseManager = courseManager;
			this.slideHintRepo = slideHintRepo;
		}

		[HttpPost]
		public async Task<ActionResult> UseHint(string courseId, Guid slideId, bool isNeedNewHint)
		{
			var slide = courseManager.GetCourse(courseId).GetSlideById(slideId);
			if (!(slide is ExerciseSlide))
				return Json(new { Text = "Для слайда нет подсказок" });
			var exerciseSlide = (ExerciseSlide)slide;
			if (exerciseSlide.Exercise.HintsMd.Count == 0)
				return Json(new { Text = "Для слайда нет подсказок" });
			var model = new HintPageModel { Hints = await GetNewHintHtml(exerciseSlide, courseId, isNeedNewHint) };
			if (model.Hints == null)
				return Json(new { Text = "Подсказок больше нет" });
			return PartialView(model);
		}

		private async Task<HintWithLikeButton[]> GetNewHintHtml(ExerciseSlide exerciseSlide, string courseId, bool isNeedNewHint)
		{
			var usedHintsCount = slideHintRepo.GetUsedHintsCount(courseId, exerciseSlide.Id, User.Identity.GetUserId());
			if (usedHintsCount < exerciseSlide.Exercise.HintsMd.Count)
				return await RenderHtmlWithHint(exerciseSlide, isNeedNewHint ? usedHintsCount : usedHintsCount - 1, courseId);
			if (isNeedNewHint)
				return null;
			return await RenderHtmlWithHint(exerciseSlide, usedHintsCount - 1, courseId);
		}

		private async Task<HintWithLikeButton[]> RenderHtmlWithHint(ExerciseSlide exerciseSlide, int hintsCount, string courseId)
		{
			hintsCount = Math.Min(hintsCount, exerciseSlide.Exercise.HintsMd.Count - 1);
			var ans = new HintWithLikeButton[hintsCount + 1];
			for (var i = 0; i <= hintsCount; i++)
			{
				var isLiked = slideHintRepo.IsHintLiked(courseId, exerciseSlide.Id, User.Identity.GetUserId(), i);
				ans[i] = await MakeExerciseHint(exerciseSlide.Exercise.HintsMd[i].RenderMarkdown(exerciseSlide.Info.SlideFile), i, courseId, exerciseSlide.Id, isLiked);
			}
			return ans;
		}

		private async Task<HintWithLikeButton> MakeExerciseHint(string hintText, int hintId, string courseId, Guid slideId, bool isLiked)
		{
			await slideHintRepo.AddHint(User.Identity.GetUserId(), hintId, courseId, slideId);
			return new HintWithLikeButton
			{
				CourseId = courseId,
				Hint = hintText,
				HintId = hintId,
				IsLiked = isLiked,
				SlideId = slideId
			};
		}

		[HttpPost]
		public async Task<string> LikeHint(string courseId, Guid slideId, int hintId)
		{
			return await slideHintRepo.LikeHint(courseId, slideId, hintId, User.Identity.GetUserId());
		}
	}
}