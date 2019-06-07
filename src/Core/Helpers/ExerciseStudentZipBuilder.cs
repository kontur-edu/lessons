using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Evaluation;
using Ulearn.Common;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Courses.Slides.Exercises;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;

namespace Ulearn.Core.Helpers
{
	public class ExerciseStudentZipBuilder
	{
		private static readonly Regex anySolutionNameRegex = new Regex("(.+)\\.Solution\\.cs", RegexOptions.IgnoreCase);
		private static readonly Regex anyWrongAnswerNameRegex = new Regex("(.+)\\.WrongAnswer\\.(.+)\\.cs", RegexOptions.IgnoreCase);

		public static bool IsAnyWrongAnswerOrAnySolution(string name) => anyWrongAnswerNameRegex.IsMatch(name) || anySolutionNameRegex.IsMatch(name);
		public static bool IsAnySolution(string name) => anySolutionNameRegex.IsMatch(name);		
		
		public void BuildStudentZip(Slide slide, FileInfo zipFile)
		{
			var block = (slide as ExerciseSlide)?.Exercise as CsProjectExerciseBlock;
			if (block == null)
				throw new InvalidOperationException($"Can't generate student zip for non-project exercise block: slide \"{slide.Title}\" ({slide.Id})");
			
			var zip = new LazilyUpdatingZip(
				block.ExerciseFolder,
				new[] { "checking", "bin", "obj" },
				file => NeedExcludeFromStudentZip(block, file),
				file => GetFileContentInStudentZip(block, file),
				ResolveCsprojLinks(block),
				zipFile);
			zip.UpdateZip();
		}
		
		private static bool NeedExcludeFromStudentZip(CsProjectExerciseBlock block, FileInfo file)
		{
			var relativeFilePath = file.GetRelativePath(block.ExerciseFolder.FullName);
			return NeedExcludeFromStudentZip(block, relativeFilePath);
		}

		public static bool NeedExcludeFromStudentZip(CsProjectExerciseBlock block, string filepath)
		{
			return IsAnyWrongAnswerOrAnySolution(filepath) ||
					block.PathsToExcludeForStudent != null && block.PathsToExcludeForStudent.Any(p => p == filepath);
		}
		
		private static byte[] GetFileContentInStudentZip(CsProjectExerciseBlock block, FileInfo file)
		{
			if (!file.Name.Equals(block.CsprojFileName, StringComparison.InvariantCultureIgnoreCase))
				return null;
			return ProjModifier.ModifyCsproj(file, proj => ProjModifier.PrepareForStudentZip(proj, block));
		}
		
		public static IEnumerable<FileContent> ResolveCsprojLinks(CsProjectExerciseBlock block)
		{
			return ResolveCsprojLinks(block.CsprojFile, block.BuildEnvironmentOptions.ToolsVersion);
		}
		
		public static IEnumerable<FileContent> ResolveCsprojLinks(FileInfo csprojFile, string toolsVersion)
		{
			
			return FuncUtils.Using(
				new ProjectCollection(),
				projectCollection =>
				{
					return Body();
					IEnumerable<FileContent> Body()
					{
						var project = new Project(csprojFile.FullName, null, toolsVersion, projectCollection);
						var filesToCopy = ProjModifier.ReplaceLinksWithItemsAndReturnWhatToCopy(project);
						foreach (var fileToCopy in filesToCopy)
						{
							var fullSourcePath = Path.Combine(project.DirectoryPath, fileToCopy.SourceFile);
							yield return new FileContent { Path = fileToCopy.DestinationFile, Data = File.ReadAllBytes(fullSourcePath) };
						}
					}
				}, 
				projectCollection => projectCollection.UnloadAllProjects());
		}
	}
}