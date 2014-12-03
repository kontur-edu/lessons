using System;

namespace uLearn.Courses.BasicProgramming.Slides.U04_Arrays
{
	[Slide("Типы ссылки и типы значения", "{09364518-7781-43A3-BC0E-C37686D890C8}")]
    public class ArrayNull
    {
		//#video OMxsc6Csj4k
		/*
		[Иллюстрация карты памяти](_S050_ValueRferenceType.odp)
		*/
		/*
		## Заметки по лекции
		*/
        static int[] globalArray;
		
        public static void Main()
        {
            //Массив, как и строка, может быть равен null
            int[] localArray = null;

            //И, соответственно, неинициализированные глобальные переменные типа массивов
            //также равны null
            if (globalArray == null)
                Console.WriteLine("GlobalArray is null");

            //Этот код вызовет NullReferenceException
            Console.WriteLine(globalArray[0]);

            var intArray=new int[10];
            //Этот код выведет 0, поскольку массив чисел инициализирован нулями
            Console.WriteLine(intArray[0].ToString());

            var stringArray = new string[10];
            //Этот код вызовет exception, поскольку массив строк инициализирован значениями null
            Console.WriteLine(stringArray[0].ToString());

            //Почему так? Есть некая фундаментальная разница между массивами и строками с одной стороны
            //и числами - с другой. 
        }
    }
}