using System;
using uLearn;

namespace uLearn.Courses.BasicProgramming.Slides.U12_Inheritance
{
	[Slide("Интерфейсы", "D416FD8C-0F8D-49B8-9D30-02E740A7F0DE")]
	public class S050_Interfaces
	{
		//#video ODtgRp-U0a4
		/*
		## Заметки по лекции
		*/

		public static void Sort(Array array)
		{
			for (int i = array.Length - 1; i > 0; i--)
				for (int j = 1; j <= i; j++)
				{
					object element1 = array.GetValue(j - 1);
					object element2 = array.GetValue(j);
					var comparableElement1 = (IComparable)element1;
					if (comparableElement1.CompareTo(element2) < 0)
					{
						object temporary = array.GetValue(j);
						array.SetValue(array.GetValue(j - 1), j);
						array.SetValue(temporary, j - 1);
					}
				}
		}

		static void Main()
		{
			Sort(new int[] { 1, 2, 3 });
			Sort(new string[] { "A", "B", "C" });
		}
	}
}