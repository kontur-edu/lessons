namespace uLearn.CSharp.RefArgumentsValidation.TestData.Correct
{
	internal class RefArgumentsValidationTestDataClass
	{
		public void DoWork(ref bool foo, out int workResult)
		{
			workResult = 0;
		}

		public void DoWork(ref int foo, out int workResult, ref float foo2)
		{
			workResult = 0;
		}
	}
}
