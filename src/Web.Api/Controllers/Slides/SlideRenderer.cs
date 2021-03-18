using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Database.Models;
using Database.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ulearn.Common;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Courses.Slides.Flashcards;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Slides.Quizzes.Blocks;
using Ulearn.Web.Api.Clients;
using Ulearn.Web.Api.Models.Common;
using Ulearn.Web.Api.Models.Responses.Exercise;
using Ulearn.Web.Api.Models.Responses.SlideBlocks;

namespace Ulearn.Web.Api.Controllers.Slides
{
	public class SlideRenderer
	{
		private readonly IUlearnVideoAnnotationsClient videoAnnotationsClient;
		private readonly IUserSolutionsRepo solutionsRepo;
		private readonly ICourseRolesRepo courseRolesRepo;
		private readonly ISlideCheckingsRepo slideCheckingsRepo;

		public SlideRenderer(IUlearnVideoAnnotationsClient videoAnnotationsClient,
			IUserSolutionsRepo solutionsRepo, ISlideCheckingsRepo slideCheckingsRepo, ICourseRolesRepo courseRolesRepo)
		{
			this.videoAnnotationsClient = videoAnnotationsClient;
			this.solutionsRepo = solutionsRepo;
			this.slideCheckingsRepo = slideCheckingsRepo;
			this.courseRolesRepo = courseRolesRepo;
		}

		public ShortSlideInfo BuildShortSlideInfo(string courseId, Slide slide, Func<Slide, int> getSlideMaxScoreFunc, Func<Slide, string> getGitEditLink, IUrlHelper urlHelper)
		{
			return BuildShortSlideInfo<ShortSlideInfo>(courseId, slide, getSlideMaxScoreFunc, getGitEditLink, urlHelper);
		}

		private T BuildShortSlideInfo<T>(string courseId, Slide slide, Func<Slide, int> getSlideMaxScoreFunc, Func<Slide, string> getGitEditLink, IUrlHelper urlHelper)
			where T : ShortSlideInfo, new()
		{
			return new T
			{
				Id = slide.Id,
				Title = slide.Title,
				Hide = slide.Hide,
				Slug = slide.Url,
				ApiUrl = urlHelper.Action("SlideInfo", "Slides", new { courseId = courseId, slideId = slide.Id }),
				MaxScore = getSlideMaxScoreFunc(slide),
				ScoringGroup = slide.ScoringGroup,
				Type = GetSlideType(slide),
				QuestionsCount = slide.Blocks.OfType<AbstractQuestionBlock>().Count(),
				ContainsVideo = slide.Blocks.OfType<YoutubeBlock>().Any(),
				GitEditLink = getGitEditLink(slide),
				QuizMaxTriesCount = slide is QuizSlide quizSlide ? quizSlide.MaxTriesCount : 0,
			};
		}

		private static SlideType GetSlideType(Slide slide)
		{
			switch (slide)
			{
				case ExerciseSlide _:
					return SlideType.Exercise;
				case QuizSlide _:
					return SlideType.Quiz;
				case FlashcardSlide _:
					return SlideType.Flashcards;
				default:
					return SlideType.Lesson;
			}
		}

		public async Task<ApiSlideInfo> BuildSlideInfo(SlideRenderContext slideRenderContext, Func<Slide, int> getSlideMaxScoreFunc, Func<Slide, string> getGitEditLink)
		{
			var result = BuildShortSlideInfo<ApiSlideInfo>(slideRenderContext.CourseId, slideRenderContext.Slide, getSlideMaxScoreFunc, getGitEditLink, slideRenderContext.UrlHelper);

			result.Blocks = new List<IApiSlideBlock>();
			foreach (var b in slideRenderContext.Slide.Blocks)
				result.Blocks.AddRange(await ToApiSlideBlocks(b, slideRenderContext));

			return result;
		}

		public async Task<IEnumerable<IApiSlideBlock>> ToApiSlideBlocks(SlideBlock slideBlock, SlideRenderContext context)
		{
			if (context.RemoveHiddenBlocks && slideBlock.Hide)
				return new IApiSlideBlock[] {};
			var apiSlideBlocks = (IEnumerable<IApiSlideBlock>)await RenderBlock((dynamic)slideBlock, context);
			if (context.RemoveHiddenBlocks)
				apiSlideBlocks = apiSlideBlocks.Where(b => !b.Hide);
			return apiSlideBlocks;
		}
		
		private async Task<IEnumerable<IApiSlideBlock>> RenderBlock(SlideBlock b, SlideRenderContext context)
		{
			return Enumerable.Empty<IApiSlideBlock>();
		}

		private async Task<IEnumerable<IApiSlideBlock>> RenderBlock(CodeBlock b, SlideRenderContext context)
		{
			return new[] { new CodeBlockResponse(b) };
		}
		
		private async Task<IEnumerable<IApiSlideBlock>> RenderBlock(HtmlBlock b, SlideRenderContext context)
		{
			return new[] { new HtmlBlockResponse(b, false) };
		}
		
		private async Task<IEnumerable<IApiSlideBlock>> RenderBlock(ImageGalleryBlock b, SlideRenderContext context)
		{
			return new[] { new ImageGalleryBlockResponse(b) };
		}
		
		private async Task<IEnumerable<IApiSlideBlock>> RenderBlock(TexBlock b, SlideRenderContext context)
		{
			return new[] { new TexBlockResponse(b) };
		}

		private async Task<IEnumerable<IApiSlideBlock>> RenderBlock(SpoilerBlock sb, SlideRenderContext context)
		{
			var innerBlocks = new List<IApiSlideBlock>();
			foreach (var b in sb.Blocks)
				innerBlocks.AddRange(await ToApiSlideBlocks(b, context));
			if (sb.Hide)
				innerBlocks.ForEach(b => b.Hide = true);
			return new [] { new SpoilerBlockResponse(sb, innerBlocks) };
		}

		private static async Task<IEnumerable<IApiSlideBlock>> RenderBlock(MarkdownBlock mb, SlideRenderContext context)
		{
			var renderedMarkdown = mb.RenderMarkdown(context.CourseId, context.Slide, context.BaseUrl);
			var parsedBlocks = ParseBlocksFromMarkdown(renderedMarkdown);
			if (mb.Hide)
				parsedBlocks.ForEach(b => b.Hide = true);
			return parsedBlocks;
		}

		private async Task<IEnumerable<IApiSlideBlock>> RenderBlock(YoutubeBlock yb, SlideRenderContext context)
		{
			var annotation = await videoAnnotationsClient.GetVideoAnnotations(context.VideoAnnotationsGoogleDoc, yb.VideoId);
			var googleDocLink = string.IsNullOrEmpty(context.VideoAnnotationsGoogleDoc) ? null
				: "https://docs.google.com/document/d/" + context.VideoAnnotationsGoogleDoc;
			var response = new YoutubeBlockResponse(yb, annotation, googleDocLink);
			return new [] { response };
		}

		private async Task<IEnumerable<IApiSlideBlock>> RenderBlock(AbstractExerciseBlock b, SlideRenderContext context)
		{
			var submissions = await solutionsRepo
				.GetAllSubmissionsByUser(context.CourseId, context.Slide.Id, context.UserId)
				.Include(s => s.AutomaticChecking).ThenInclude(c => c.Output)
				.Include(s => s.AutomaticChecking).ThenInclude(c => c.CompilationError)
				.Include(s => s.AutomaticChecking).ThenInclude(c => c.DebugLogs)
				.Include(s => s.SolutionCode)
				.Include(s => s.Reviews).ThenInclude(c => c.Author)
				.Include(s => s.ManualCheckings).ThenInclude(c => c.Reviews).ThenInclude(r => r.Author)
				.ToListAsync();
			var codeReviewComments = await slideCheckingsRepo.GetExerciseCodeReviewComments(context.CourseId, context.Slide.Id, context.UserId);
			var isCourseAdmin = await courseRolesRepo.HasUserAccessToCourseAsync(context.UserId, context.CourseId, CourseRoleType.CourseAdmin);
			
			ExerciseAttemptsStatistics exerciseAttemptsStatistics = null;
			if (b.HasAutomaticChecking())
			{
				var exerciseUsersCount = await slideCheckingsRepo.GetExerciseUsersCount(context.CourseId, context.Slide.Id);
				var exerciseUsersWithRightAnswerCount = await slideCheckingsRepo.GetExerciseUsersWithRightAnswerCount(context.CourseId, context.Slide.Id);
				var lastSuccessAttemptDate = await slideCheckingsRepo.GetExerciseLastRightAnswerDate(context.CourseId, context.Slide.Id);
				exerciseAttemptsStatistics = new ExerciseAttemptsStatistics
				{
					AttemptedUsersCount = exerciseUsersCount,
					UsersWithRightAnswerCount = exerciseUsersWithRightAnswerCount,
					LastSuccessAttemptDate = lastSuccessAttemptDate
				};
			}

			var exerciseSlideRendererContext = new ExerciseSlideRendererContext
			{
				Submissions = submissions,
				CodeReviewComments = codeReviewComments,
				SlideFile = context.Slide.Info.SlideFile,
				CanSeeCheckerLogs = isCourseAdmin,
				AttemptsStatistics = exerciseAttemptsStatistics,
			};
			return new[] { new ExerciseBlockResponse(b, exerciseSlideRendererContext) };
		}

		private static List<IApiSlideBlock> ParseBlocksFromMarkdown(string renderedMarkdown)
		{
			var parser = new HtmlParser();
			var document = parser.ParseDocument(renderedMarkdown);
			var rootElements = document.Body.Children;
			var blocks = new List<IApiSlideBlock>();
			foreach (var element in rootElements)
			{
				var tagName = element.TagName.ToLower();
				if (tagName == "textarea")
				{
					var langStr = element.GetAttribute("data-lang");
					var lang = (Language)Enum.Parse(typeof(Language), langStr, true);
					var code = element.TextContent;
					blocks.Add(new CodeBlockResponse { Code = code, Language = lang });
				}
				else if (tagName == "img")
				{
					var href = element.GetAttribute("href");
					blocks.Add(new ImageGalleryBlockResponse { ImageUrls = new[] { href } });
				}
				else if (tagName == "p"
						&& element.Children.Length == 1
						&& string.Equals(element.Children[0].TagName, "img", StringComparison.OrdinalIgnoreCase)
						&& string.IsNullOrWhiteSpace(element.TextContent))
				{
					var href = element.Children[0].GetAttribute("src");
					blocks.Add(new ImageGalleryBlockResponse { ImageUrls = new[] { href } });
				}
				else
				{
					var htmlContent = element.OuterHtml;
					if (blocks.Count > 0 && blocks.Last() is HtmlBlockResponse last)
					{
						htmlContent = last.Content + "\n" + htmlContent;
						blocks[blocks.Count - 1] = new HtmlBlockResponse { Content = htmlContent, FromMarkdown = true };
					}
					else
						blocks.Add(new HtmlBlockResponse { Content = htmlContent, FromMarkdown = true });
				}
			}
			return blocks;
		}
	}
}