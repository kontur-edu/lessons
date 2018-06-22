using System.IO;
using System.Threading.Tasks;

namespace RunJsJob
{
	public class AsyncReader
	{
		private readonly char[] buffer;
		private readonly Task<int> readerTask;

		public AsyncReader(StreamReader reader, int length)
		{
			buffer = new char[length];
			readerTask = reader.ReadBlockAsync(buffer, 0, length);
		}

		public string GetData()
		{
			readerTask.Wait();
			return new string(buffer, 0, readerTask.Result);
		}

		private bool IsCompleted => readerTask.IsCompleted;

		public int ReadedLength => IsCompleted ? readerTask.Result : -1;
	}
}