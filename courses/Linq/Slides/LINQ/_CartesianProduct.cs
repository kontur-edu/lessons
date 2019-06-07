using System;
using System.Collections.Generic;
using System.Linq;
using uLearn.CSharp;

namespace uLearn.Courses.Linq.Slides.LINQ.LINQ.LINQ
{
	public class S060_CartesianProduct : SlideTestBase
	{
		/*

		Одно из не совсем очевидных применений `SelectMany` — это вычисление декартова произведения двух множеств.
		Опробуйте этот трюк на следующей задаче:

		Вычислить координаты всех восьми соседей заданной точки.

		Можете полагаться на то, что в классе `Point` переопределен метод `Equals`, 
		сравнивающий точки покоординатно.

		*/

		public static void Main()
		{
			CheckForPoint(new Point(1, 2));
			CheckForPoint(new Point(0, 0));
		}

		private static void CheckForPoint(Point point)
		{
			Console.WriteLine("Neighbours of ({0})", point);
			var lines = GetNeighbours(point).OrderBy(p => p.X).ThenBy(p => p.Y).Select(p => p.ToString());
			foreach (var line in lines)
				Console.WriteLine(line);
		}

		public static IEnumerable<Point> GetNeighbours(Point p)
		{
			int[] d = { -1, 0, 1 };
			return d
				.SelectMany(dx => d.Select(dy => new Point(p.X + dx, p.Y + dy)))
				.Where(n => !n.Equals(p));
			/*uncomment
			int[] d = {-1, 0, 1}; // используйте подсказку, если не понимаете зачем тут этот массив :)
			return ...
			*/
		}

		public static bool IsRightCartesian(IEnumerable<Point> neighbors)
		{
			var correctAnswer = new HashSet<Point>()
			{
				new Point(0, 1),
				new Point(0, 2),
				new Point(0, 3),
				new Point(1, 1),
				new Point(1, 3),
				new Point(2, 1),
				new Point(2, 2),
				new Point(2, 3)
			};
			return neighbors.All(correctAnswer.Contains);
		}

		public class Point
		{
			public Point(int x, int y)
			{
				X = x;
				Y = y;
			}
			public int X, Y;

			public override string ToString()
			{
				return string.Format("{0} {1}", X, Y);
			}

			protected bool Equals(Point other)
			{
				return X == other.X && Y == other.Y;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((Point)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return (X * 397) ^ Y;
				}
			}
		}

	}
}