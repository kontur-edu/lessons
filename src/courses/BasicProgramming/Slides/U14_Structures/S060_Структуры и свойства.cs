namespace uLearn.Courses.BasicProgramming.Slides.U14_Structures
{
	[Slide("Структуры и свойства", "b6bcf37a-2c46-4804-b1f0-e0c742e07fbb")]
	class S060_Структуры_и_свойства
	{
		//#video NHqkpRjh5jU
		/*
		## Заметки по лекции
		*/

        public struct Point
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class Rectangle
        {
            public Point Point { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        public class Program
        {
            static void MainX()
            {
                Point point;
                // point.X = 10; // Так писать нельзя. Сеттер - это метод, а метод
                // можно вызывать, только если структура полностью
                // проинициализирована
                point = new Point();
                point.X = 10;    // Теперь так писать можно

                var rectangle = new Rectangle();
                //rectangle.Point.X = 10; //Так писать нельзя. Rectangle.Point - это сеттер,
                //сеттер - это метод, и как изменить значение, 
                // возвращенное методом и нигде не сохраненное?
                point = rectangle.Point;
                point.X = 10; //так писать можно, но к изменению прямоугольника это не приведет
                //поскольку будет изменена копия, сохраненная в стэке метода Main
                rectangle.Point = new Point { X = 10, Y = 10 }; //Вот это другое дело.

                //Если бы rectangle.Point было полем, а не свойством, то этой проблемы бы не было

            }
        }
    }
}
