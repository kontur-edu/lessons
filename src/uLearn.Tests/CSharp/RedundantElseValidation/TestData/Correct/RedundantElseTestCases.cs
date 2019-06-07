using System;

namespace uLearn.CSharp.RedundantElseValidation.TestData.Correct
{
	public class RedundantElseTestCases
	{
		public bool MoreThanOneStatementInIfStatement()
		{
			if (true)
			{
				var a = 1;
				return true;
			}
			else
			{
				var a = 2;
				return false;
			}
		}

		public bool OnluOneStatementInElseClause()
		{
			if (true)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}