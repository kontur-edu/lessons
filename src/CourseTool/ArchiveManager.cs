using System.IO;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace uLearn.CourseTool
{
	class ArchiveManager
	{
		/// <summary>
		/// Extracts contents of Tar file to specified directory
		/// </summary>
		/// <param name="filename">Input .tar file</param>
		/// <param name="directory">Output directory</param>
		public static void ExtractTar(string filename, string directory)
		{
			using (Stream inStream = File.OpenRead(filename))
			using (TarArchive tarArchive = TarArchive.CreateInputTarArchive(inStream))
				tarArchive.ExtractContents(directory);
		}

		/// <summary>
		/// Extracts contents of GZipped Tar file to specified directory
		/// </summary>
		/// <param name="filename">Input .tar.gz file</param>
		/// <param name="directory">Output directory</param>
		public static void ExtractTarGz(string filename, string directory)
		{
			using (Stream inStream = File.OpenRead(filename))
			using (Stream gzipStream = new GZipInputStream(inStream))
			using (TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream))
				tarArchive.ExtractContents(directory);
		}


		/// <summary>
		/// Creates a GZipped Tar file from a source directory
		/// </summary>
		/// <param name="outputTarFilename">Output .tar.gz file</param>
		/// <param name="sourceDirectory">Input directory containing files to be added to GZipped tar archive</param>
		public static void CreateTarGz(string outputTarFilename, string sourceDirectory)
		{
			using (FileStream fs = new FileStream(outputTarFilename, FileMode.Create, FileAccess.Write, FileShare.None))
			using (Stream gzipStream = new GZipOutputStream(fs))
			using (TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzipStream))
				AddDirectoryFilesToTar(tarArchive, sourceDirectory, true);
		}

		/// <summary>
		/// Creates a Tar file from a source directory
		/// </summary>
		/// <param name="outputTarFilename">Output .tar file</param>
		/// <param name="sourceDirectory">Input directory containing files to be added to tar archive</param>
		public static void CreateTar(string outputTarFilename, string sourceDirectory)
		{
			using (FileStream fs = new FileStream(outputTarFilename, FileMode.Create, FileAccess.Write, FileShare.None))
			using (TarArchive tarArchive = TarArchive.CreateOutputTarArchive(fs))
				AddDirectoryFilesToTar(tarArchive, sourceDirectory, true);
		}

		/// <summary>
		/// Recursively adds folders and files to archive
		/// </summary>
		/// <param name="tarArchive"></param>
		/// <param name="sourceDirectory"></param>
		/// <param name="recurse"></param>
		public static void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
		{
			// Recursively add sub-folders
			if (recurse)
			{
				string[] directories = Directory.GetDirectories(sourceDirectory);
				foreach (string directory in directories)
					AddDirectoryFilesToTar(tarArchive, directory, recurse);
			}

			// Add files
			string[] filenames = Directory.GetFiles(sourceDirectory);
			foreach (string filename in filenames)
			{
				TarEntry tarEntry = TarEntry.CreateEntryFromFile(filename);
				tarArchive.WriteEntry(tarEntry, true);
			}
		}
	}
}