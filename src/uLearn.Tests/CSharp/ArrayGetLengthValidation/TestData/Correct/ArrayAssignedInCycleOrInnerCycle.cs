namespace uLearn.CSharp.ArrayGetLengthValidation.TestData.Correct
{
	public class ArrayAssignedInCycleOrInnerCycle
	{
		public void ArrayAssignedInCycle()
		{
			var a = new[] { 1 };
			for (var i = 1; i < 485; i++)
			{
				var b = a.GetLength(0);
				a = new[] { 1 };
			}
		}
		
		public void ArrayAssignedInInnerCycle()
		{
			var a = new[] { 1 };
			for (var i = 1; i < 485; i++)
			{
				var b = a.GetLength(0);
				for (var j = 1; j < 2; j++)
				{
					a = new[] { 1 };
				}
			}
		}
	}
}