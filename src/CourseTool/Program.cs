using System;
using CommandLine;
using uLearn.CourseTool.CmdLineOptions;

namespace uLearn.CourseTool
{
	public class Program
	{
		public static int Main(string[] args)
		{
			return Parser.Default
				.ParseArguments<
					OlxConvertFromUlearnOptions, 
					OlxPatchFromUlearnOptions, 
					OlxSquashChaptersOptions, OlxDesquashChaptersOptions, 
					OlxGcOptions, 
					OlxUnpackTarGzOptions, OlxUnpackTarOptions, 
					OlxPackTarGzOptions, OlxPackTarOptions,
					OlxSetChapterStartDatesOptions, 
					OlxPatchVideoOptions, 
					MonitorOptions, 
					ULearnOptions, 
					TestCourseOptions,
					GenerateEmptyVideoAnnotations
				>(args)
				.MapResult(
					(AbstractOptions options) => ExecuteOption(options),
					_ => -1
				);
		}

		private static int ExecuteOption(AbstractOptions options)
		{
			try
			{
				options.Execute();
				return 0;
			}
			catch (OperationFailedGracefully)
			{
				Console.WriteLine("Operation failed.");
				//nothing to do — failed gracefully already.
				return -1;
			}
		}
	}
}