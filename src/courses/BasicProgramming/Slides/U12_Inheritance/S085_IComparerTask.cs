using System;
using System.Collections;

namespace uLearn.Courses.BasicProgramming.Slides.U12_Inheritance
{
	[Slide("По часовой стрелке", "{E147BFF3-7BC6-454B-A6E4-BFE9E9DE6D41}")]
	public class S016_ClockwiseComparer : SlideTestBase
	{
		/*
		А как насчет того, чтобы отсортировать точки в порядке следования против часовой стрелки,
		считая первой ту, что находится на 3:00?
		*/

		[ExpectedOutput(
			@"1 0
0.01 1
0 1
-1 0
0 -1")]
		[Hint("Используйте функцию Math.Atan2(y, x) возвращающую угол радиус-вектора (x, y) в диапазоне (-PI..PI]")]
		private static void Main()
		{
			var array = new[]
			{
				new Point { X = 1, Y = 0 },
				new Point { X = -1, Y = 0 },
				new Point { X = 0, Y = 1 },
				new Point { X = 0, Y = -1 },
				new Point { X = 0.01, Y = 1 }
			};
			Array.Sort(array, new ClockwiseComparer());
			foreach (Point e in array)
				Console.WriteLine("{0} {1}", e.X, e.Y);
		}

		private class Point
		{
			public double X;
			public double Y;
		}

		[ExcludeFromSolution]
		[HideOnSlide]
		public class ClockwiseComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				return GetAngle(x as Point).CompareTo(GetAngle(y as Point));
			}

			private double GetAngle(Point p)
			{
				var angle = Math.Atan2(p.Y, p.X);
				if (angle < 0)
					angle += 2 * Math.PI;
				return angle;
			}
		}

		/*uncomment
		public class ClockwiseComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				
			}
		}
		*/
	}
}