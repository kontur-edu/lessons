using System;
using System.Collections.Generic;
using System.Linq;
using uLearn.CSharp;

namespace uLearn.Courses.Linq.Slides.LINQ.LINQ.LINQ
{
	public class S020_ReadNumbersExercise : SlideTestBase
	{
		/*
		`LINQ` удобно использовать для чтения из файла и разбора простых текстовых форматов.
		Особенно удобно сочетать методы `LINQ` с методами класса `File`: 
		`File.ReadLines(filename)`, `File.WriteLines(filename, lines)`.

		Пусть у вас есть файл, в котором каждая строка либо пустая, либо содержит одно целое число. 
		Кто-то уже вызвал метод `File.ReadAllLines(filename)` и теперь у вас есть массив всех строк этого файла.

		У вас даже есть метод `Main`, запускающий ваш метод на тестовых данных:
		*/

		public static void Main()
		{
			foreach (var num in ParseNumbers(new[] {"-0", "+0000"}))
				Console.WriteLine(num);
			foreach (var num in ParseNumbers(new List<string> {"1", "", "-03", "0"}))
				Console.WriteLine(num);
		}

		/*
		Реализуйте метод `ParseNumbers` в одно `LINQ`-выражение.
		*/

		public static int[] ParseNumbers(IEnumerable<string> lines)
		{
			return lines
				.Where(line => line != "")
				.Select(int.Parse)
				.ToArray();
			/*uncomment
			return lines
				.Where(...)
				.Select(...)
				...
			*/
		}
	}
}