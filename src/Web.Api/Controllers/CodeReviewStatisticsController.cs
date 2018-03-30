using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using uLearn;
using Ulearn.Web.Api.Models.Results.CodeReviewStatistics;

namespace Ulearn.Web.Api.Controllers
{
	[Route("/codereview/statistics")]
	public class CodeReviewStatisticsController : BaseController
	{
		private readonly SlideCheckingsRepo slideCheckingsRepo;
		private readonly UserRolesRepo userRolesRepo;
		private readonly UsersRepo usersRepo;
		private readonly GroupsRepo groupsRepo;

		public CodeReviewStatisticsController(ILogger logger, WebCourseManager courseManager,
			SlideCheckingsRepo slideCheckingsRepo,
			UserRolesRepo userRolesRepo,
			UsersRepo usersRepo,
			GroupsRepo groupsRepo)
			: base(logger, courseManager)
		{
			this.slideCheckingsRepo = slideCheckingsRepo;
			this.userRolesRepo = userRolesRepo;
			this.usersRepo = usersRepo;
			this.groupsRepo = groupsRepo;
		}
		
		[Route("{courseId}/instructors")]
		[Authorize(Policy = "CourseAdmins")]
		public async Task<IActionResult> InstructorsStatistics(Course course, int count=1000, DateTime? from=null, DateTime? to=null)
		{
			if (course == null)
				return NotFound();
			
			if (! from.HasValue)
				from = DateTime.MinValue;
			if (! to.HasValue)
				to = DateTime.MaxValue;
			
			count = Math.Max(count, 1000);
			
			var instructorIds = userRolesRepo.GetListOfUsersWithCourseRole(CourseRole.Instructor, course.Id);
			var instructors = usersRepo.GetUsersByIds(instructorIds);
			
			var exerciseSlides = course.Slides.OfType<ExerciseSlide>().ToList();

			var allSlideCheckings = await slideCheckingsRepo.GetManualCheckingQueueAsync<ManualExerciseChecking>(new ManualCheckingQueueFilterOptions
			{
				CourseId = course.Id,
				Count = count,
				OnlyChecked = null,
				From = from.Value,
				To = to.Value,
			}).Include(c => c.Reviews).ToListAsync();

			var result = new CodeReviewInstructorsStatisticsResult
			{
				Instructors = new List<CodeReviewInstructorStatistics>()
			};
			foreach (var instructor in instructors)
			{
				var checkingsCheckedByInstructor = allSlideCheckings.Where(c => c.IsChecked && (c.LockedById == instructor.Id || c.Reviews.Any(r => r.AuthorId == instructor.Id))).ToList();
				var instructorGroups = await groupsRepo.GetMyGroupsFilterAccessibleToUserAsync(course.Id, instructor.Id);
				var instructorGroupMemberIds = (await groupsRepo.GetGroupsMembersAsync(instructorGroups.Select(g => g.Id))).Select(m => m.UserId);
				var checkingQueue = allSlideCheckings.Where(c => !c.IsChecked && instructorGroupMemberIds.Contains(c.UserId)).ToList();
				var comments = checkingsCheckedByInstructor.SelectMany(c => c.NotDeletedReviews).ToList();
				var instructorStatistics = new CodeReviewInstructorStatistics
				{
					Instructor = BuildShortUserInfo(instructor),
					Exercises = exerciseSlides.ToDictionary(
						slide => slide.Id,
						slide => new CodeReviewExerciseStatistics
						{
							Exercise = BuildSlideInfo(course.Id, slide),
							ReviewedSubmissionsCount = checkingsCheckedByInstructor.Count(c => c.SlideId == slide.Id),
							QueueSize = checkingQueue.Count(c => c.SlideId == slide.Id),
							CommentsCount = comments.Count(c => c.ExerciseChecking.SlideId == slide.Id),
						}
					)
				};
				result.Instructors.Add(instructorStatistics);
			}

			return Json(result);
		}
	}
}