using System;
using System.Linq;
using System.Text.RegularExpressions;
using uLearn.CSharp;

namespace uLearn.Courses.Linq.Slides.LINQ.LINQ.LINQ
{
	public class S140_GroupingExercise : SlideTestBase
	{
		/*

		Дан текст, нужно вывести `count` наиболее часто встречающихся в тексте слов вместе с их частотой.
		Среди слов, встречающихся одинаково часто, отдавать предпочтение лексикографически меньшим словам.
		Слова сравнивать регистронезависимо и выводить в нижнем регистре.
		
		Напомним сигнатуры некоторых `LINQ`-методов, которые могут понадобиться в этом упражнении:

		    IEnumerable<IGrouping<K, T>>    GroupBy(this IEnumerable<T> items, Func<T, K> keySelector)
		    IOrderedEnumerable<T>           OrderBy(this IEnumerable<T> items, Func<T, K> keySelector)
		    IOrderedEnumerable<T> OrderByDescending(this IEnumerable<T> items, Func<T, K> keySelector)
		    IOrderedEnumerable<T>            ThenBy(this IOrderedEnumerable<T> items, Func<T, K> keySelector)
		    IOrderedEnumerable<T>  ThenByDescending(this IOrderedEnumerable<T> items, Func<T, K> keySelector)
		    IEnumerable<T>                     Take(this IEnumerable<T> items, int count)
		*/

		public static Tuple<string, int>[] GetMostFrequentWords(string text, int count)
		{
			return Regex.Split(text, @"\W+")
				.Where(word => word != "")
				.GroupBy(w => w.ToLower())
				.Select(group => Tuple.Create(group.Key, group.Count()))
				.OrderByDescending(tuple => tuple.Item2)
				.ThenBy(tuple => tuple.Item1)
				.Take(count)
				.ToArray();
			/*uncomment
			return Regex.Split(text, @"\W+")
				.Where(word => word != "")
				// ваш код
				.ToArray();
			*/
		}

		public static void Main()
		{
			CheckOn(2, "A box of biscuits, a box of mixed biscuits, and a biscuit mixer.");
			CheckOn(100, "");
			CheckOn(3, "Each Easter Eddie eats eighty Easter eggs.");
		}

		private static void CheckOn(int count, string text)
		{
			Console.WriteLine("GetMostFrequentWords(\"{0}\", {1})", text, count);
			var sortedList = GetMostFrequentWords(text, count);
			Console.WriteLine(string.Join(Environment.NewLine, sortedList.Select(t => "  " + t.Item1 + " " + t.Item2).ToArray()));
			Console.WriteLine();
		}
	}
}