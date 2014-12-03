using System;

namespace uLearn.Courses.BasicProgramming.Slides.U12_Inheritance
{
	[Slide("Сравнение книг", "{BD1F8054-A372-4EA4-9F5A-2EEE97CD2B71}")]
	public class S016_CompareBooks : SlideTestBase
	{
		/*
		Вы пишете учетную систему для книжного магазина, и вам необходимо научиться 
		сравнивать между собой записи о книгах, чтобы продавцам было удобно искать нужную.
		
		Каждая книга имеет название `Title` и номер тематического раздела `Theme`.
		
		Реализуйте интерфейс IComparable у класса Book.
		Логику сравнения восстановите по тестам, которые вы увидите при первом запуске.
		*/

		[ExpectedOutput(
@"
5 B = 5 B
1 A < 1 C
3 B > 2 B
1 B < 2 A
9 A > 1 Z")]
		[HideOnSlide]
		static void Main()
		{
			Check(5, "B", 5, "B");
			Check(1, "A", 1, "C");
			Check(3, "B", 2, "B");
			Check(1, "B", 2, "A");
			Check(9, "A", 1, "Z");
		}

		[HideOnSlide]
		static void Check(int theme1, string title1, int theme2, string title2)
		{
			var book1 = new Book { Theme = theme1, Title = title1 };
			var book2 = new Book { Theme = theme2, Title = title2 };
			// ReSharper disable once RedundantCast
			var comparison = ((IComparable)book1).CompareTo(book2);
			string sign = "=";
			if (comparison < 0) sign = "<";
			if (comparison > 0) sign = ">";

			Console.WriteLine("{0} {1} {2} {3} {4}",
				book1.Theme,
				book1.Title,
				sign,
				book2.Theme,
				book2.Title);
		}


		[HideOnSlide]
		[ExcludeFromSolution]
		class Book : IComparable
		{
			public string Title;
			public int Theme;

			public int CompareTo(object obj)
			{
				var book = (Book)obj;
				var themeCmp = Theme.CompareTo(book.Theme);
				if (themeCmp != 0) return themeCmp;
				return Title.CompareTo(book.Title);
			}
		}
		/*uncomment
		class Book
		{
			public string Title;
			public int Theme;
		}	
		*/
	}
}