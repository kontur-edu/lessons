using System;

namespace uLearn.Courses.BasicProgramming.Slides.U13_Consistency
{
	[Slide("Статические конструкторы", "73a67604-e583-417a-96a8-3e8740a6de25")]
	class S080_Статические_конструкторы
	{
		//#video i8IkV-y3fps
		/*
		## Заметки по лекции
		*/
        class Test
        {
            public static readonly DateTime Readonly;
            public readonly int Number;
            //Статические конструкторы всегда без параметров
            static Test()
            {
                Readonly = DateTime.Now;
            }
            //это динамический конструктор
            public Test()
            {
                Number = 3;
            }
        }

        class Program
        {
            public static void Main()
            {
                var test = new Test();
                //сначала вызовется статический конструктор (настройка типа)
                //и только после этого - динамический
            }
        }
    }
}
