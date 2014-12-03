using System;
using uLearn;

namespace uLearn.Courses.BasicProgramming.Slides.U12_Inheritance
{
	[Slide("Виртуальные методы", "C74B59D5-CEF2-4687-B6FA-1F4D49389043")]
	public class S100_VirtualMethods
	{
		//#video JjJ6U1X4sPk
		/*
		## Заметки по лекции
		*/

		class Point
		{
			public int X;
			public int Y;

			public override string ToString()
			{
				return string.Format("{0},{1}", X, Y);
			}
		}

		public class Program
		{
			static void Main()
			{
				var point = new Point { X = 1, Y = 3 };
				Console.WriteLine(point);
			}
		}
	}
}