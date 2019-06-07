using System.Collections.Generic;
using System.Linq;
using Ulearn.Core.CSharp;

namespace Ulearn.Core
{
	public class SolutionBuildResult
	{
		public SolutionBuildResult(string sourceCode, string errorMessage = null, List<SolutionStyleError> styleErrors = null)
		{
			SourceCode = sourceCode;			
			ErrorMessage = errorMessage;
			StyleErrors = styleErrors ?? new List<SolutionStyleError>();
		}

		public override string ToString()
		{
			return $"ErrorMessage: {ErrorMessage ?? ""}, StyleErrors: {(HasStyleErrors ? string.Join(", ", StyleErrors) : "<none>")}, SourceCode:\r\n{SourceCode}";
		}

		public bool HasErrors => ErrorMessage != null;
		public bool HasStyleErrors => StyleErrors?.Any() ?? false;

		public readonly string ErrorMessage;
		public readonly List<SolutionStyleError> StyleErrors;
		public readonly string SourceCode;
	}
}