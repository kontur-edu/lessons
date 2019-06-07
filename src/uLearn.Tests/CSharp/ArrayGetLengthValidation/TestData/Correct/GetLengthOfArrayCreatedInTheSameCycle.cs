using System.Linq;

namespace uLearn.CSharp.ArrayGetLengthValidation.TestData.Correct
{
	public class GetLengthOfArrayCreatedInTheSameCycle
	{
		public void A()
		{
			foreach (var number in Enumerable.Range(0, 10))
			{
				var arr = new int[number];
				var length = arr.GetLength(0);
			}
		}
	}
}