using System;
using System.Globalization;

namespace uLearn.Courses.BasicProgramming.Slides.U13_Consistency
{
	[Slide("Дробь", "DEEEF0D75D5C481A9A07480641705EFE")]
	class S075_Дробь_задача : SlideTestBase
	{
		/*
		Проведите рефакторинг класса `Ratio`. В результате:

		* `Numerator`, `Denominator` и `Value` должны остаться полями класса `Ratio`.
		* После создания объекта `Ratio` не должно быть возможности его изменить, то есть поменять поля `Numerator`, `Denominator` или `Value`.
		* После создания объекта `Ratio` знаменатель всегда должен быть больше нуля.
		Бросайте исключение `ArgumentException` при попытке установить неверное значение знаменателя.
		*/
		[HideOnSlide]
		[ExcludeFromSolution]
		public class Ratio
		{
			public readonly int Numerator; //числитель
			public readonly int Denominator; //знаменатель
			public readonly double Value; //значение дроби Numerator / Denominator

			public Ratio(int num, int den)
			{
				if (den <= 0) throw new ArgumentException();
				Numerator = num;
				Denominator = den;
				Value = (double)num / den;
			}
		}
		/*uncomment
		public class Ratio
		{
			public Ratio(int num, int den)
			{
				...
			}

			public int Numerator; //числитель
			public int Denominator; //знаменатель
			public double Value; //значение дроби Numerator / Denominator
		}
		*/
		public static void Check(int num, int den)
		{
			var ratio = new Ratio(num, den);
			Console.WriteLine("{0}/{1} = {2}",
				ratio.Numerator, ratio.Denominator, 
				ratio.Value.ToString(CultureInfo.InvariantCulture));
		}

		[HideOnSlide]
		[ExpectedOutput(@"
1/2 = 0.5
-10/5 = -2
ArgumentException
ArgumentException
")]
		[Hint("Вам поможет ключевое слово readonly")]
		[Hint("Не забудьте проинициализировать Value в конструкторе")]
		public static void Main()
		{
			var numField = typeof(Ratio).GetField("Numerator");
			var denField = typeof(Ratio).GetField("Denominator");
			if (numField == null || denField == null)
				Console.WriteLine("Numerator, Denominator и Value должны остаться полями класса Ratio");
			else if (!numField.IsInitOnly || !denField.IsInitOnly)
				Console.WriteLine("После создания объекта Ratio не должно быть возможности его изменить, то есть поменять поля Numerator, Denominator или Value");
			else
			{
				Check(1, 2);
				Check(-10, 5);
				try
				{
					Check(10, 0);
				}
				catch (ArgumentException)
				{
					Console.WriteLine("ArgumentException");
				}
				try
				{
					Check(10, -5);
				}
				catch (ArgumentException)
				{
					Console.WriteLine("ArgumentException");
				}
			}
		}
	}
}
