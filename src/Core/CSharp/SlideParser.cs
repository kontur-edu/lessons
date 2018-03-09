using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Ulearn.Common.Extensions;

namespace uLearn.CSharp
{
	public static class SlideParser
	{
		public static Slide ParseSlide(FileInfo file, SlideInfo slideInfo, DirectoryInfo directoryForIncludes, CourseSettings settings)
		{
			return ParseSlide(file, slideInfo, "using System; using System.Linq;", directoryForIncludes, settings);
		}

		public static Slide ParseSlide(FileInfo file, SlideInfo slideInfo, string prelude, DirectoryInfo directoryForIncludes, CourseSettings settings)
		{
			var tree = CSharpSyntaxTree.ParseText(file.ContentAsUtf8());
			return ParseSyntaxTree(tree, slideInfo, prelude, directoryForIncludes, settings);
		}

		private static Slide ParseSyntaxTree(SyntaxTree tree, SlideInfo slideInfo, string prelude,
			DirectoryInfo getInclude, CourseSettings settings)
		{
			var blocksBuilder = new SlideBuilder(getInclude);
			blocksBuilder.Visit(tree.GetRoot());
			if (!ExerciseBuilder.IsExercise(tree))
				return new Slide(blocksBuilder.Blocks, slideInfo, blocksBuilder.Title, blocksBuilder.Id, meta: null);
			var exerciseBlock = new ExerciseBuilder(SlideBuilder.LangId, prelude).BuildBlockFrom(tree, slideInfo.SlideFile);
			exerciseBlock.CheckScoringGroup(slideInfo.SlideFile.FullName, settings.Scoring);
			blocksBuilder.Blocks.Add(exerciseBlock);
			return new ExerciseSlide(blocksBuilder.Blocks, slideInfo, blocksBuilder.Title, blocksBuilder.Id, meta: null);
		}
	}
}