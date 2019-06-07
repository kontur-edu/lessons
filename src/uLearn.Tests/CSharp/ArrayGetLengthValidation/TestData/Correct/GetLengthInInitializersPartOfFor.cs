using System;

namespace uLearn.CSharp.ArrayGetLengthValidation.TestData.Correct
{
	public class GetLengthInInitializersPartOfFor
	{
		public void GetLengthInStatement()
		{
			var array = new[] {1};
			for (var i = array.GetLength(0); i >= 0; i--) ;
		}
	}
}
