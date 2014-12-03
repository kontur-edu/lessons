using System;

namespace uLearn.Courses.BasicProgramming.Slides.U01_Mistakes
{
	[Slide("Ошибки на этапе выполнения", "{F3C57AC8-D153-4E3A-A898-1668205AFBFC}")]
	class S040_RuntimeExceptionsVideo
	{
		//#video Nn_Dba8MqB0

		/*
		## Заметки по лекции

		Есть большое множество разнообразных ошибок на этапе выполнения. Вот несколько примеров:
		
		### Null Reference Exception

		Все поля заполняются значениями по умолчанию. Для типа string значение по умолчанию — null — отсутствие объекта.

		Любая попытка обратиться к null вызывает исключение Null reference exception
		*/

		class Program
		{
			static string myString;

			static void Main()
			{
				Console.WriteLine(myString[0]);
			}
		}

		/*
		### Вложенные ошибки
 
		В этом случае, исключение будет не информативным, а вся существенная информация об ошибке будет находится в свойстве InnerException.
		
		Однако лучше избегать такого рода ошибок, не выполняя сложных действий в инициализаторах статических полей.
		*/

		class Program2
		{
			static int variable = int.Parse(Console.ReadLine());

			static void Main()
			{
				Console.WriteLine(variable);
			}
		}

		/*
		### Деление на ноль

		При делении целочисленного числа на целочисленный ноль, возникает исключение.

		Любое исключение кроме сообщения об ошибке содержит в себе информацию о стеке вызовов, то есть последовательности вызывавшихся методов.
		Эта информация помогает локализовать проблему.

		*/

		//#include _DivisionByZero.cs
	}
}
