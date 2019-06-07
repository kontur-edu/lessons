using System.Collections.Generic;

namespace uLearn.CSharp.ArrayGetLengthValidation.TestData.Incorrect
{
	public class GetLengthInInnerCycle
	{
		public void GetLengthInInnerCycleWhenArrayCreatedInOuterCycle()
		{
			foreach(var number in new List<int> {1})
			{
				var a = new[] { 1 };
				for (var i = 1; i < 485; i++)
				{
					var b = a.GetLength(0);
				}
			}
		}
	}
}