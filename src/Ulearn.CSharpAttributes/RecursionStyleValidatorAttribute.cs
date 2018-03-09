using System;

namespace uLearn
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class RecursionStyleValidatorAttribute : Attribute
	{
		public RecursionStyleValidatorAttribute(bool requireRecursion)
		{
			RequireRecursion = requireRecursion;
		}

		public bool RequireRecursion { get; }
	}
}