using System.Collections.Generic;
using uLearn.CSharp;

namespace uLearn
{
	public class NullValidator : ISolutionValidator
	{
		public string FindFullSourceError(string userCode)
		{
			return null;
		}

		public string FindSyntaxError(string solution)
		{
			return null;
		}

		public List<SolutionStyleError> FindValidatorErrors(string userCode, string solution)
		{
			return null;
		}

		public string FindStrictValidatorErrors(string userCode, string solution)
		{
			return null;
		}
	}
}