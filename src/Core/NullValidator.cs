using System.Collections.Generic;
using Ulearn.Core.CSharp;

namespace Ulearn.Core
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
			return new List<SolutionStyleError>();
		}

		public string FindStrictValidatorErrors(string userCode, string solution)
		{
			return null;
		}
	}
}