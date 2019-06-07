using System;
using CommandLine;
using Ulearn.Common.Extensions;
using Ulearn.Core.Model.Edx;

namespace uLearn.CourseTool.CmdLineOptions
{
	abstract class OlxAbstractPatchOptions : AbstractOptions
	{
		[Option('s', "skip-existing", Default = false, HelpText = "If set, patch skips uLearn slides if edx slide with the same id exists already")]
		public bool SkipExistingGuids { get; set; }

		[Option("skip-targz", Default = false, HelpText = "Load olx from 'olx' directory, skip extracting tar.gz file. Usefull after manual modifications of olx directory")]
		public bool SkipExtractingTarGz { get; set; }

		[Option('g', "guid", HelpText = "Specific guids to be patched separated by comma")]
		public string Guids { get; set; }

		[Option("targz", HelpText = "Course tar.gz file from edx (will be guessed if not specified)")]
		public string CourseTarGz { get; set; }

		public override void DoExecute()
		{
			Console.WriteLine("Profile {0}", Profile);
			var profile = Config.GetProfile(Profile);

			if (!SkipExtractingTarGz)
			{
				var tarGzPath = Dir.GetSingleFile(CourseTarGz ?? "*.tar.gz");
				EdxInteraction.ExtractEdxCourseArchive(Dir, tarGzPath);
			}

			Console.WriteLine("Loading OLX");
			var edxCourse = EdxCourse.Load(Dir + "/olx");
			Console.WriteLine("Patching OLX...");
			Patch(new OlxPatcher(Dir + "/olx"), Config, profile, edxCourse);
			Console.WriteLine("Patched!");
			EdxInteraction.CreateEdxCourseArchive(Dir, Config.ULearnCourseId);
		}

		public abstract void Patch(OlxPatcher patcher, Config config, Profile profile, EdxCourse edxCourse);
	}
}