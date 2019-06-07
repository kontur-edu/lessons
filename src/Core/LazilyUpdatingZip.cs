using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
using Ionic.Zlib;
using NUnit.Framework;
using Ulearn.Common;
using Ulearn.Common.Extensions;

namespace Ulearn.Core
{
	public class LazilyUpdatingZip
	{
		private readonly DirectoryInfo dir;
		private readonly string[] excludedDirs;
		private readonly Func<FileInfo, bool> needExcludeFile;
		private readonly Func<FileInfo, byte[]> getFileContent;
		private readonly FileInfo zipFile;
		private readonly IEnumerable<FileContent> filesToAdd;

		public LazilyUpdatingZip(DirectoryInfo dir, string[] excludedDirs, Func<FileInfo, bool> needExcludeFile, Func<FileInfo, byte[]> getFileContent, IEnumerable<FileContent> filesToAdd, FileInfo zipFile)
		{
			this.dir = dir;
			this.excludedDirs = excludedDirs;
			this.needExcludeFile = needExcludeFile;
			this.getFileContent = getFileContent;
			this.filesToAdd = filesToAdd;
			this.zipFile = zipFile;
		}

		public void UpdateZip()
		{
			if (IsActual())
				return;
			using (var zip = new ZipFile())
			{
				zip.CompressionLevel = CompressionLevel.BestSpeed;
				foreach (var f in EnumerateFiles())
				{
					var newContent = getFileContent(f);
					if (newContent == null)
						zip.AddFile(f.FullName, Path.GetDirectoryName(f.GetRelativePath(dir.FullName)));
					else
						zip.AddEntry(f.GetRelativePath(dir.FullName), newContent);
				}
				foreach (var fileToAdd in filesToAdd)
				{
					var directoriesList = GetDirectoriesList(fileToAdd.Path);
					if (!excludedDirs.Intersect(directoriesList).Any())
						zip.UpdateEntry(fileToAdd.Path, fileToAdd.Data);
				}
				zip.Save(zipFile.FullName);
			}
		}

		public static IEnumerable<string> GetDirectoriesList(string filename)
		{
			var pathParts = filename.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			return pathParts.Take(pathParts.Length - 1);
		}

		private bool IsActual()
		{
			var lastWriteTime = EnumerateFiles().Max(f => f.LastWriteTimeUtc);
			return zipFile.Exists && zipFile.LastWriteTimeUtc > lastWriteTime;
		}

		public IEnumerable<FileInfo> EnumerateFiles()
		{
			return EnumerateFiles(dir);
		}

		private IEnumerable<FileInfo> EnumerateFiles(DirectoryInfo aDir)
		{
			foreach (var f in aDir.GetFiles())
				if (!needExcludeFile(f))
					yield return f;
			var dirs = aDir.GetDirectories().Where(d => !excludedDirs.Contains(d.Name));
			foreach (var subdir in dirs)
				foreach (var f in EnumerateFiles(subdir))
					yield return f;
		}
	}

	[TestFixture]
	public class LazilyUpdatingZip_should
	{
		[Test]
		public void TestGetDirectoriesList()
		{
			const string fileName = "directory-1/directory-2/subdirectory/file.txt";
			var directories = LazilyUpdatingZip.GetDirectoriesList(fileName).ToList();
			CollectionAssert.Contains(directories, "directory-1");
			CollectionAssert.Contains(directories, "directory-2");
			CollectionAssert.Contains(directories, "subdirectory");
			CollectionAssert.DoesNotContain(directories, "file.txt");
		}
	}
}