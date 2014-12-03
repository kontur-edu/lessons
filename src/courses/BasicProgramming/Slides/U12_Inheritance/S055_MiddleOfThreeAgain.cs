using System;
using System.Linq;
using uLearn.CSharp;

namespace uLearn.Courses.BasicProgramming.Slides.U12_Inheritance
{
	[Slide("Снова среднее трех", "{3DFF4D01-45D2-4F80-889D-75ED651B31C2}")]
	public class S055_MiddleOfThreeAgain : SlideTestBase
	{
		/*
		Помните задачу "Среднее трех"? Пришло время повторить ее для общего случая!       
		*/
		[ExpectedOutput(
@"4
2
5
B
3.12")]
		[Hint("Аргументами метода должны быть IComparable")]
		[Hint("И возвращаемым значением тоже")]
		public static void Main()
		{
			Console.WriteLine(MiddleOfThree(2, 5, 4));
			Console.WriteLine(MiddleOfThree(3, 1, 2));
			Console.WriteLine(MiddleOfThree(3, 5, 9));
			Console.WriteLine(MiddleOfThree("B", "Z", "A"));
			Console.WriteLine(MiddleOfThree(3.45, 2.67, 3.12));
		}


		[ExcludeFromSolution]
		[HideOnSlide]
		static IComparable MiddleOfThree(IComparable a, IComparable b, IComparable c)
		{
			if (a.CompareTo(b) < 0)
			{
				if (c.CompareTo(a) < 0) return a;
				if (b.CompareTo(c) < 0) return b;
				return c;
			}
			else
			{
				if (c.CompareTo(b) < 0) return b;
				if (a.CompareTo(c) < 0) return a;
				return c;
			}
		}
		/*uncomment
		static ... MiddleOfThree(... a, ... b, ... c)
		{
			...
		}

		*/

	}
}