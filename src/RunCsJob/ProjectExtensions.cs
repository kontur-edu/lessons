using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Evaluation;

namespace RunCsJob
{
	public static class ProjectExtensions
	{
		public static bool HasReference(this Project project, string libraryName)
		{
			return GetReferences(project).Any(r => IsLibrary(r.EvaluatedInclude, libraryName));
		}

		public static void AddReference(this Project project, string libraryName, string hintPath = null)
		{
			var metadata = hintPath == null ? null : new[] { new KeyValuePair<string, string>("HintPath", hintPath) };
			project.AddItem("Reference", libraryName, metadata);
		}

		private static IEnumerable<ProjectItem> GetReferences(Project project)
		{
			return project.AllEvaluatedItems.Where(i => i.ItemType == "Reference");
		}

		private static bool IsLibrary(string include, string libraryName)
		{
			return include.Equals(libraryName, StringComparison.OrdinalIgnoreCase) ||
					include.StartsWith($"{libraryName},", StringComparison.OrdinalIgnoreCase);
		}
	}
}