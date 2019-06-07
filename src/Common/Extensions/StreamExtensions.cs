using System.IO;
using System.Text;

namespace Ulearn.Common.Extensions
{
	public static class StreamExtensions
	{
		public static string GetString(this Stream inputStream)
		{
			return Encoding.UTF8.GetString(inputStream.ToArray());
		}
		
		public static byte[] ToArray(this Stream inputStream)
		{
			var codeBytes = new MemoryStream();
			inputStream.CopyTo(codeBytes);
			return codeBytes.ToArray();
		}
	}
}