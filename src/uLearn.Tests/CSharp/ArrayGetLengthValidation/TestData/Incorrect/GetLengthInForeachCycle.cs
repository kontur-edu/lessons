using System.Linq;

namespace uLearn.CSharp.ArrayGetLengthValidation.TestData.Incorrect
{
	public class GetLengthInForeachCycle
	{
		public void GetLengthInBody()
		{
			var arr = new int[2, 2];
			foreach (var number in Enumerable.Range(0, 5))
			{
				var a = arr.GetLength(1);
			}
		}
	}
}
