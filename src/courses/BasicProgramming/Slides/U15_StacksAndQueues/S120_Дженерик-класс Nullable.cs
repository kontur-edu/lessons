using System;
using System.Threading;

namespace uLearn.Courses.BasicProgramming.Slides.U15_StacksAndQueues
{
	[Slide("Дженерик-класс Nullable", "be6d71ce-2df0-4fe3-9be3-55770400199b")]
	class S120_Дженерик_класс_Nullable
	{
		//#video nc__prtwK8Y
		/*
		## Заметки по лекции
		*/
        static Random rnd = new Random();

        static int? GetNumber7()
        {
            for (int i = 0; i < 10; i++)
            {
                if (Console.KeyAvailable)
                    return rnd.Next(100);
                Thread.Sleep(100);
            }
            return null;
        }

        static void Main7()
        {
            var value = GetNumber7();
            if (value != null)
                Console.WriteLine(value);
            else
                Console.WriteLine("Error");
        }
    }
}
