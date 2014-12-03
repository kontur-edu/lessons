using System;

namespace uLearn.Courses.BasicProgramming.Slides.U12_Inheritance
{
	[Slide("Метод ToString", "{910105F8-F895-4608-9134-8B8D42D68E41}")]
	public class S016_ToString : SlideTestBase
	{
		/*
		Метод `ToString` в реальной практике переопределяется довольно часто,
		поэтому давайте научимся это делать.
		
		Создайте класс `Triangle` и переопределите в нем метод `ToString`.
		*/

		[ExpectedOutput(@"(0 0) (1 2) (3 2)")]
		static void Main()
		{
			var triangle = new Triangle
			{
				A = new Point { X = 0, Y = 0 },
				B = new Point { X = 1, Y = 2 },
				C = new Point { X = 3, Y = 2 }
			};
			Console.WriteLine(triangle.ToString());
		}
		

		class Point
		{
			public double X;
			public double Y;
			public override string ToString()
			{
				return string.Format("{0} {1}", X, Y);
			}
		}

		[ExcludeFromSolution]
		[HideOnSlide]
		class Triangle
		{
			public Point A;
			public Point B;
			public Point C;
			public override string ToString()
			{
				return string.Format("({0}) ({1}) ({2})", A, B, C);
			}
		}

	}
}