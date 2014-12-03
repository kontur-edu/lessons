using System;

namespace uLearn.Courses.BasicProgramming.Slides.U13_Consistency
{
	[Slide("Вектор", "6011B77EB481445D982393E6A57FD757")]
	class S065_Конструктор_вектора_задача : SlideTestBase
	{
		/*
		Добавьте конструктор в класс Vector.
		
		Сделайте так, чтобы:

		* поля этого класса инициализировались в конструкторе.
		* поле Length (длина вектора), стало вычисляемым свойством.
		*/

		[HideOnSlide]
		[ExcludeFromSolution]
		public class Vector
		{
			public double X;
			public double Y;
			public double Length { get { return Math.Sqrt(X * X + Y * Y); } }

			public Vector(double x, double y)
			{
				X = x;
				Y = y;
			}

			public override string ToString()
			{
				return string.Format("({0}, {1}) with length: {2}", X, Y, Length);
			}
		}
		/*uncomment
		public class Vector
		{
			public double X;
			public double Y;
			public double Length;

			// добавьте конструктор!

			public override string ToString()
			{
				return string.Format("({0}, {1}) with length: {2}", X, Y, Length);
			}
		}
		*/
		public static void Check()
		{
			Vector vector = new Vector(3, 4);
			Console.WriteLine(vector.ToString());

			vector.X = 0;
			vector.Y = -1;
			Console.WriteLine(vector.ToString());

			vector = new Vector(9, 40);
			Console.WriteLine(vector.ToString());
	
			Console.WriteLine(new Vector(0, 0).ToString());
		}
		
		[HideOnSlide]
		[ExpectedOutput(@"
(3, 4) with length: 5
(0, -1) with length: 1
(9, 40) with length: 41
(0, 0) with length: 0
")]
		[Hint("Length должно стать свойством, у которого определен только getter")]
		[Hint("Примерно так: public double Length { get { return ... } }")]
		[Hint("Для вычисления квадратного корня используйте функцию Math.Sqrt")]
		public static void Main()
		{
			var propertyInfo = typeof(Vector).GetProperty("Length");
			if (propertyInfo == null)
				Console.WriteLine("Length должно быть свойством класса Vector");
			else if (propertyInfo.CanWrite)
				Console.WriteLine("Свойство Length должно быть только для чтения");
			else
				Check();
		}
	}
}
