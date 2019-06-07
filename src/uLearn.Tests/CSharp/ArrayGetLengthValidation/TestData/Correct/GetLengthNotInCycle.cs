using System;

namespace uLearn.CSharp.ArrayGetLengthValidation.TestData.Correct
{
	public class GetLengthNotInCycle
	{
		public void A()
		{
			var arr = new int[2, 2];
			var dim = arr.GetLength(1);
			Console.WriteLine(dim);
		}
	}
}
