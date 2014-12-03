namespace uLearn.Courses.BasicProgramming.Slides.U12_Inheritance
{
	[Slide("Наследование", "FD1D51EF-DDD1-4CA0-872F-3A9C89CE06E4")]
	public class S020_Inheritance
	{
		//#video u8jnHaW_FP0
		/*
		## Заметки по лекции
		*/
		class Transport
		{
			public double Velocity;
			public double KilometerPrice;
			public int Capacity;
		}

		class CombustionEngineTransport : Transport
		// это двоеточие обозначает наследование
		{
			public double EngineVolume;
			public double EnginePower;
		}

		enum ControlType
		{
			Forward,
			Backward
		}

		class Car : CombustionEngineTransport
		{
			public ControlType Control;
		}

		class Program
		{
			public static void Main()
			{
				var c = new Car();
				c.Control = ControlType.Backward; //это собственное поле класса Car
				c.EnginePower = 100; // это поле унаследовано от CombustionEngineTransport
				c.Capacity = 4; // это поле унаследовано от Transport
			}
		}
	}
}