using System;

namespace uLearn
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class HintAttribute : Attribute
	{
		public HintAttribute(string hint)
		{
			HintText = hint;
		}

		public string HintText { get; set; }
	}
}