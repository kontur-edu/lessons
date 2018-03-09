using System;

namespace uLearn.CSharp.ArrayGetLengthValidation.TestData.Incorrect
{
	public class GetLengthInMethodCall
	{
		public void A()
		{
			var arr = new int[] { 1 };
			for (var i = 0; i < 2; i++)
			{
				Console.WriteLine(arr.GetLength(0));
			}
		}
	}
}