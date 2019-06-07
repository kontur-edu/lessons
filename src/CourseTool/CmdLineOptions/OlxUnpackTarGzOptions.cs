using CommandLine;
using Ulearn.Common.Extensions;

namespace uLearn.CourseTool.CmdLineOptions
{
	[Verb("olx-unpack-targz", HelpText = "Unpack course.tar.gz to olx directory. Replaces all the old content of the olx directory. No patching!")]
	public class OlxUnpackTarGzOptions : AbstractOptions
	{
		[Option('f', "file", HelpText = "Filepath of course tar.gz file")]
		public string CourseArchiveFile { get; set; }

		public override void DoExecute()
		{
			EdxInteraction.ExtractEdxCourseArchive(Dir, Dir.GetSingleFile(CourseArchiveFile ?? "*.tar.gz"), gzipped:true);
		}
	}

	[Verb("olx-unpack-tar", HelpText = "Unpack course.tar to olx directory. Replaces all the old content of the olx directory. No patching!")]
	public class OlxUnpackTarOptions : AbstractOptions
	{
		[Option('f', "file", HelpText = "Filepath of course tar file")]
		public string CourseArchiveFile { get; set; }

		public override void DoExecute()
		{
			EdxInteraction.ExtractEdxCourseArchive(Dir, Dir.GetSingleFile(CourseArchiveFile ?? "*.tar"));
		}
	}
}