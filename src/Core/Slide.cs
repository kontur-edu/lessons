using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using uLearn.Model;
using uLearn.Model.Blocks;
using uLearn.Model.Edx;
using uLearn.Model.Edx.EdxComponents;
using uLearn.Quizes;
using Ulearn.Common.Extensions;

namespace uLearn
{
	public class Slide
	{
		public readonly string Title;
		public SlideBlock[] Blocks { get; private set; }
		public readonly SlideInfo Info;
		public int Index => Info.Index;
		public readonly Guid Id;
		public string NormalizedGuid => Id.GetNormalizedGuid();
		public virtual bool ShouldBeSolved => false;
		public int MaxScore { get; protected set; }
		public SlideMetaDescription Meta { get; protected set; }

		[NotNull]
		public string ScoringGroup { get; protected set; }

		public IEnumerable<SlideBlock[]> GetBlocksRangesWithSameVisibility()
		{
			if (Blocks.Length == 0)
				yield break;
			var range = new List<SlideBlock> { Blocks[0] };
			foreach (var block in Blocks.Skip(1))
			{
				if (block.Hide != range.Last().Hide)
				{
					yield return range.ToArray();
					range.Clear();
				}
				range.Add(block);
			}
			yield return range.ToArray();
		}

		public Slide(IEnumerable<SlideBlock> blocks, SlideInfo info, string title, Guid id, SlideMetaDescription meta)
		{
			try
			{
				Info = info;
				Title = title;
				Id = id;
				MaxScore = 0;
				Blocks = blocks.ToArray();
				ScoringGroup = "";
				Meta = meta;
				foreach (var block in Blocks)
					block.Validate();
			}
			catch (Exception e)
			{
				throw new FormatException($"Error in slide {title} (id: {id}). {e.Message}", e);
			}
		}

		public override string ToString()
		{
			return $"Title: {Title}, Id: {NormalizedGuid}, MaxScore: {MaxScore}";
		}

		public string LatinTitle => Title.ToLatin();

		public string Url => LatinTitle + "_" + NormalizedGuid;

		private static IEnumerable<Vertical> OrdinarySlideToVerticals(string courseId, Slide slide, string slideUrl, string solutionsUrl, Dictionary<string, string> videoGuids, string ltiId)
		{
			var componentIndex = 0;
			var components = new List<Component>();
			while (componentIndex < slide.Blocks.Length)
			{
				var blocks = slide.Blocks.Skip(componentIndex).TakeWhile(x => !(x is YoutubeBlock) && !(x is ExerciseBlock)).ToList();
				if (blocks.Count != 0)
				{
					var innerComponents = new List<Component>();
					foreach (var block in blocks)
					{
						if (!block.Hide)
						{
							var component = block.ToEdxComponent("", slide, componentIndex);
							innerComponents.Add(component);
						}
						componentIndex++;
					}

					if (innerComponents.Any())
					{
						var displayName = componentIndex == blocks.Count ? slide.Title : "";
						var header = displayName == "" ? "" : "<h2>" + displayName + "</h2>";
						var slideComponent = new HtmlComponent
						{
							DisplayName = displayName,
							UrlName = slide.NormalizedGuid + componentIndex,
							Filename = slide.NormalizedGuid + componentIndex,
							Source = header + string.Join("", innerComponents.Select(x => x.AsHtmlString())),
							Subcomponents = innerComponents.ToArray()
						};
						components.Add(slideComponent);
					}
				}
				if (componentIndex >= slide.Blocks.Length)
					break;

				var exerciseBlock = slide.Blocks[componentIndex] as ExerciseBlock;
				var otherComponent = exerciseBlock != null
					? exerciseBlock.GetExerciseComponent(componentIndex == 0 ? slide.Title : "Упражнение", slide, componentIndex, string.Format(slideUrl, courseId, slide.Id), ltiId)
					: ((YoutubeBlock)slide.Blocks[componentIndex]).GetVideoComponent(componentIndex == 0 ? slide.Title : "", slide, componentIndex, videoGuids);

				components.Add(otherComponent);
				componentIndex++;
			}

			var exerciseWithSolutionsToShow = slide.Blocks.OfType<ExerciseBlock>().FirstOrDefault(e => !e.HideShowSolutionsButton);
			if (exerciseWithSolutionsToShow != null)
			{
				var comp = exerciseWithSolutionsToShow.GetSolutionsComponent(
					"Решения",
					slide, componentIndex,
					string.Format(solutionsUrl, courseId, slide.Id), ltiId);
				components.Add(comp);
				//yield return new Vertical(slide.NormalizedGuid + "0", "Решения", new[] { comp });
			}
			var exBlock = slide.Blocks.OfType<ExerciseBlock>().FirstOrDefault();
			if (exBlock == null)
				yield return new Vertical(slide.NormalizedGuid, slide.Title, components.ToArray());
			else
				yield return new Vertical(slide.NormalizedGuid, slide.Title, components.ToArray(), EdxScoringGroupsHack.ToEdxName(exBlock.ScoringGroup), exBlock.MaxScore);
		}

		private static IEnumerable<Vertical> QuizToVerticals(string courseId, QuizSlide slide, string slideUrl, string ltiId)
		{
			var ltiComponent =
				new LtiComponent(slide.Title, slide.NormalizedGuid + "-quiz", string.Format(slideUrl, courseId, slide.Id), ltiId, true, slide.MaxScore, false);
			yield return new Vertical(slide.NormalizedGuid, slide.Title, new Component[] { ltiComponent }, EdxScoringGroupsHack.ToEdxName(slide.ScoringGroup), slide.MaxScore);
		}

		public IEnumerable<Vertical> ToVerticals(string courseId, string slideUrl, string solutionsUrl, Dictionary<string, string> videoGuids, string ltiId)
		{
			try
			{
				var quizSlide = this as QuizSlide;
				if (quizSlide != null)
					return QuizToVerticals(courseId, quizSlide, slideUrl, ltiId).ToList();
				return OrdinarySlideToVerticals(courseId, this, slideUrl, solutionsUrl, videoGuids, ltiId).ToList();
			}
			catch (Exception e)
			{
				throw new Exception($"Slide {this}. {e.Message}", e);
			}
		}
	}

	class EdxScoringGroupsHack
	{
		public static string ToEdxName(string scoringGroup)
		{
			return scoringGroup == "homework" ? "Практика" : "Упражнения";
		}
	}
}