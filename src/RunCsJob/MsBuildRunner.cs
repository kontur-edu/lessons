using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Ulearn.Common;

namespace RunCsJob
{
	public class MsBuildSettings
	{
		private const string compilersFolderName = "Microsoft.Net.Compilers.2.4.0";
		private const string wellKnownLibsFolderName = "WellKnownLibs";

		public MsBuildSettings()
		{
			BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			CompilerDirectory = new DirectoryInfo(Path.Combine(BaseDirectory, compilersFolderName));
			WellKnownLibsDirectory = new DirectoryInfo(Path.Combine(BaseDirectory, wellKnownLibsFolderName));
		}

		public readonly string BaseDirectory;
		public DirectoryInfo CompilerDirectory;
		public readonly DirectoryInfo WellKnownLibsDirectory;
		public readonly string MsBuildToolsVersion = "15.0";
	}

	public static class MsBuildRunner
	{
		private static readonly string[] obligatoryLibs = {"System.Runtime", "System.Reflection"};

		public static MSbuildResult BuildProject(MsBuildSettings settings, string projectFileName, DirectoryInfo dir)
		{
			var result = new MSbuildResult();
			var path = Path.Combine(dir.FullName, projectFileName);
			return FuncUtils.Using(
				new ProjectCollection(),
				projectCollection =>
				{
					var project = new Project(path, null, settings.MsBuildToolsVersion, projectCollection);
					project.SetProperty("CscToolPath", settings.CompilerDirectory.FullName);

					/* Workaround for MSB4216 (we don't know why it appears at some moment)
					* https://medium.com/@kviat/msb4216-fix-83d9e891a47b
					*/
					project.SetProperty("DisableOutOfProcTaskHost", "true");

					/* WPF markups should be compiled in separate AppDomain, otherwise MsBuild raises NRE while building:
					* https://stackoverflow.com/questions/1552092/microsoft-build-buildengine-engine-throws-error-when-building-wpf-application
					*/
					project.SetProperty("AlwaysCompileMarkupFilesInSeparateDomain", "True");

					/* We don't know why, but MSBuild on server set BaseIntermediateOutputPath to "\".
					* Here we return default value "obj\". 
					*/
					project.SetProperty("BaseIntermediateOutputPath", @"obj\");

					foreach (var libName in obligatoryLibs)
					{
						if (!project.HasReference(libName))
							project.AddReference(libName);
					}

					project.ReevaluateIfNecessary();

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
				},
				projectCollection =>
				{
					projectCollection.UnloadAllProjects(); // https://github.com/Microsoft/msbuild/pull/474
				}); 
		}

		private static volatile object buildLock = new object();

		private static bool SyncBuild(Project project, ILogger logger)
		{
			lock (buildLock)
				return project.Build("Rebuild", new List<ILogger> { logger });
		}
	}
}