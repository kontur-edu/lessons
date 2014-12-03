using System;

namespace uLearn.Courses.BasicProgramming.Slides.U14_Structures
{
	[Slide("Боксинг и анбоксинг", "3ce66559-68b8-4d55-8a32-a961b8281952")]
	class S050_Боксинг_и_анбоксинг
	{
		//#video FiRKqdQV-hQ
		/*
		## Заметки по лекции
		*/

        struct MyStruct
        {
            public int field;
        }
        
        //При апкасте структуры к object и последующем даункасте,
        //дважды происходит копирование
        public class Program
        {
            public static void MainX()
            {
                MyStruct original;
                original.field = 10;

                object boxed = (object)original;
                MyStruct unboxed = (MyStruct)boxed;

                unboxed.field = 20;

                Console.WriteLine(original.field);
                Console.WriteLine(unboxed.field);
            }
        }
    }
}
