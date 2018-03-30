using System.Collections.Generic;
using System.IO;
using System.Linq;
using uLearn.SpellChecking;

namespace uLearn.Extensions
{
	public static class SpellCheckExtensions
	{
		public static string[] SpellCheck(this Course course)
		{
			using (var spellchecker = new SpellChecker(course.TryGetDictionaryPath()))
			{
				return spellchecker.SpellCheckCourse(course);
			}
		}

		public static string[] SpellCheckCourse(this SpellChecker spellchecker, Course course)
		{
			var titleErrors = spellchecker.SpellCheckString(course.Title).Select(e => e.ToPrettyString()).ToList();
			var titleError = ToPrettyMessage("Заголовок курса:", titleErrors);

			var unitsTitlesErrors = course.Units
				.Select(u => u.Title)
				.SelectMany(spellchecker.SpellCheckString)
				.Select(e => e.ToPrettyString())
				.ToList();
			var unitsTitlesError = ToPrettyMessage("Заголовки модулей:", unitsTitlesErrors);

			var slidesErrors = course.Slides.Select(spellchecker.SpellCheckSlide).Where(s => !string.IsNullOrWhiteSpace(s));

			var res = new List<string> { titleError, unitsTitlesError };
			res.AddRange(slidesErrors);
			return res.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
		}

		public static string SpellCheckSlide(this SpellChecker spellchecker, Slide slide)
		{
			var prefix = string.Format("{0} ({1}):", slide.Title, slide.NormalizedGuid);
			var titleErrors = spellchecker.SpellCheckString(slide.Title);
			var blocksErrors = slide.Blocks.Select(b => b.TryGetText()).Where(s => !string.IsNullOrWhiteSpace(s)).SelectMany(spellchecker.SpellCheckString);
			var errorsList = titleErrors.Concat(blocksErrors).Select(e => e.ToPrettyString()).ToList();
			return ToPrettyMessage(prefix, errorsList);
		}

		public static string ToPrettyString(this SpellingError error)
		{
			return string.Format("Найдено '{0}', возможные варианты: [{1}]", error.Mispelling, string.Join(", ", error.Suggestions));
		}

		private static string ToPrettyMessage(string title, IList<string> errors)
		{
			if (!errors.Any())
				return null;
			var errorsString = string.Join("\n", errors.Select(s => '\t' + s));
			return title + '\n' + errorsString;
		}

		private static string TryGetDictionaryPath(this Course course)
		{
			var file = Path.Combine(course.Directory.FullName, course.Settings.GetDictionaryFile());
			return File.Exists(file) ? file : null;
		}
	}
}