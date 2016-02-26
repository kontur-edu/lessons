using System;
using System.Collections.Generic;
using System.Linq;
using uLearn.Model.Blocks;
using uLearn.Model.Edx;
using uLearn.Model.Edx.EdxComponents;
using uLearn.Quizes;

namespace uLearn
{
	public class Slide
	{
		public readonly string Title;
		public readonly SlideBlock[] Blocks;
		public readonly SlideInfo Info;
		public int Index { get { return Info.Index; }}
		public readonly string Id;
		public string Guid { get { return Utils.GetNormalizedGuid(Id); } }
		public virtual bool ShouldBeSolved { get { return false; } }
		public int MaxScore { get; protected set; }

		public IEnumerable<SlideBlock[]> GetBlocksRangesWithSameVisibility()
		{
			if (Blocks.Length == 0)
				yield break;
			var range = new List<SlideBlock> {Blocks[0]};
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


		public Slide(IEnumerable<SlideBlock> blocks, SlideInfo info, string title, string id)
		{
			try
			{
				Info = info;
				Title = title;
				Id = id;
				MaxScore = 0;
				Blocks = blocks.ToArray();
				foreach (var block in Blocks)
					block.Validate();
			}
			catch (Exception e)
			{
				throw new FormatException(string.Format("Error in slide {0} (id: {1}). {2}", title, id, e.Message), e);
			}
		}

		public override string ToString()
		{
			return string.Format("Title: {0}, Id: {1}, MaxScore: {2}", Title, Id, MaxScore);
		}

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
						var component = block.ToEdxComponent("", slide, componentIndex);
						innerComponents.Add(component);
						componentIndex++;
					}

					var displayName = componentIndex == blocks.Count ? slide.Title : "";
					var header = displayName == "" ? "" : "<h2>" + displayName + "</h2>";
					var slideComponent = new HtmlComponent
					{
						DisplayName = displayName,
						UrlName = slide.Guid + componentIndex,
						Filename = slide.Guid + componentIndex,
						Source = header + string.Join("", innerComponents.Select(x => x.AsHtmlString())),
						Subcomponents = innerComponents.ToArray()
					};
					components.Add(slideComponent);
				}
				if (componentIndex >= slide.Blocks.Length)
					break;

				var exerciseBlock = slide.Blocks[componentIndex] as ExerciseBlock;
				var otherComponent = exerciseBlock != null
					? exerciseBlock.GetExerciseComponent(componentIndex == 0 ? slide.Title : "Упражнение", slide, componentIndex, string.Format(slideUrl, courseId, slide.Index), ltiId)
					: ((YoutubeBlock)slide.Blocks[componentIndex]).GetVideoComponent(componentIndex == 0 ? slide.Title : "", slide, componentIndex, videoGuids);

				components.Add(otherComponent);
				componentIndex++;
			}

			var solutionComponents = new List<Component>();
			foreach (var result in slide.Blocks.OfType<ExerciseBlock>())
			{
				var comp = result.GetSolutionsComponent("Решения", slide, componentIndex, string.Format(solutionsUrl, courseId, slide.Index), ltiId);
				solutionComponents.Add(comp);
				componentIndex++;
			}

			yield return new Vertical(slide.Guid, slide.Title, components.ToArray());
			if (solutionComponents.Count != 0)
				yield return new Vertical(slide.Guid + "0", "Решения", solutionComponents.ToArray());
		}

		private static IEnumerable<Vertical> QuizToVerticals(string courseId, QuizSlide slide, string slideUrl, string ltiId)
		{
			var ltiComponent = 
				new LtiComponent(slide.Title, slide.Guid + "-quiz", string.Format(slideUrl, courseId, slide.Index), ltiId, true, slide.MaxScore, false);
			yield return new Vertical(slide.Guid, slide.Title, new Component[] { ltiComponent });
		}

		public IEnumerable<Vertical> ToVerticals(string courseId, string slideUrl, string solutionsUrl, Dictionary<string, string> videoGuids, string ltiId)
		{
			var quizSlide = this as QuizSlide;
			if (quizSlide != null)
				return QuizToVerticals(courseId, quizSlide, slideUrl, ltiId);
			return OrdinarySlideToVerticals(courseId, this, slideUrl, solutionsUrl, videoGuids, ltiId);
		}
	}
}