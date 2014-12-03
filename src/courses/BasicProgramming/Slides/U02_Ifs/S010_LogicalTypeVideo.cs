using System;

namespace uLearn.Courses.BasicProgramming.Slides.U02_Ifs
{
	[Slide("Сравнение и логический тип", "{2C4919E2-42BF-4B32-93DC-5D54A8FDB25C}")]
	class S010_LogicalTypeVideo
	{
		//#video 6jo2hk4ntV8

		/*
		## Заметки по лекции
		*/
		public static void Main()
		{
			//Сравнение, как и операции сложения или деления, тоже имеет возвращаемое значение
			Console.WriteLine(5 < 4);

			// И его можно сохранить в переменную логического типа
			// Это тип, абсолютно равноправный с int, double и другими 
			bool comparisonResult = 6 > 7;
			Console.WriteLine(comparisonResult);

			//Константы истины и лжи
			bool trueValue = true;
			bool falseValue = false;

			//Все операции сравнения
			Console.WriteLine(5 == 6);
			Console.WriteLine(5 != 6);
			Console.WriteLine(5 < 5);
			Console.WriteLine(5 <= 5);
			Console.WriteLine(5 > 5);
			Console.WriteLine(5 >= 5);

			// Операция "И", или конъюнкция
			Console.WriteLine((5 < 4) && (3 < 4));

			// Операция "ИЛИ", или дизъюнкция
			Console.WriteLine((5 < 4) || (3 < 4));

			// Операция "НЕ", или отрицание
			Console.WriteLine(!(5 < 4));
		}
	}
}
