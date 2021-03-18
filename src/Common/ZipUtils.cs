using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ionic.Zip;
using JetBrains.Annotations;
using Microsoft.IO;

namespace Ulearn.Common
{
	public static class ZipUtils
	{
		public static MemoryStream CreateZipFromDirectory([NotNull]List<string> directoriesToInclude, [CanBeNull]List<string> excludeCriterias,
			[CanBeNull]IEnumerable<FileContent> filesToUpdateOrCreate, [CanBeNull]Encoding encoding)
		{
			if (excludeCriterias == null)
				excludeCriterias = excludeCriterias.EmptyIfNull().ToList();
			var excludeRegexps = GetExcludeRegexps(excludeCriterias).ToList();
			using (var zip = encoding != null ? new ZipFile(encoding) : new ZipFile())
			{
				foreach (var pathToDirectory in directoriesToInclude)
				{
					var allFiles = Directory.GetFiles(pathToDirectory, "*.*", SearchOption.AllDirectories)
						.Select(p => p.Replace(pathToDirectory, "").TrimStart('/').TrimStart('\\')).ToList();
					foreach (var filePath in allFiles)
					{
						var isExcluded = IsExcluded(filePath, excludeRegexps);
						if(isExcluded)
							continue;
						var fullPath = Path.Combine(pathToDirectory, filePath);
						var bytes = File.ReadAllBytes(fullPath);
						zip.UpdateEntry(filePath, bytes);
					}
				}
				foreach (var zipUpdateData in filesToUpdateOrCreate.EmptyIfNull())
				{
					var isExcluded = IsExcluded(zipUpdateData.Path, excludeRegexps);
					if(isExcluded)
						continue;
					zip.UpdateEntry(zipUpdateData.Path, zipUpdateData.Data);
				}
				var ms = StaticRecyclableMemoryStreamManager.Manager.GetStream();
				zip.Save(ms);
				ms.Position = 0;
				return ms;
			}
		}

		private static bool IsExcluded(string filePath, List<Regex> excludeRegexps)
		{
			filePath = filePath.Replace('\\', '/');
			foreach (var regex in excludeRegexps)
			{
				if (regex.IsMatch(filePath))
					return true;
			}
			return false;
		}
		
		// Пути не имеют ведущего /
		// Если / в конце, то папка, иначе файл
		// Если / в начале, то ищем от корневой директории, иначе путь можнт начинаться в поддиректории
		// * - 0 или более любых символов, кроме /
		// ? - 0 или 1 любой символ, кроме /
		private static IEnumerable<Regex> GetExcludeRegexps(List<string> excludeCriterias)
		{
			foreach (var excludeCriteria in excludeCriterias)
			{
				var criterion = excludeCriteria.Trim();
				criterion = criterion.Replace('\\', '/');
				if (criterion.Length == 0)
					continue;
				var isDirectory = false;
				if (criterion.Last() == '/')
				{
					isDirectory = true;
					criterion = criterion.Substring(0, criterion.Length - 1);
				}
				var isPathFromRoot = criterion.StartsWith("/");
				if (isPathFromRoot)
					criterion = criterion.Remove(0, 1);
				var regexpSb = new StringBuilder();
				regexpSb.Append(isPathFromRoot ? "^" : "(^|/)");
				var specChars = "[]^$.|+(){}";
				foreach (var specChar in specChars)
					criterion = criterion.Replace(specChar.ToString(), "\\" + specChar);
				criterion = criterion
					.Replace("*", "[^/]*")
					.Replace("?", "[^/]?");
				regexpSb.Append(criterion);
				regexpSb.Append(isDirectory ? '/' : '$');
				var regex = new Regex(regexpSb.ToString(), RegexOptions.Compiled);
				yield return regex;
			}
		}
	}
}