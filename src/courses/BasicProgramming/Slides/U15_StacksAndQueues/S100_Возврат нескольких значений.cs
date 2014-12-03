using System;
using System.Threading;

namespace uLearn.Courses.BasicProgramming.Slides.U15_StacksAndQueues
{
    [Slide("Возврат нескольких значений", "9c8da7bf-3c85-4f19-84f7-a86ffa1fdb30")]
    class S100_Возврат_нескольких_значений
    {
        //#video tVIO43KW7Lk
        /*
        ## Заметки по лекции
        */

        static Random rnd = new Random();


        //Можно так, через ключевое слово ref.
        //Но ref устанавливает двустороннее соединение между методами,
        //чего нужно избегать из архитектурных соображений
        public static bool GetNumber3(ref int value)
        {
            for (int i = 0; i < 10; i++)
            {
                if (Console.KeyAvailable)
                {
                    value = rnd.Next(100);
                    return true;
                }
                Thread.Sleep(100);
            }
            return false;
        }

        static void Main3()
        {
            int value = 0;
            if (!GetNumber3(ref value))
                Console.WriteLine("Error");
            else
                Console.WriteLine(value);
        }


        //Лучше использовать out-параметр.
        //Он работает примерно также, как ref, но является односторонним
        public static bool GetNumber4(out int value)
        {
            //Console.WriteLine(value); // value не может быть использовано до присвоения внутри метода
            for (int i = 0; i < 10; i++)
            {
                if (Console.KeyAvailable)
                {
                    value = rnd.Next(100);
                    return true;
                }
                Thread.Sleep(100);
            }
            //value обязано быть присвоено до выхода из метода
            value = 0;
            return false;
        }

        static void Main4()
        {
            int value;
            if (!GetNumber4(out value))
                Console.WriteLine("Error");
            else
                Console.WriteLine(value);
        }
    }
}