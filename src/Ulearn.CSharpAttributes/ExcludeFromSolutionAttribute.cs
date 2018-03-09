using System;

namespace uLearn
{
	public class ExcludeFromSolutionAttribute : Attribute
	{
		public ExcludeFromSolutionAttribute(bool isSolutionPart = true)
		{
			IsSolutionPart = isSolutionPart;
		}

		public bool IsSolutionPart { get; set; }
	}
}