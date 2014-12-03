using System;

namespace uLearn.Courses.BasicProgramming.Slides.U14_Structures
{
    [Slide("Ключевое слово ref", "0c03ce51-c850-4dee-b632-f84a114998c3")]
    class S040_Ключевое_слово_ref
    {
        //#video BoKrLQRPjpA
        /*
        ## Заметки по лекции
        */

        struct Point
        {
            public int X;
            public int Y;
        }

        //Ключевое слово ref позволяет передавать в метод не копию структуры,
        //а ссылку на то место, где хранится оригинальная структура
        public class Program
        {
            static void ProcessStruct(ref Point point)
            {
                point.X = 10;
            }


            static void ProcessInt(ref int n)
            {
                n = 10;
            }

            static void ProcessArray(ref int[] array)
            {
                array = new int[2];
            }

            
            static void Main()
            {
                Point point = new Point();
                ProcessStruct(ref point);
                Console.WriteLine(point.X);

                int n = 0;
                ProcessInt(ref n);
                Console.WriteLine(n);

                var array = new int[3];
                ProcessArray(ref array);
                Console.WriteLine(array.Length);
            }
        }
    }
}