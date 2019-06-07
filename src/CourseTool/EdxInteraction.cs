using System;
using System.IO;
using System.Linq;
using Ulearn.Common.Extensions;
using Ulearn.Core;

namespace uLearn.CourseTool
{
	public static class EdxInteraction
	{
		public static string GetTarGzFile(string dir)
		{
			var archives = dir.GetFiles("*.tar.gz");
			if (archives.Length == 1)
				return archives[0];
			throw new Exception($"Can't decide which archive to extract! Remove all other tar.gz files. Found: {string.Join(", ", archives)}");
		}

		public static void ExtractEdxCourseArchive(string baseDir, string tarGzFilepath, bool gzipped = false)
		{
			var filename = tarGzFilepath;
			Console.WriteLine($"Extracting {filename}");

			var extension = gzipped ? ".tar.gz" : ".tar";
			var extractedTarDirectory = Path.Combine(baseDir, ".extracted-" + Path.GetFileName(filename).EnsureNotNull().Replace(extension, ""));
			if (gzipped)
				ArchiveManager.ExtractTarGz(filename, extractedTarDirectory);
			else
				ArchiveManager.ExtractTar(filename, extractedTarDirectory);
			var extractedOlxDirectory = Directory.GetDirectories(extractedTarDirectory).Single();
			Utils.DeleteDirectoryIfExists(baseDir + "/olx");
			Directory.Move(extractedOlxDirectory, baseDir + "/olx");
			Directory.Delete(extractedTarDirectory);
		}

		public static void CreateEdxCourseArchive(string baseDir, string courseName, bool gzipped = false)
		{
			var extension = gzipped ? ".tar.gz" : ".tar";
			Environment.CurrentDirectory = baseDir;
			var outputTarFilename = $"{courseName}{extension}";
			Console.WriteLine($"Creating archive {outputTarFilename}");
			Utils.DeleteFileIfExists(outputTarFilename);
			if (gzipped)
				ArchiveManager.CreateTarGz(outputTarFilename, "olx");
			else
				ArchiveManager.CreateTar(outputTarFilename, "olx");
		}
	}
}