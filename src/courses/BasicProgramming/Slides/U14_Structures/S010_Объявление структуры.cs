namespace uLearn.Courses.BasicProgramming.Slides.U14_Structures
{
    [Slide("Объявление структуры", "c151954b-6d81-45a4-8744-e5906f851709")]
    class S010_Объявление_структуры
    {
        //#video c18ptIGEwnI
        /*
        ## Заметки по лекции
        */

        // Так объявляются структуры: собственный тип-значение
        struct Point
        {
            public int X;
            public int Y;
            public void Test() { }
        }


        public class Program
        {
            static Point staticPoint;

            Point dynamicPoint;

            static void MainX()
            {
                //Для структур все эти инструкции будут работать
                //Для классов - не скомпилируются или выкинут null reference exception.
                Point localPoint;
                localPoint.X = 10;
                localPoint.Y = 10;

                var array = new Point[10];
                array[0].X = 10;

                staticPoint.X = 10;

                var instance = new Program();
                instance.dynamicPoint.X = 10;


            }
        }
    }
}
