using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Ionic.Zip;
using JetBrains.Annotations;
using Ulearn.Common;

namespace Ulearn.Core
{
	public class ZipHelper
	{
		public static void UnpackZip(byte[] data, string pathToExtractDir)
		{
			using (var ms = new MemoryStream(data))
			{
				using (var zip = Ionic.Zip.ZipFile.Read(ms))
				{
					foreach (var file in zip)
						try
						{
							file.Extract(pathToExtractDir, ExtractExistingFileAction.OverwriteSilently);
						}
						catch (Exception e)
						{
							throw new IOException("File " + file.FileName, e);
						}
				}
			}
		}

		public static MemoryStream CreateFromDirectory(
			string sourceDirectoryName,
			CompressionLevel compressionLevel,
			bool includeBaseDirectory,
			Encoding entryNameEncoding,
			[CanBeNull] Predicate<string> filter
		)
		{
			var filesToAdd = Directory.GetFiles(sourceDirectoryName, "*", SearchOption.AllDirectories);
			var entryNames = GetEntryNames(filesToAdd, sourceDirectoryName, includeBaseDirectory);
			var zipFileStream = StaticRecyclableMemoryStreamManager.Manager.GetStream();
			using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create, leaveOpen: true))
			{
				for (var i = 0; i < filesToAdd.Length; i++)
				{
					if (filter != null && !filter(filesToAdd[i]))
					{
						continue;
					}
					archive.CreateEntryFromFile(filesToAdd[i], entryNames[i], compressionLevel);
				}
			}
			zipFileStream.Position = 0;
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