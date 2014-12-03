using System;

namespace uLearn.Courses.BasicProgramming.Slides.U14_Structures
{
	[Slide("Передача структуры в метод", "f15a5143-f505-48dc-90dd-d1d0d5878091")]
	class S030_Передача_структуры_в_метод
	{
		//#video yC8S-IBJO0E
		/*
		## Заметки по лекции
		*/

        struct PointStruct
        {
            public int X;
            public int Y;
        }

        class PointClass
        {
            public int X;
            public int Y;
        }

        public class Program1
        {
            static void ProcessStruct(PointStruct point)
            {
                point.X = 10;
            }
            static void ProcessClass(PointClass point)
            {
                point.X = 10;
            }
            public static void Main()
            {
                var pointStruct = new PointStruct();
                ProcessStruct(pointStruct);
                Console.WriteLine(pointStruct.X);//напечатает 10, т.е. структуры копируются
                
                var pointClass = new PointClass();
                ProcessClass(pointClass);
                Console.WriteLine(pointClass.X); //напечатает 0, т.к. объект передается по ссылке
           }
        }

        public class MyClass
        {
            public int classField;
        }

        public struct MyStruct
        {
            public MyClass myObject;
        }

        public class Program2
        {
            public static void ProcessStruct(MyStruct str)
            {
                str.myObject.classField = 10;
            }

            public static void Main()
            {
                var str = new MyStruct();
                str.myObject = new MyClass();
                ProcessStruct(str);
                Console.WriteLine(str.myObject.classField);
            }
        }
    }
}
