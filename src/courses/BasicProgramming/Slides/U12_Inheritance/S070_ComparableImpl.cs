using System;
using uLearn;

namespace uLearn.Courses.BasicProgramming.Slides.U12_Inheritance
{
	[Slide("Реализация IComparable", "E09036C5-B739-4585-B929-487BA92DB493")]
	public class S070_ComparableImpl
	{
		//#video aOpHYGQTyAM
		/*
		## Заметки по лекции
		*/
		public class Point : IComparable
		{
			public double X;
			public double Y;

			public int CompareTo(object obj)
			{
				var point = obj as Point;
				var thisDistance = Math.Sqrt(X * X + Y * Y);
				var thatDistance = Math.Sqrt(point.X * point.X + point.Y * point.Y);
				return thisDistance.CompareTo(thatDistance);
				//или
				//if (thisDistance < thatDistance) return -1;
				//else if (thisDistance == thatDistance) return 0;
				//else return 1;
			}
		}
	}
}