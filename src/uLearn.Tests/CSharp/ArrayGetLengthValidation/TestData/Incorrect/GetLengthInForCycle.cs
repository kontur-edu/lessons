using System;

namespace uLearn.CSharp.ArrayGetLengthValidation.TestData.Incorrect
{
	public class GetLengthInForCycle
	{
		public void GetLengthInStatement()
		{
			var arr = new int[2, 2];
			for (var i = 1; i < arr.GetLength(1); i++);
		}

		public void GetLengthInBody()
		{
			var arr = new int[2, 2];
			for (var i = 1; i < 5; i++)
				Console.WriteLine(arr.GetLength(1));
		}
	}
}
