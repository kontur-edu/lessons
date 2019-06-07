using System;

namespace uLearn.CSharp.RedundantElseValidation.TestData.Incorrect
{
	public class RedundantElseTestCases
	{
		public bool ReturnStatementInIf()
		{
			if (true)
				return true;
			else
			{
				var a = 1;
				return false;
			}
		}

		public bool ThrowStatementInIf()
		{
			if (true)
				throw new Exception();
			else
			{
				var a = 1;
				return false;
			}
		}
	}
}