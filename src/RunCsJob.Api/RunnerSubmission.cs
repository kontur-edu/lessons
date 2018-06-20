using System.ComponentModel;

namespace RunCsJob.Api
{
	public abstract class RunnerSubmission
	{
		public string Id;
		public string Input;
		public bool NeedRun;

		public override string ToString()
		{
			return $"Id: {Id}, NeedRun: {NeedRun}";
		}
	}

	[DisplayName("file")]
	public class FileRunnerSubmission : RunnerSubmission
	{
		public string Code;
	}

	[DisplayName("proj")]
	public class ProjRunnerSubmission : RunnerSubmission
	{
		public byte[] ZipFileData;
		public string ProjectFileName;
	}

	[DisplayName("zip")]
	public class ZipRunnerSubmission : RunnerSubmission
	{
		public byte[] ZipFileData;
		public string LangId;
	}
}