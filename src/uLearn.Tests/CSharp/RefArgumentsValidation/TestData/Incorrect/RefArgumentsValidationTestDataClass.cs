namespace uLearn.CSharp.RefArgumentsValidation.TestData.Incorrect
{
	internal class RefArgumentsValidationTestDataClass
	{
		public void DoWork(ref string foo, out int workResult)
		{
			workResult = 0;
		}

		public void DoWork(ref string foo, out int workResult, ref string foo2)
		{
			workResult = 0;
		}

		public void DoWork(ref RefArgumentsValidationTestDataClass foo)
		{
		}

		public void DoWork(ref int foo, ref string foo2)
		{
		}
	}
}