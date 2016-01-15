using System;
using System.IO;
using CommandLine;

namespace uLearn.CourseTool
{
	public class Program
	{
		public static int Main(string[] args)
		{
			return Parser.Default
				.ParseArguments<ConvertOptions, DownloadOlxOption, UploadOlxOption, ULearnPatchOptions, VideoPatchOptions, SampleCustomPatchOptions, MonitorOptions, ULearnOptions>(args)
				.Return(
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
