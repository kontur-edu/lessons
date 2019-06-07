namespace uLearn.CSharp.ArrayGetLengthValidation.TestData.Incorrect
{
	public class AssignToArrayElement
	{
		public static void AssignToArrayElementNotToArray()
		{
			var arr = new int [1, 1];
			for (var i = 0; i < arr.GetLength(0); i++)
				for (var j = 0; j < arr.GetLength(1); j++)
					arr[i, j] = 1;
		}
	}
}