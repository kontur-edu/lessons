using System;
using System.Threading;

namespace uLearn.Courses.BasicProgramming.Slides.U15_StacksAndQueues
{
    [Slide("Возврат из метода значения и ошибки", "93505d6a-12d4-4a9a-9a4d-02d4af129fbd")]
    class S090_Возврат_из_метода_значения_и_ошибки
    {
        //#video z_eFycGQDfI
        /*
        ## Заметки по лекции
        */

        static Random rnd = new Random();

        //Разрабатываем метод, который возвращает число,
        //если пользователь успел нажать клавишу,
        //и что-то, если не успел
        public static int GetNumber1()
        {
            for (int i = 0; i < 10; i++)
            {
                if (Console.KeyAvailable)
                    return rnd.Next(100);
                Thread.Sleep(100);
            }
            // Так делать очень плохо. Потому что -1 - это такое же число, как и другие
            // и требуется специальная договоренность
            return -1;
        }

        static void Main1()
        {
            var value = GetNumber1();
            if (value == -1)
                Console.WriteLine("Error");
            else
                Console.WriteLine(value);
        }


        public static int GetNumber2()
        {
            for (int i = 0; i < 10; i++)
            {
                if (Console.KeyAvailable)
                    return rnd.Next(100);
                Thread.Sleep(100);
            }
            // Это нормально, если отсутствие ввода - действительно исключительная,
            // ошибочная ситуация. Но иногда это не так. Иногда просто хочется
            // вернуть два значения - флаг ввода и его содержимое
            throw new Exception();
        }

        static void Main2()
        {
            try
            {
                Console.WriteLine(GetNumber2());
            }
            catch
            {
                Console.WriteLine("Error");
            }
        }
    }
}