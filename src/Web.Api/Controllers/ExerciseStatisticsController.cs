using System;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using uLearn;
using Ulearn.Web.Api.Models.Results.ExerciseStatistics;

namespace Ulearn.Web.Api.Controllers
{
	[Route("/exercise/statistics")]
	public class ExerciseStatisticsController : BaseController
	{
		private readonly UserSolutionsRepo userSolutionsRepo;

		public ExerciseStatisticsController(ILogger logger, WebCourseManager courseManager, UserSolutionsRepo userSolutionsRepo)
			: base(logger, courseManager)
		{
			this.userSolutionsRepo = userSolutionsRepo;
		}

		[Route("{courseId}")]
		public async Task<IActionResult> CourseStatistics(Course course, int count=10000, DateTime? from=null, DateTime? to=null)
		{
			if (course == null)
				return NotFound();
			
			if (! from.HasValue)
				from = DateTime.MinValue;
			if (! to.HasValue)
				to = DateTime.MaxValue;

			count = Math.Max(count, 10000);
			
			var exerciseSlides = course.Slides.OfType<ExerciseSlide>().ToList();
			/* TODO (andgein): I can't select all submissions because ApplicationUserId column doesn't exist in database (ApplicationUser_Id exists).
			   We should remove this column after EF Core 2.1 release (and remove tuples here)
			*/
			var submissions = await userSolutionsRepo.GetAllSubmissions(course.Id)
				.Where(s => s.Timestamp >= from && s.Timestamp <= to)
				.OrderByDescending(s => s.Timestamp)
				.Take(count)
				.Select(s => Tuple.Create(s.SlideId, s.AutomaticCheckingIsRightAnswer, s.Timestamp))
				.ToListAsync();

			const int daysLimit = 30;
			var result = new CourseExercisesStatisticsResult
			{
				AnalyzedSubmissionsCount = submissions.Count,
				Exercises = exerciseSlides.ToDictionary(
					slide => slide.Id,
					slide =>
					{
						/* Statistics for this exercise slide: */
						var exerciseSubmissions = submissions.Where(s => s.Item1 == slide.Id).ToList();
						return new OneExerciseStatistics
						{
							Exercise = BuildSlideInfo(course.Id, slide),
							SubmissionsCount = exerciseSubmissions.Count,
							AcceptedCount = exerciseSubmissions.Count(s => s.Item2),
							/* Select last 30 (`datesLimit`) dates */
							LastDates = exerciseSubmissions.GroupBy(s => s.Item3.Date).OrderByDescending(g => g.Key).Take(daysLimit).ToDictionary(
								/* Date: */
								g => g.Key,
								/* Statistics for this date: */
								g => new OneExerciseStatisticsForDate
								{
									SubmissionsCount = g.Count(),
									AcceptedCount = g.Count(s => s.Item2)
								}
							)
						};
					})
			};

			return Json(result);
		}
	}
}