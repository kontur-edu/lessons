﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Database;
using Database.Models;
using Database.Models.Comments;
using Database.Repos;
using Database.Repos.Groups;
using Database.Repos.Users;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Vostok.Logging.Abstractions;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Web.Api.Controllers.Groups;
using Ulearn.Web.Api.Models.Common;
using Ulearn.Web.Api.Models.Responses.Notifications;

namespace Ulearn.Web.Api.Controllers
{
	/* See https://docs.microsoft.com/en-us/aspnet/core/web-api/index?view=aspnetcore-2.1#annotate-class-with-apicontrollerattribute
	   for detailed description of ApiControllerAttribute */
	[ApiController]
	[Produces("application/json")]
	public class BaseController : Controller
	{
		private static ILog log => LogProvider.Get().ForContext(typeof(BaseController));
		protected readonly IWebCourseManager courseManager;
		protected readonly UlearnDb db;
		protected readonly IUsersRepo usersRepo;

		protected string UserId => User.GetUserId();
		protected bool IsAuthenticated => User.Identity.IsAuthenticated;

		public BaseController(IWebCourseManager courseManager, UlearnDb db, IUsersRepo usersRepo)
		{
			this.courseManager = courseManager;
			this.db = db;
			this.usersRepo = usersRepo;
		}

		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			DisableEfChangesTrackingForGetRequests(context);

			await next().ConfigureAwait(false);
		}

		private void DisableEfChangesTrackingForGetRequests(ActionContext context)
		{
			/* Disable change tracking in EF Core for GET requests due to performance issues */
			/* TODO (andgein): we need a way to enable change tracking for some GET requests in future */
			var isRequestSafe = context.HttpContext.Request.Method == "GET"; // Maybe for HEAD and OPTION requests too?
			db.ChangeTracker.AutoDetectChangesEnabled = !isRequestSafe;

			if (isRequestSafe)
			{
				log.Debug("Выключаю автоматическое отслеживание изменений в EF Core: db.ChangeTracker.AutoDetectChangesEnabled = false");
			}
		}

		protected async Task<bool> IsSystemAdministratorAsync()
		{
			var user = await usersRepo.FindUserByIdAsync(UserId).ConfigureAwait(false);
			return usersRepo.IsSystemAdministrator(user);
		}

		public static ShortUserInfo BuildShortUserInfo(ApplicationUser user, bool discloseLogin = false, bool discloseEmail = false)
		{
			return new ShortUserInfo
			{
				Id = user.Id,
				Login = discloseLogin ? user.UserName : null,
				Email = discloseEmail ? user.Email : null,
				FirstName = user.FirstName ?? "",
				LastName = user.LastName ?? "",
				VisibleName = user.VisibleName,
				AvatarUrl = user.AvatarUrl,
				Gender = user.Gender,
			};
		}

		protected NotificationCommentInfo BuildNotificationCommentInfo(Comment comment)
		{
			if (comment == null)
				return null;

			return new NotificationCommentInfo
			{
				Id = comment.Id,
				CourseId = comment.CourseId,
				SlideId = comment.SlideId,
				PublishTime = comment.PublishTime,
				Author = BuildShortUserInfo(comment.Author),
				Text = comment.Text,
			};
		}

		protected ShortGroupInfo BuildShortGroupInfo(Group g)
		{
			return new ShortGroupInfo
			{
				Id = g.Id,
				Name = g.Name,
				CourseId = g.CourseId,
				IsArchived = g.IsArchived,
				ApiUrl = Url.Action(new UrlActionContext { Action = nameof(GroupController.Group), Controller = "Group", Values = new { groupId = g.Id } })
			};
		}

		public static async Task<Func<Slide, int>> BuildGetSlideMaxScoreFunc(IUserSolutionsRepo solutionsRepo, IUserQuizzesRepo userQuizzesRepo, IVisitsRepo visitsRepo, IGroupsRepo groupsRepo,
			Course course, string userId)
		{
			if (userId == null)
				return GetMaxScoreForGuest;
			var solvedSlidesIds = await GetSolvedSlides(solutionsRepo, userQuizzesRepo, course, userId);
			var slidesWithUsersManualChecking = (await visitsRepo.GetSlidesWithUsersManualChecking(course.Id, userId)).ToImmutableHashSet();
			var enabledManualCheckingForUser = await groupsRepo.IsManualCheckingEnabledForUserAsync(course, userId).ConfigureAwait(false);
			return s => GetMaxScoreForUsersSlide(s, solvedSlidesIds.Contains(s.Id), slidesWithUsersManualChecking.Contains(s.Id), enabledManualCheckingForUser);
		}

		[NotNull]
		public static async Task<Func<Slide, string>> BuildGetGitEditLinkFunc(string userId, Course course,
			ICourseRolesRepo courseRolesRepo, ICoursesRepo coursesRepo)
		{
			var courseRole = await courseRolesRepo.GetRoleAsync(userId, course.Id);
			var canEditGit = courseRole <= CourseRoleType.CourseAdmin;
			if (!canEditGit)
				return s => null;
			var publishedCourseVersion = await coursesRepo.GetPublishedCourseVersionAsync(course.Id);
			if (publishedCourseVersion == null)
				return s => null;
			var repoUrl = publishedCourseVersion.RepoUrl;
			var pathToCourseXml = publishedCourseVersion.PathToCourseXml;
			if (repoUrl == null || pathToCourseXml == null)
				return s => null;
			var courseXmlDirectory = course.CourseXmlDirectory;
			return slide =>
			{
				var pathRelative2CourseXml = slide.Info.SlideFile.FullName.Substring(courseXmlDirectory.FullName.Length + 1);
				return GitUtils.GetSlideEditLink(repoUrl, pathToCourseXml, pathRelative2CourseXml);
			};
		}

		public static Func<Slide, int> BuildGetSlideMaxScoreFunc(Course course, Group group)
		{
			var enabledManualCheckingForGroup = course.Settings.IsManualCheckingEnabled || group.IsManualCheckingEnabled;
			return s => GetMaxScoreForUsersSlide(s, false, false, enabledManualCheckingForGroup);
		}

		public static async Task<HashSet<Guid>> GetSolvedSlides(IUserSolutionsRepo solutionsRepo, IUserQuizzesRepo userQuizzesRepo, Course course, string userId)
		{
			var solvedSlides = await solutionsRepo.GetIdOfPassedSlides(course.Id, userId);
			solvedSlides.UnionWith(await userQuizzesRepo.GetPassedSlideIdsAsync(course.Id, userId));
			return solvedSlides;
		}

		public static int GetMaxScoreForUsersSlide(Slide slide, bool isSolved, bool hasManualChecking, bool enabledManualCheckingForUser)
		{
			var isExerciseOrQuiz = slide is ExerciseSlide || slide is QuizSlide;

			if (!isExerciseOrQuiz)
				return slide.MaxScore;

			if (isSolved)
				return hasManualChecking ? slide.MaxScore : GetMaxScoreWithoutManualChecking(slide);
			return enabledManualCheckingForUser ? slide.MaxScore : GetMaxScoreWithoutManualChecking(slide);
		}
		
		private static int GetMaxScoreForGuest(Slide slide)
		{
			var isExerciseOrQuiz = slide is ExerciseSlide || slide is QuizSlide;

			if (!isExerciseOrQuiz)
				return slide.MaxScore;

			return GetMaxScoreWithoutManualChecking(slide);
		}

		public static int GetMaxScoreWithoutManualChecking(Slide slide)
		{
			switch (slide)
			{
				case ExerciseSlide exerciseSlide:
					return exerciseSlide.Scoring.PassedTestsScore;
				case QuizSlide quizSlide:
					return quizSlide.ManualChecking ? 0 : quizSlide.MaxScore;
				default:
					return slide.MaxScore;
			}
		}
	}
}