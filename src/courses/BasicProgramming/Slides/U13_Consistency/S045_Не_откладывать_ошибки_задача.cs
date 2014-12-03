using System;

namespace uLearn.Courses.BasicProgramming.Slides.U13_Consistency
{
	[Slide("Не откладывать ошибки", "9EB0E353BB504AE09064E8BB86DD33DE")]
	internal class S045_Не_откладывать_ошибки_задача : SlideTestBase
	{
		/*
		Если в свойстве Name окажется null, то с ошибкой завершится метод FormatStudent.
		Чтобы предотвратить отложенную ошибку, сделайте так, чтобы свойству Name нельзя было присвоить null.
		При попытке это сделать бросайте исключение оператором `throw new ArgumentException();`
		*/

		[HideOnSlide]
		[ExpectedOutput(@"
student VIKTOR
student VASILIY
student FEDOR
ArgumentException")]
		[Hint("Добавьте в setter проверку на null и выбрасывание исключения")]
		public static void Main()
		{
			for (int i = 0; i < studentsCount; i++)
				try
				{
					WriteStudent();
				}
				catch (ArgumentException)
				{
					Console.WriteLine("ArgumentException");
				}
		}

		private static void WriteStudent()
		{
			// ReadName считает неизвестно откуда имя очередного студента
			var student = new Student { Name = ReadName() };
			Console.WriteLine("student " + FormatStudent(student));
		}

		//Делает только первую букву имени заглавной
		private static string FormatStudent(Student student)
		{
			return student.Name.ToUpper();
		}

		[HideOnSlide]
		private const int studentsCount = 4;
		[HideOnSlide]
		private static int studentIndex = 0;

		[HideOnSlide]
		private static string ReadName()
		{
			return new[]{"Viktor", "VASILIY", "fedor", null}[(studentIndex++) % studentsCount];
		}

		[HideOnSlide]
		[ExcludeFromSolution]
		public class Student
		{
			private string name;
			public string Name { 
				get { return name; }
				set
				{
					if (value == null) throw new ArgumentException();
					name = value;
				} 
			}
		}

		/*uncomment
		public class Student
		{
			private string name;
			public string Name { 
				get { return name; } 
				set { name = value; } 
			}
		}
		*/
	}
}
