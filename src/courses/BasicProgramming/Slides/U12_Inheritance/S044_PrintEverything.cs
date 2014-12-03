using System;

namespace uLearn.Courses.BasicProgramming.Slides.U12_Inheritance
{
	[Slide("Всем печать!", "{039B257E-D0F4-4BDD-99A2-66ADE6485165}")]
	public class S044_PrintEverything : SlideTestBase
	{
		/*
		Напишите метод, который печатает все, что угодно, через запятую.
		*/
		[ExpectedOutput("1, 2\na, b\n1, a\nTrue, a, 1")]
		[Hint("Для создания метода с переменным количеством аргументов используйте ключевое слово params")]
		[Hint("object — это универсальный базовый тип")]
		[Hint("static void Print(params object[] arguments)")]
		public static void Main()
		{
			Print(1, 2);
			Print("a", 'b');
			Print(1, "a");
			Print(true, "a", 1);
		}

		[ExcludeFromSolution]
		[HideOnSlide]
		static void Print(params object[] arguments)
		{
			bool firstTime = true;
			foreach (var e in arguments)
			{
				if (!firstTime)
					Console.Write(", ");
				else
					firstTime = false;
				Console.Write(e);
			}
			Console.WriteLine();
		}
		/*uncomment
		public static void Print(...)
		{
			for(var i=0; i<...){
				if (i > 0) 
					Console.Write(", ");
				Console.Write(...);
			}
			Console.WriteLine();
		}
		*/
	}
}