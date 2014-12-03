using System;

namespace uLearn.Courses.BasicProgramming.Slides.U04_Arrays
{
	[Slide("Многомерные массивы", "{04957308-829A-41FE-AA60-7D553E2AAC1E}")]
    public class S060_MultiDimensionalArray
    {
		//#video tlE1-K0JE5w
		/*
		[Иллюстрация карты памяти](_S070_GraphicDimensional.odp)
		*/
		/*
		## Заметки по лекции
		*/
        public static void Main()
        {
            //Двумерные массивы имеют тип int[,] (соответственно, double[,], string[,] и т.д.)
            int[,] twoDimensionalArray = new int[2, 3];

            //Адресация двумерных массивов также идет через запятую
            twoDimensionalArray[1,2]=1;

            for (int i = 0; i < twoDimensionalArray.GetLength(0); i++)
                for (int j = 0; j < twoDimensionalArray.GetLength(1); j++)
                    twoDimensionalArray[i, j] = 10*i + j;
            //В памяти значения хранятся в следующем порядке: 0,1,2,10,11,12
            Console.WriteLine(twoDimensionalArray.Length); 
            //Метод выше напечатает 6. Длины размерностей нужно получать через метод GetLength
            //Могут быть массивы сколь угодно большой размерности
            int[, ,] threeDimensionalArray = new int[2, 3, 4];


        }
    }
}