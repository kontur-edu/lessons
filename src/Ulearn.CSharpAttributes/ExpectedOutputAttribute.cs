using System;

namespace uLearn
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
	public class ExpectedOutputAttribute : Attribute
	{
		public ExpectedOutputAttribute(string expectedOutput)
		{
			Output = expectedOutput;
		}

		public string Output { get; private set; }
	}
}