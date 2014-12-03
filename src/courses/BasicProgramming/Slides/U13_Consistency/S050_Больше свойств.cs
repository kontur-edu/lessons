using System;

namespace uLearn.Courses.BasicProgramming.Slides.U13_Consistency
{
	[Slide("Больше свойств", "632b5529-8873-4084-b3aa-186e5c26cc3d")]
	class S050_Больше_свойств
	{
		//#video HxyT3aOeq_M
        /*
        ## Заметки по лекции
        */
        public class Student
        {
            //Даже в тех случаях, когда поля не требуют процедуры проверки целостности
            //используют свойства.
            public string lastName;
            public string LastName
            {
                get { return lastName; } //часто здесь пишут LastName вместо lastName, получают рекурсию и переполнение стэка. Осторожнее!
                set { lastName = value; }
            }

            //Чтобы упростить эту практику, придумали автосвойства
            //Следующая строка делает то же самое, что и предыдущие 5, автоматически
            public string FirstName { get; set; }

            //Возможен различные модификаторы доступа у свойств
            public string Id { get; private set; }
        }

        public class Purchase
        {
            public double Price { get; set; }
            public double Count { get; set; }

            //В этом случае никакого поля не создается, и вообще это свойство не связано с хранением данных
            //Это просто удобный синтаксический сахар, можно было бы использовать метод GetSalary() { return Price*Count; }
            //Но в шарпе принято писать так
            public double Summary
            {
                get { return Price * Count; }
            }
        }

        public class Program
        {
            static void Main()
            {
                var purchase = new Purchase { Price = 1000, Count = 5 };
                // purchase.Summary=100; // так нельзя, поскольку сеттер не определен
                Console.WriteLine(purchase.Summary);

            }
        }


    }
}
