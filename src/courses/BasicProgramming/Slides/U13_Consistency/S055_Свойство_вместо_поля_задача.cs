using System;

namespace uLearn.Courses.BasicProgramming.Slides.U13_Consistency
{
	[Slide("Свойство вместо поля", "3ADAD7BAA46249D586777D1F8C85AF33")]
	internal class S055_Свойство_вместо_поля_задача : SlideTestBase
	{
		/*
		Превратите поле Name в свойство самостоятельно. Это совсем не сложно!
		*/

		public static void Check()
		{
			var book = new Book();
			book.Title = "Structure and interpretation of computer programs";
			Console.WriteLine(book.Title);
		}

		[HideOnSlide]
		[ExpectedOutput("Structure and interpretation of computer programs")]
		[HideExpectedOutputOnError]
		[Hint("Проще всего будет воспользоваться сокращенным синтаксисом для определения свойства")]
		[Hint("Сокращенный синтаксис для целочисленного свойства X выглядит так: public int X { get; set; }")]
		public static void Main()
		{
			var property = typeof(Book).GetProperty("Title");
			if (property == null)
				Console.WriteLine("В классе Book Title должно стать свойством");
			else
				Check();
		}

		[HideOnSlide]
		[ExcludeFromSolution]
		public class Book
		{
			public string Title { get; set; }
		}

		/*uncomment
		public class Book
		{
			public string Title;
		}
		*/
	}
}
