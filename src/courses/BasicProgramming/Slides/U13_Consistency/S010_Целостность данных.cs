using System;

namespace uLearn.Courses.BasicProgramming.Slides.U13_Consistency
{
	[Slide("Целостность данных", "cd98d9f9-ecab-4aca-bbb7-1521280e1c8f")]
	class S010_Целостность_данных
	{
		//#video m-1qS-Sf6g0
		/*
		## Заметки по лекции
		*/

        public class Statistics
        {
            public int SuccessCount;
            public int TotalCount;
            public void Print()
            {
                Console.WriteLine("{0}%", (SuccessCount * 100) / TotalCount);
            }
        }

        public class Program
        {
            public static void Main()
            {
                var rnd = new Random();
                var stat = new Statistics();
                for (int i = 0; i < 1000; i++)
                {
                    if (rnd.Next(3) > 1) stat.SuccessCount++;
                    stat.TotalCount++;
                }
                stat.Print();

                //проблема в том, что никто нам не помешает написать вот так:
                stat.TotalCount = 100;
                stat.SuccessCount = 146;
                stat.Print();
            }
        }
    }
}
