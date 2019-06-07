using System;

namespace uLearn.CSharp.ArrayGetLengthValidation.TestData.Incorrect
{
	public class GetLengthInDoWhileCycle
	{
		public void GetLengthInStatement()
		{
			var arr = new int[2, 2];
			int count = 0;
			do
			{
				Console.WriteLine(count);
			} while (count++ < arr.GetLength(1));
		}

		public void GetLengthInBody()
		{
			var arr = new int[2, 2];
			int count = 0;
			do
			{
				count = arr.GetLength(1);  
				Console.WriteLine(count);
			} while (count < 2);
		}
	}
}
