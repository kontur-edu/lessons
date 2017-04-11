using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace RunCsJob
{
	public class MsBuildSettings
	{
		private const string CompilersFolderName = "Microsoft.Net.Compilers.2.0.1";
		private const string WellKnownLibsFolderName = "WellKnownLibs";
		public MsBuildSettings()
		{
			var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			CompilerDirectory = new DirectoryInfo(Path.Combine(baseDirectory, CompilersFolderName));
			WellKnownLibsDirectory = new DirectoryInfo(Path.Combine(baseDirectory, WellKnownLibsFolderName));
		}
		public DirectoryInfo CompilerDirectory;
		public DirectoryInfo WellKnownLibsDirectory;
	}
	public static class MsBuildRunner
	{
		private const string ValueTupleLibName = "System.ValueTuple";

		public static MSbuildResult BuildProject(MsBuildSettings settings, string projectFileName, DirectoryInfo dir)
		{
			var result = new MSbuildResult();
			var path = Path.Combine(dir.FullName, projectFileName);
			var project = new Project(path, null, null, new ProjectCollection());
			project.SetProperty("CscToolPath", settings.CompilerDirectory.FullName);

			var references = project.AllEvaluatedItems.Where(i => i.ItemType == "Reference");
			if (references.All(r => !string.Equals(r.EvaluatedInclude, ValueTupleLibName, StringComparison.OrdinalIgnoreCase)))
			{
				project.AddItem(
					"Reference", ValueTupleLibName,
					new[] { new KeyValuePair<string, string>("HintPath", typeof(ValueTuple).Assembly.Location) });
				project.ReevaluateIfNecessary();
			}

			var includes = new HashSet<string>(
				project.AllEvaluatedItems
				.Where(i => i.ItemType == "None" || i.ItemType == "Content")
				.Select(i => Path.GetFileName(i.EvaluatedInclude.ToLowerInvariant())));

			foreach (var dll in settings.WellKnownLibsDirectory.GetFiles("*.dll"))
				if (!includes.Contains(dll.Name.ToLowerInvariant()))
					project.AddItem("None", dll.FullName);
			project.Save();
			using (var stringWriter = new StringWriter())
			{
				var logger = new ConsoleLogger(LoggerVerbosity.Minimal, stringWriter.Write, color => { }, () => { });
				result.Success = SyncBuild(project, logger);
				if (result.Success)
					result.PathToExe = Path.Combine(project.DirectoryPath,
													project.GetPropertyValue("OutputPath"),
													project.GetPropertyValue("AssemblyName") + ".exe");
				else
					result.ErrorMessage = stringWriter.ToString();
				return result;
			}
		}

		private static readonly object buildLock = new object();

		private static bool SyncBuild(Project project, ILogger logger)
		{
			lock (buildLock)
				return project.Build(logger);
		}
	}
}