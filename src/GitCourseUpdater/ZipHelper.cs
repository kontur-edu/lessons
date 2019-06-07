using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace GitCourseUpdater
{
	public class ZipHelper
	{
		public static MemoryStream CreateFromDirectory(
			string sourceDirectoryName,
			CompressionLevel compressionLevel,
			bool includeBaseDirectory,
			Encoding entryNameEncoding,
			Predicate<string> filter
		) {
			var filesToAdd = Directory.GetFiles(sourceDirectoryName, "*", SearchOption.AllDirectories);
			var entryNames = GetEntryNames(filesToAdd, sourceDirectoryName, includeBaseDirectory);
			var zipFileStream = new MemoryStream();
			using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create)) {
				for (var i = 0; i < filesToAdd.Length; i++) {
					if (!filter(filesToAdd[i])) {
						continue;
					}
					archive.CreateEntryFromFile(filesToAdd[i], entryNames[i], compressionLevel);
				}
			}

			return zipFileStream;
		}
		
		private static string[] GetEntryNames(string[] names, string sourceFolder, bool includeBaseName)
		{
			if (names == null || names.Length == 0)
				return new string[0];

			if (includeBaseName)
				sourceFolder = Path.GetDirectoryName(sourceFolder);

			var length = string.IsNullOrEmpty(sourceFolder) ? 0 : sourceFolder.Length;
			if (length > 0 && sourceFolder != null && sourceFolder[length - 1] != Path.DirectorySeparatorChar && sourceFolder[length - 1] != Path.AltDirectorySeparatorChar)
				length++;

			var result = new string[names.Length];
			for (var i = 0; i < names.Length; i++)
			{
				result[i] = names[i].Substring(length);
			}

			return result;
		}
	}
}