using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using uLearn;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Slides.Quizzes.Blocks;
using Ulearn.Core.Courses.Units;

namespace Stepik.Api
{
	public enum UploadVideoToStepikOption
	{
		Iframe,
		NativeVideo
	}

	public abstract class CourseExportOptions
	{
		public int StepikCourseId { get; protected set; }

		public string XQueueName { get; protected set; }

		public UploadVideoToStepikOption VideoUploadOptions = UploadVideoToStepikOption.Iframe;

		public TimeSpan PauseAfterVideoUploaded = TimeSpan.FromMinutes(1);
	}

	public class CourseInitialExportOptions : CourseExportOptions
	{
		public CourseInitialExportOptions(int stepikCourseId, string xQueueName, IEnumerable<Guid> slideIdsWhereNewLessonsStart)
		{
			StepikCourseId = stepikCourseId;
			XQueueName = xQueueName;
			SlideIdsWhereNewLessonsStart = new HashSet<Guid>(slideIdsWhereNewLessonsStart);
		}

		public HashSet<Guid> SlideIdsWhereNewLessonsStart { get; }
	}

	public class CourseUpdateOptions : CourseExportOptions
	{
		public CourseUpdateOptions(int stepikCourseId, string xQueueName, List<SlideUpdateOptions> slidesUpdateOptions, IEnumerable<Guid> slideIdsWhereNewLessonsStart)
		{
			SlidesUpdateOptions = slidesUpdateOptions;
			StepikCourseId = stepikCourseId;
			XQueueName = xQueueName;
			SlideIdsWhereNewLessonsStart = new HashSet<Guid>(slideIdsWhereNewLessonsStart);
		}

		public HashSet<Guid> SlideIdsWhereNewLessonsStart { get; }

		public List<SlideUpdateOptions> SlidesUpdateOptions { get; }
	}

	public class SlideUpdateOptions
	{
		public SlideUpdateOptions(Guid slideId, int insertAfterStep, List<int> removeStepsIds)
		{
			SlideId = slideId;
			InsertAfterStep = insertAfterStep;
			RemoveStepsIds = removeStepsIds;
		}

		public Guid SlideId { get; }

		public int InsertAfterStep { get; }

		public List<int> RemoveStepsIds { get; }
	}

	public class CourseExportResults
	{
		public CourseExportResults(string ulearnCourseId, int stepikCourseId)
		{
			UlearnCourseId = ulearnCourseId;
			StepikCourseId = stepikCourseId;
			SlideIdToStepsIdsMap = new DefaultDictionary<Guid, List<int>>();
		}

		public string UlearnCourseId { get; }

		public int StepikCourseId { get; }

		public string StepikCourseTitle { get; set; }

		public DefaultDictionary<Guid, List<int>> SlideIdToStepsIdsMap { get; }

		public string Log;

		public Exception Exception;

		public void Info(string message)
		{
			Log += $"[INFO] {message}\n";
		}

		public void Error(string message)
		{
			Log += $"[ERROR] {message}\n";
		}
	}

	public class CourseExporter
	{
		private const string stepikCSharpLanguageName = "mono c#";

		private readonly string ulearnBaseUrl;
		private readonly StepikApiClient client;
		private readonly YoutubeVideoUrlExtractor youtubeVideoUrlExtractor;

		public CourseExporter(string stepikClientId, string stepikClientSecret, string ulearnBaseUrl, string accessToken)
		{
			this.ulearnBaseUrl = ulearnBaseUrl;
			client = new StepikApiClient(new StepikApiOptions
			{
				ClientId = stepikClientId,
				ClientSecret = stepikClientSecret,
				AccessToken = accessToken,
			});
			youtubeVideoUrlExtractor = new YoutubeVideoUrlExtractor();
		}

		public CourseExporter(string accessToken)
			:this(
				ConfigurationManager.AppSettings["stepik.clientId"],
				ConfigurationManager.AppSettings["stepik.clientSecret"],
				ConfigurationManager.AppSettings["ulearn.baseUrl"],
				accessToken
			)
		{
		}

		public async Task<CourseExportResults> InitialExportCourse(Course course, CourseInitialExportOptions exportOptions)
		{
			var results = new CourseExportResults(course.Id, exportOptions.StepikCourseId);
			try
			{
				await TryInitialExportCourse(course, exportOptions, results);
			}
			catch (Exception e)
			{
				results.Error(e.Message);
				results.Exception = e;
			}
			return results;
		}

		private async Task TryInitialExportCourse(Course course, CourseInitialExportOptions exportOptions, CourseExportResults results)
		{
			var stepikCourse = await client.GetCourse(exportOptions.StepikCourseId).ConfigureAwait(false);
			results.StepikCourseTitle = stepikCourse.Title;
			await ClearCourse(stepikCourse).ConfigureAwait(false);

			var unitIndex = 0;
			foreach (var unit in course.Units)
			{
				results.Info($"Converting ulearn unit «{unit.Title}» into stepik section");
				var section = ConvertUlearnUnitIntoStepikSection(unit);
				section.CourseId = exportOptions.StepikCourseId;
				section.Position = ++unitIndex;
				section = await client.UploadSection(section).ConfigureAwait(false);
				if (!section.Id.HasValue)
					throw new StepikApiException($"Didn't receive `section`'s ID from stepik: {section.JsonSerialize()}");

				var currentStepikLessonId = -1;
				var blockIndex = 0;
				var lessonIndex = 0;
				var isFirstSlideInUnit = true;
				foreach (var slide in unit.Slides)
				{
					results.Info($"Converting ulearn slide «{slide.Title}» with id {slide.Id} into stepik steps");
					var needToStartNewLesson = isFirstSlideInUnit || exportOptions.SlideIdsWhereNewLessonsStart.Contains(slide.Id);
					isFirstSlideInUnit = false;
					if (needToStartNewLesson)
					{
						results.Info("Starting new stepik lesson for this slide");
						var lesson = await client.UploadLesson(new StepikApiLesson
						{
							Title = slide.Title
						}).ConfigureAwait(false);
						if (!lesson.Id.HasValue)
							throw new StepikApiException($"Didn't receive `lesson`'s ID from stepik: {lesson.JsonSerialize()}");

						currentStepikLessonId = lesson.Id.Value;

						await client.UploadUnit(new StepikApiUnit
						{
							Position = ++lessonIndex,
							SectionId = section.Id.Value,
							LessonId = currentStepikLessonId,
						}).ConfigureAwait(false);

						blockIndex = 0;
					}

					blockIndex += await InsertSlideAsStepsInLesson(course, slide, currentStepikLessonId, blockIndex, exportOptions, results).ConfigureAwait(false);
				}
			}
		}

		private async Task<int> InsertSlideAsStepsInLesson(Course course, Slide slide, int stepikLessonId, int position, CourseExportOptions options, CourseExportResults results)
		{
			var stepikBlocks = (await ConvertUlearnBlocksIntoStepikBlocks(course.Id, slide, slide.Blocks, options, stepikLessonId, results).ConfigureAwait(false)).ToList();
			var blocksCountToInsert = stepikBlocks.Count;

			await MoveStepsForSpaceEmpting(stepikLessonId, position, blocksCountToInsert).ConfigureAwait(false);

			foreach (var stepikBlock in stepikBlocks)
			{
				/* Stepik has no step's titles so insert slide title in each step as H1 */
				stepikBlock.Text = $"<h1>{slide.Title.EscapeHtml()}</h1>" + (stepikBlock.Text ?? "");
				var uploadedStep = await client.UploadStep(new StepikApiStepSource
				{
					Block = stepikBlock,
					Position = ++position,
					LessonId = stepikLessonId,
					Cost = stepikBlock.Cost,
				}).ConfigureAwait(false);

				if (!uploadedStep.Id.HasValue)
					throw new StepikApiException("Didn't receive `step`'s ID from stepik");
				results.SlideIdToStepsIdsMap[slide.Id].Add(uploadedStep.Id.Value);
			}

			return blocksCountToInsert;
		}

		private async Task MoveStepsForSpaceEmpting(int stepikLessonId, int position, int blocksCountToInsert)
		{
			var lesson = await client.GetLesson(stepikLessonId).ConfigureAwait(false);
			var stepsIds = lesson.StepsIds;
			if (stepsIds.Count <= position)
				return;

			for (var idx = position; idx < stepsIds.Count; idx++)
				await client.MoveStep(stepsIds[idx], idx + 1 + blocksCountToInsert).ConfigureAwait(false);
		}

		public async Task<CourseExportResults> UpdateCourse(Course course, CourseUpdateOptions updateOptions)
		{
			var results = new CourseExportResults(course.Id, updateOptions.StepikCourseId);
			try
			{
				await TryUpdateCourse(course, updateOptions, results).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				results.Exception = e;
			}
			return results;
		}

		private async Task TryUpdateCourse(Course course, CourseUpdateOptions updateOptions, CourseExportResults results)
		{
			results.Info($"Downloading stepik course #{updateOptions.StepikCourseId}");
			var stepikCourse = await client.GetCourse(updateOptions.StepikCourseId).ConfigureAwait(false);
			results.StepikCourseTitle = stepikCourse.Title;

			results.Info($"Downloading stepik sections for it ({string.Join(", ", stepikCourse.SectionsIds)})");
			var stepikSections = new Dictionary<int, StepikApiSection>();
			// TODO (andgein): download multiple sections in one request
			foreach (var sectionId in stepikCourse.SectionsIds)
				stepikSections[sectionId] = await client.GetSection(sectionId).ConfigureAwait(false);
				
			var stepikUnitsIds = stepikSections.SelectMany(kvp => kvp.Value.UnitsIds).ToList();
			results.Info($"Downloading stepik units ({string.Join(", ", stepikUnitsIds)}) and lessons for them");
			var stepikUnits = new Dictionary<int, StepikApiUnit>();
			foreach (var unitId in stepikUnitsIds)
				stepikUnits[unitId] = await client.GetUnit(unitId).ConfigureAwait(false);

			var stepikLessonsIds = stepikUnits.Select(kvp => kvp.Value.LessonId);
			var stepikLessons = new Dictionary<int, StepikApiLesson>();
			foreach (var lessonId in stepikLessonsIds)
				stepikLessons[lessonId] = await client.GetLesson(lessonId).ConfigureAwait(false);

			var sectionIndex = stepikCourse.SectionsIds.Count;
			foreach (var slideUpdateOptions in updateOptions.SlidesUpdateOptions)
			{ 
				var slideId = slideUpdateOptions.SlideId;
				var slide = course.FindSlideById(slideId);
				if (slide == null)
				{
					results.Error($"Unable to find slide {slideId}, continue without it");
					continue;
				}
				results.Info($"Updating slide «{slide.Title}» with id {slide.Id}");
				var stepId = slideUpdateOptions.InsertAfterStep;
				StepikApiLesson lesson;
				int position;

				if (stepId != -1 && stepikLessons.Values.Any(l => l.StepsIds.Contains(stepId)))
				{
					var lessonId = stepikLessons.FirstOrDefault(kvp => kvp.Value.StepsIds.Contains(stepId)).Key;
					// Re-download lesson because it can be changed for a while 
					lesson = await client.GetLesson(lessonId).ConfigureAwait(false);
					position = lesson.StepsIds.FindIndex(stepId);

					results.Info($"Removing old steps created for this slide: {string.Join(", ", slideUpdateOptions.RemoveStepsIds)}");
					await RemoveStepsFromLesson(lesson, slideUpdateOptions.RemoveStepsIds, results).ConfigureAwait(false);
				}
				else
				{
					results.Info("Creating new stepik lesson for this slide");
					lesson = await CreateLessonAndSectionForSlide(slide, stepikCourse, ++sectionIndex).ConfigureAwait(false);
					position = 0;
				}

				results.Info($"Inserting steps for slide «{slide.Title}» into lesson {lesson.Id} on position {position}");
				await InsertSlideAsStepsInLesson(course, slide, lesson.Id.Value, position + 1, updateOptions, results);
			}
		}

		private async Task RemoveStepsFromLesson(StepikApiLesson lesson, List<int> removeStepsIds, CourseExportResults results)
		{
			var realStepsIds = lesson.StepsIds;

			var newPosition = 0;
			foreach (var stepId in realStepsIds)
				if (removeStepsIds.Contains(stepId))
				{
					try
					{
						await client.DeleteStep(stepId).ConfigureAwait(false);
					}
					catch (StepikApiException e)
					{
						results.Error("Can't delete step: " + e.Message);
					}
				}
				else
				{
					try
					{
						await client.MoveStep(stepId, newPosition + 1).ConfigureAwait(false);
						// Increment newPosition only if success
						newPosition++;
					}
					catch (StepikApiException e)
					{
						results.Error("Can't move step: " + e.Message);
					}
				}
		}

		private async Task<StepikApiLesson> CreateLessonAndSectionForSlide(Slide slide, StepikApiCourse stepikCourse, int sectionIndex)
		{
			var lesson = await client.UploadLesson(new StepikApiLesson
			{
				Title = slide.Title,
			}).ConfigureAwait(false);
			var section = await client.UploadSection(new StepikApiSection
			{
				CourseId = stepikCourse.Id.Value,
				Position = sectionIndex,
				Title = slide.Title,
			});
			await client.UploadUnit(new StepikApiUnit
			{
				SectionId = section.Id.Value,
				LessonId = lesson.Id.Value,
				Position = 1,
			});
			return lesson;
		}

		/* Convert ulearn's blocks and group some of them into on stepik's text block*/
		private async Task<IEnumerable<StepikApiBlock>> ConvertUlearnBlocksIntoStepikBlocks(string courseId, Slide slide, IEnumerable<SlideBlock> blocks, CourseExportOptions options, int stepikLessonId, CourseExportResults results)
		{
			var stepikBlocks = new List<StepikApiBlock>();

			var previousTextBlock = new StepikApiBlock
			{
				Name = "text",
				Text = "",
			};
			var attachTextBlocksToInteractiveContent = slide is QuizSlide || slide is ExerciseSlide;
			foreach (var block in blocks)
			{
				if (block.Hide)
					continue;

				var isCurrentBlockText = IsCurrentBlockText(block, options);
				if (isCurrentBlockText)
				{
					previousTextBlock.Text += GetTextForStepikBlockFromUlearnBlock(courseId, slide, block);
				}
				else
				{
					if (!string.IsNullOrEmpty(previousTextBlock.Text) && !attachTextBlocksToInteractiveContent)
					{
						stepikBlocks.Add(previousTextBlock);
						previousTextBlock = new StepikApiBlock
						{
							Name = "text",
							Text = "",
						};
					}

					var stepikBlock = await ConvertUlearnNonTextBlockIntoStepikStepBlock(
						block, previousTextBlock, courseId, slide, stepikLessonId, options, results
					).ConfigureAwait(false);
					if (stepikBlock != null)
						stepikBlocks.Add(stepikBlock);
				}
			}
			if (!string.IsNullOrEmpty(previousTextBlock.Text) && !attachTextBlocksToInteractiveContent)
				stepikBlocks.Add(previousTextBlock);

			return stepikBlocks;
		}

		private static bool IsCurrentBlockText(SlideBlock block, CourseExportOptions options)
		{
			return block is MarkdownBlock || block is CodeBlock || (options.VideoUploadOptions == UploadVideoToStepikOption.Iframe && block is YoutubeBlock);
		}

		private string GetTextForStepikBlockFromUlearnBlock(string courseId, Slide slide, SlideBlock block)
		{
			switch (block)
			{
				case MarkdownBlock markdownBlock:
					return markdownBlock.RenderMarkdown(courseId, slide.Id, slide.Info.SlideFile, ulearnBaseUrl);
				case CodeBlock codeBlock:
				{
					const string codeTemplate = "<pre><code class=\"%LANG%\">%CODE%</code></pre>";
					return codeTemplate.Replace("%LANG%", codeBlock.Language.GetName()).Replace("%CODE%", codeBlock.Code.EscapeHtml());
				}
				case YoutubeBlock videoBlock:
				{
					const string videoTemplate = "<iframe width='864' height='480' src='//www.youtube.com/embed/%VIDEO_ID%' frameborder='0' allowfullscreen></iframe>";
					return videoTemplate.Replace("%VIDEO_ID%", videoBlock.VideoId);
				}
				default:
					throw new StepikApiException($"Unknown block type ({block.GetType().Name}) for text extracting. Can't get text from {block}");
			}
		}

		private async Task<StepikApiBlock> ConvertUlearnNonTextBlockIntoStepikStepBlock(SlideBlock block, StepikApiBlock lastTextBlock, string courseId, Slide slide, int lessonId, CourseExportOptions options, CourseExportResults results)
		{
			switch (block)
			{
				case AbstractExerciseBlock exerciseBlock:
					return new StepikApiBlock
					{
						Name = "external-grader",
						Text = lastTextBlock.Text,
						Cost = ((ExerciseSlide) slide).Scoring.PassedTestsScore,
						Source = new StepikApiExternalGraderBlockSource(courseId, slide.Id, options.XQueueName, exerciseBlock.ExerciseInitialCode, stepikCSharpLanguageName)
					};
				case ChoiceBlock choiceBlock:
					return new StepikApiBlock
					{
						Name = "choice",
						Text = lastTextBlock.Text + $"<p><br/><b>{choiceBlock.Text.EscapeHtml()}</b></p>",
						Cost = choiceBlock.MaxScore,
						Source = new StepikApiChoiceBlockSource(choiceBlock.Items.Select(ConvertUlearnChoiceItemIntoStepikChoiceOption))
						{
							IsMultipleChoice = choiceBlock.Multiple,
							PreserveOrder = !choiceBlock.Shuffle
						}
					};
				case YoutubeBlock videoBlock:
				{
					var rawVideoUrl = await youtubeVideoUrlExtractor.GetVideoUrl(videoBlock.GetYoutubeUrl()).ConfigureAwait(false);
					var video = await client.UploadVideo(rawVideoUrl, lessonId).ConfigureAwait(false);
					await Task.Delay(options.PauseAfterVideoUploaded).ConfigureAwait(false);
					return new StepikApiBlock
					{
						Name = "video",
						Video = video,
					};
				}
				default:
					results.Error($"Unknown block type for converting into stepik step: {block.GetType().Name}, ignoring it");
					return null;
			}
		}

		private static StepikApiChoiceOption ConvertUlearnChoiceItemIntoStepikChoiceOption(ChoiceItem item)
		{
			return new StepikApiChoiceOption(item.Description, item.IsCorrect.IsTrueOrMaybe(), item.Explanation);
		}

		private static StepikApiSection ConvertUlearnUnitIntoStepikSection(Unit unit)
		{
			return new StepikApiSection
			{
				Title = unit.Title,
			};
		}

		private async Task ClearCourse(StepikApiCourse course)
		{
			foreach (var sectionId in course.SectionsIds)
				await client.DeleteSection(sectionId).ConfigureAwait(false);
		}
	}
}
