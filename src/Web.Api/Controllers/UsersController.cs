using System;
using System.Linq;
using Database;
using Database.Models;
using Database.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using uLearn;
using Ulearn.Web.Api.Models.Results.Users;

namespace Ulearn.Web.Api.Controllers
{
	[Route("/users")]
	public class UsersController : BaseController
	{
		private readonly UsersRepo usersRepo;
		private readonly UserRolesRepo userRolesRepo;

		public UsersController(ILogger logger, WebCourseManager courseManager, UsersRepo usersRepo, UserRolesRepo userRolesRepo)
			: base(logger, courseManager)
		{
			this.usersRepo = usersRepo ?? throw new ArgumentNullException(nameof(usersRepo));
			this.userRolesRepo = userRolesRepo ?? throw new ArgumentNullException(nameof(userRolesRepo));
		}

		[HttpGet("{courseId}/instructors")]
		[Authorize(Policy = "Instructors")]
		public IActionResult InstructorsList(Course course)
		{
			if (course == null)
				return NotFound();
					
			var instructorIds = userRolesRepo.GetListOfUsersWithCourseRole(CourseRole.Instructor, course.Id);
			var instructors = usersRepo.GetUsersByIds(instructorIds);
			return Json(new InstructorsListResult
			{
				Instructors = instructors.Select(BuildShortUserInfo).ToList()
			});
		}
	}
}