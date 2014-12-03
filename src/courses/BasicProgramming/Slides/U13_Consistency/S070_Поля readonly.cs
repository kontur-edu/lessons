using System;

namespace uLearn.Courses.BasicProgramming.Slides.U13_Consistency
{
    [Slide("Поля readonly", "ac9cb965-c780-4117-abae-8f1ad41a08c9")]
    class S070_Поля_readonly
    {
        //#video cNSd8kDKK0E
        /*
        ## Заметки по лекции
        */
        public class TournirInfo
        {
            //readonly поле - это еще более сильное ограничение целостности
            //такие поля можно присваивать только в конструкторе
            public readonly int TeamsCount;
            public readonly string[] TeamsNames;
            public readonly double[,] Scores;
            public TournirInfo(int teamsCount)
            {
                TeamsCount = teamsCount;
                TeamsNames = new string[teamsCount];
                Scores = new double[teamsCount, teamsCount];
            }

            public void SomeMethod()
            {
                // TeamsCount = 4; //так писать нельзя, хотя мы внутри класса
            }
        }

        public class Program
        {
            public static void Main()
            {
                var info = new TournirInfo(4);
                // info.TeamsCount = 5; //так тоже нельзя, хотя поле public
            }
        }

        //Разница между readonly полями и константами заключается в том,
        //что readonly поля присваиваются при вызове конструктора (возможно, неявно),
        //а константы - во время компиляции программы.

        class Test
        {
            public readonly DateTime Time = DateTime.Now; 
            //Вызов Now произойдет при создании объекта
            //в неявно созданном конструкторе
        }


    }
}