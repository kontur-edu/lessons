using System;

namespace uLearn.CSharp.ArrayGetLengthValidation.TestData.Incorrect
{
	public class GetLengthInWhileCycle
	{
		public void GetLengthInStatement()
		{
			var arr = new int[2, 2];
			int count = 0;
			while (count++ < arr.GetLength(1))
			{
				Console.WriteLine(count);
			}
		}

		public void GetLengthInBody()
		{
			var arr = new int[2, 2];
			int count = 0;
			while (count < 2)
			{
				count = arr.GetLength(1);  
				Console.WriteLine(count);
			}
		}
	}
}
