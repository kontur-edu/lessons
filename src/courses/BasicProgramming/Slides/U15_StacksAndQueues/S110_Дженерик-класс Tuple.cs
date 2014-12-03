using System;
using System.Threading;

namespace uLearn.Courses.BasicProgramming.Slides.U15_StacksAndQueues
{
	[Slide("Дженерик-класс Tuple", "77f0459d-ec14-4300-a796-71f30528229e")]
	class S110_Дженерик_класс_Tuple
	{
		//#video Xk1Ks98JbhM
		/*
		## Заметки по лекции
		*/

        static Random rnd = new Random();

        public class IntReply
        {
            public int Number { get; set; }
            public bool Available { get; set; }
        }

        //Но все же out и ref - это не очень удачно.
        //Лучше просто возвращать объект
        public static IntReply GetNumber5()
        {
            for (int i = 0; i < 10; i++)
            {
                if (Console.KeyAvailable)
                    return new IntReply { Available = true, Number = rnd.Next(100) };
                Thread.Sleep(100);
            }
            return new IntReply { Available = false };
        }

        static void Main5()
        {
            var value = GetNumber5();
            if (value.Available)
                Console.WriteLine(value.Number);
            else
                Console.WriteLine("Error");
        }

    }
}
