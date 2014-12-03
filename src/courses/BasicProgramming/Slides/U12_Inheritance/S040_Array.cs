using System;
using uLearn;

namespace uLearn.Courses.BasicProgramming.Slides.U12_Inheritance
{
	[Slide("Класс Array", "9E4799D4-69BC-466F-8441-B97534286E34")]
	public class S040_Array
	{
		//#video RGfBuK16cFM
		/*
		## Заметки по лекции
		*/


		static void Main()
		{
			var strings = new string[] { "B", "A", "C" };
			strings.Swap(0, 1);

			var ints = new int[] { 1, 3, 2 };
			ints.Swap(1, 2);
		}
	}
	
	public static class ArrayExtensions
	{
		public static void Swap(this Array array, int i, int j)
		{
			object temporary = array.GetValue(i);
			array.SetValue(array.GetValue(i), j);
			array.SetValue(temporary, j);
		}
	}
}