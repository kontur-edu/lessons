using System;

namespace uLearn.Courses.BasicProgramming.Slides.U13_Consistency
{
	[Slide("Ключевое слово private", "59e03b07-750a-4f8c-9936-b4025c097ee7")]
	class S020_Ключевое_слово_private
	{
		//#video 5xrSyyjSiGA
		/*
		## Заметки по лекции
		*/
        public class Statistics
        {
            private int totalCount;
            private int successCount;
            public void AccountCase(bool isSuccess)
            {
                if (isSuccess) successCount++;
                totalCount++;
            }
            public void Print()
            {
                Console.WriteLine("{0}%", (successCount * 100) / totalCount);
            }
        }

        public class Program
        {
            public static void Main()
            {
                var rnd = new Random();
                var stat = new Statistics();
                for (int i = 0; i < 1000; i++)
                    stat.AccountCase(rnd.Next(3) > 1);
                stat.Print();

                //Теперь так сделать нельзя: доступ к приватным полям возможен только изнутри класса
                //stat.totalCount = 100;
                //stat.successCount = 146;
            }
        }
    }
}
