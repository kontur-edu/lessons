using System;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace uLearn.Helpers
{
	public static class SubmissionPacker
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(SubmissionPacker));

		public static byte[] Pack(SubmissionLanguage submissionLanguage,
			DirectoryInfo exerciseFolder, string userCodeFilePath, string code)
		{
			switch (submissionLanguage)
			{
				case SubmissionLanguage.JavaScript:
					return PackForJs(exerciseFolder, userCodeFilePath, code);
				default:
					throw new ArgumentOutOfRangeException(nameof(submissionLanguage), submissionLanguage, null);
			}
		}

		private static byte[] PackForJs(DirectoryInfo exerciseFolder, string userCodeFilePath, string code)
		{
			var excluded = new[] { "name != src/* AND name != tests/*", "*.blank.*" };

			log.Info("Собираю zip-архив для проверки: получаю список дополнительных файлов");
			var toUpdate = new[] { new FileContent { Path = userCodeFilePath, Data = Encoding.UTF8.GetBytes(code) } };
			log.Info($"Собираю zip-архив для проверки: дополнительные файлы [{string.Join(", ", toUpdate.Select(c => c.Path))}]");

			var zipBytes = exerciseFolder.ToZip(excluded, toUpdate);
			log.Info($"Собираю zip-архив для проверки: zip-архив собран, {zipBytes.Length} байтов");
			return zipBytes;
		}
	}
}