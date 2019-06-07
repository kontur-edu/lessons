using System;

namespace uLearn.CSharp.ArrayGetLengthValidation.TestData.Correct
{
	public class GetLengthWithVariable
	{
		public void GetLengthInStatement()
		{
			var arr = new int[2, 2];
			var dim = 1;
			Console.WriteLine("W");
			for (var i = 1; i < arr.GetLength(dim); i++) ;
		}
	}
}
