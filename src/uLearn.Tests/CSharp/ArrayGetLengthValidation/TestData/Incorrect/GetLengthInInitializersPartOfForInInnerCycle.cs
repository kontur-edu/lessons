using System;

namespace uLearn.CSharp.ArrayGetLengthValidation.TestData.Correct
{
	public class GetLengthInInitializersPartOfFor
	{
		public void GetLengthInStatement()
		{
			var array = new[] {1};
			for(var i = 0; i < 3; i++)
				for (var j = array.GetLength(0); j >= 0; j--) ;
		}
	}
}
