using System.Linq;

namespace uLearn.CSharp.ArrayGetLengthValidation.TestData.Correct
{
	public class GetLengthOfArrayThatUsedAsMethodArgument
	{
		public void UseArrayInMethod()
		{
			var arr = new int[2, 2];
			foreach (var number in Enumerable.Range(0, 5))
			{
				MethodThatUseArrayAsParameter(arr);
			}
		}

		public void MethodThatUseArrayAsParameter(int[,] arr)
		{
			var a = arr[0, 0];
		}
	}
}