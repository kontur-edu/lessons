using System;
using System.Linq;

namespace uLearn.Courses.BasicProgramming.Slides.U12_Inheritance
{
	[Slide("Склейка массивов", "{3A0B67FC-73C1-498A-B015-7CCC06B7FFC9}")]
	public class S046_CombineArrays : SlideTestBase
	{
		/*
		Реализуйте метод Combine, который возвращает массив, собранный из переданных массивов.
		
		Для того, чтобы создать новый массив, используйте статический метод `Array.CreateInstance`, принимающий тип элемента массива.
		
		Для того, чтобы узнать тип элементов в переданном массиве, используйте `myArray.GetType().GetElementType()`.
 
		Проверьте, что типы элементов совпадают во всех переданных массивах!
		 
		Если результирующий массив создать нельзя, возвращайте `null`.
		*/
		[ExpectedOutput(
@"1 2 1 2 
1 2 1 2 1 2 
1 2 
null
A B A B 
null")]
		[Hint("Для создания метода с переменным количеством аргументом, используйте ключевое слово params")]
		[Hint("static Array Combine(params Array[] arrays) { ...")]
		[Hint("var elementType=arrays[0].GetType().GetElementType();")]
		[Hint("var result=Array.CreateInstance(elementType, summaryLength);")]
		public static void Main()
		{
			var ints = new[] { 1, 2 };
			var strings = new[] { "A", "B" };

			Print(Combine(ints, ints));
			Print(Combine(ints, ints, ints));
			Print(Combine(ints));
			Print(Combine());
			Print(Combine(strings, strings));
			Print(Combine(ints, strings));
		}

		static void Print(Array array)
		{
			if (array == null)
			{
				Console.WriteLine("null");
				return;
			}
			for (int i = 0; i < array.Length; i++)
				Console.Write("{0} ", array.GetValue(i));
			Console.WriteLine();
		}

		[ExcludeFromSolution]
		[HideOnSlide]
		static Array Combine(params Array[] arrays)
		{
			if (arrays.Length == 0) return null;
			var type = arrays[0].GetType().GetElementType();
			if (arrays.Any(z => z.GetType().GetElementType() != type))
				return null;
			var result = Array.CreateInstance(type, arrays.Sum(z => z.Length));
			var ptr = 0;
			for (int i = 0; i < arrays.Length; i++)
				for (int j = 0; j < arrays[i].Length; j++)
					result.SetValue(arrays[i].GetValue(j), ptr++);
			return result;
		}

	}
}