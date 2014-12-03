using System;
using uLearn.CSharp;

namespace uLearn.Courses.BasicProgramming.Slides.U11_OOP
{
	[Slide("Сокращенный синтаксис", "{AECDC34E-C852-4F8B-9BCE-81A093B5B1EE}")]
	public class S017_ShortSyntax : SlideTestBase
	{
		/*
		Ваша команда пишет программу с оконным интерфейсом, и вам надо реализовать инициализацию меню.

		Для каждого пункта меню указывается название, горячая клавиша 
		(далее указана в скобках) и список подменю (null, если подменю нет).

		На верхнем уровне должно находится два пункта: File (F) и Edit (E). 

		Меню File должно содержать команды New (N), Save (S).

		Меню Edit (E) должно содержать команды Copy (C) и Paste (V).

		Решите задачу с использованием сокращенного синтаксиса создания классов в одно выражение.
		Используйте переводы строк и отступы, чтобы сделать код более читаемым.

		*/

		public class MenuItem
		{
			public string Caption;
			public string HotKey;
			public MenuItem[] Items;
		}

		[ExpectedOutput("")]
		[HideOnSlide]
		[HideExpectedOutputOnError]
		public static void Main()
		{
			MenuItem[] items = GenerateMenu();
			MenuItem[] expected = GenerateMenuRight();
			if (items == null)
				Console.WriteLine("No menu is generated");
			else if (items.Length != expected.Length)
				Console.Write("Wrong menu items count in top-level menu");
			else
				for (int i = 0; i < items.Length; i++)
					Check(items[i], expected[i]);
		}

		[HideOnSlide]
		public static void Check(MenuItem item, MenuItem expected)
		{
			if (item.Caption != expected.Caption)
			{
				Console.WriteLine("Wrong caption for '{0}'", expected.Caption);
				return;
			}
			if (item.HotKey != expected.HotKey)
			{
				Console.WriteLine("Wrong hot key for '{0}'", expected.Caption);
				return;
			}

			if (expected.Items == null && item.Items != null)
			{
				Console.WriteLine("Menu items are not expected for '{0}'", expected.Caption);
				return;
			}

			if (expected.Items != null && item.Items == null)
			{
				Console.Write("Menu items are expected for '{0}'", expected.Caption);
				return;
			}

			if (item.Items == null)
				return;
			if (item.Items.Length != expected.Items.Length)
			{
				Console.Write("Wrong number of menu items in '{0}'", expected.Caption);
				return;
			}
			for (int i = 0; i < item.Items.Length; i++)
				Check(item.Items[i], expected.Items[i]);
		}

		[HideOnSlide]
		public static MenuItem[] GenerateMenuRight()
		{
			return new[]
			{
				new MenuItem
				{
					Caption = "File", HotKey = "F", Items = new[]
					{
						new MenuItem { Caption = "New", HotKey = "N" },
						new MenuItem { Caption = "Save", HotKey = "S" }
					}
				},
				new MenuItem
				{
					Caption = "Edit", HotKey = "E", Items = new[]
					{
						new MenuItem { Caption = "Copy", HotKey = "C" },
						new MenuItem { Caption = "Paste", HotKey = "V" }
					}
				}
			};
		}

		[Exercise]
		[SingleStatementMethod]
		public static MenuItem[] GenerateMenu()
		{
			return GenerateMenuRight();
			/*uncomment
			return new[] {
				...
			}
			*/
		}
	}
}