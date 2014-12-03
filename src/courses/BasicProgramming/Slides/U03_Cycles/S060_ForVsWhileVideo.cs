using System;

namespace uLearn.Courses.BasicProgramming.Slides.U03_Cycles
{
	[Slide("Сравнение for и while", "{65707781-7344-4805-81DA-C4A5199A7412}")]
	class S060_ForVsWhileVideo
	{
		//#video f0KjSjAJ6bA

		/*
		## Заметки по лекции
		*/

		public class Program
		{
			public static void Main()
			{


				//На самом деле, for(int i=0;i<5;i++) это синтаксический сахар для:
				int i = 0;
				while (i < 5)
				{
					// инструкции цикла
					i++;
				}

				for (; ; ) //"Ктулху-фор" — вечный цикл
				{
					break;
				}


				//Да, так тоже можно
				int sum = 0;
				for (
						var line = Console.ReadLine();
						line != "";
						sum += int.Parse(line), line = Console.ReadLine()
						) ;
			}
		}
		/*
		### Какой вид циклов использовать?
		
		for и while теоретически взаимозаменяемы, однако нужно понимать, 
		когда стоит использовать каждый из них. Вот рекомендации по выбору между for и while:

		*	Заранее известно сколько раз нужно повторить действие — используйте **for**.
		*	Есть переменная, изменяемая каждую итерацию и у нее есть понятный смысл — используйте **for**.
		*	Иначе — используйте **while**.
		*/
	}
}
