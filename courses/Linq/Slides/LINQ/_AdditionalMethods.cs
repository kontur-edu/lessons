using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace uLearn.Courses.Linq.Slides.LINQ
{
	public class S012_AdditionalMethods
	{
		/*
		 Пора познакомиться еще с несколькими простыми, но часто используемыми методами.

		`Take` обрезает последовательность после указанного количества элементов.
		
		`IEnumerable<T> Take(this IEnumerable<T> items, int count)`

		`Skip` обрезает последовательность, пропуская указанное количество элементов с начала.

		`IEnumerable<T> Skip(this IEnumerable<T> items, int count)`

		`ToArray` и `ToList` используются для преобразования `IEnumerable<T>` в массив `T[]` или в `List<T>`, соответственно.
		
		Эти методы, как и предыдущие, не меняют исходную коллекцию, а возвращают новую последовательность.

		Можете изучить пример, показывающий работу всех этих методов вместе:
		*/

		[Test]
		public void BasicFunctional()
		{
			int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

			IEnumerable<int> even = numbers.Where(x => x % 2 == 0);
			IEnumerable<int> squares = even.Select(x => x * x);
			IEnumerable<int> squaresWithoutOne = squares.Skip(1);
			IEnumerable<int> secondAndThirdSquares = squaresWithoutOne.Take(2);
			int[] result = secondAndThirdSquares.ToArray();

			// `Assert.That` — это метод библиотеки NUnit. Он проверяет истинность некоторого условия. 
			// В данном случае, что result — это массив из двух элементов 16 и 36.
			Assert.That(result, Is.EqualTo(new[] { 16, 36 }));
		}

	}
}