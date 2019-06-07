using System;
using System.Linq;
using System.Text.RegularExpressions;
using uLearn.CSharp;

namespace uLearn.Courses.Linq.Slides.LINQ.LINQ.LINQ
{
	public class S080_SortExercise : SlideTestBase
	{
		/*

		Текст задан массивом строк. 
		Вам нужно составить лексикографически упорядоченный список всех встречающихся в этом тексте слов.

		Слова нужно сравнивать регистронезависимо, а выводить в нижнем регистре.
		*/

		public static string[] GetSortedWords(params string[] textLines)
		{
			return textLines.SelectMany(
				line => Regex.Split(line, @"\W+")
					.Where(word => word != "")
					.Select(word => word.ToLower())
				)
				.Distinct()
				.OrderBy(word => word)
				.ToArray();
			// ваше решение
		}

		public static void Main()
		{
			var vocabulary = GetSortedWords(
				"Hello, hello, hello, how low",
				"",
				"With the lights out, it's less dangerous",
				"Here we are now; entertain us",
				"I feel stupid and contagious",
				"Here we are now; entertain us",
				"A mulatto, an albino, a mosquito, my libido...",
				"Yeah, hey"
			);
			foreach (var word in vocabulary)
				Console.WriteLine(word);
		}
	}
}