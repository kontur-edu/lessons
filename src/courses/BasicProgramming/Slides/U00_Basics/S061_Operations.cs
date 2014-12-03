using System;
using uLearn.CSharp;

namespace uLearn.Courses.BasicProgramming.Slides.U00_Basics
{
	[Slide("Использование var", "{956A2160-CEB6-4485-8A94-C39DFE364F48}")]
	public class S061_Operations : SlideTestBase
	{
		/*
		С прибавлением единицы все оказалось просто. Казалось бы прибавление к числу половинки должно быть не сложнее...

		Подумайте, как так получилось, что казалось бы корректная программа не работает.
		Исправьте программу так, чтобы она давала корректный ответ.
		*/

		[Exercise]
		[Hint("Вспомните, как именно работает var. Какой тип у переменной a?")]
		[ExpectedOutput("5.5")]
		static public void Main()
		{
			double a = 5;
			a += 0.5;
			Console.WriteLine(a);
			/*uncomment
			var a = 5;
			a += 0.5;
			Console.WriteLine(a);
			*/
		}


	}
}
