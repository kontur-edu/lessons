using System.Collections.Generic;
using System.Linq;

namespace Ulearn.Core.CSharp.Validators.IndentsValidation.Reporters
{
	internal static class CloseBraceHasCodeOnSameLineReporter
	{
		public static IEnumerable<SolutionStyleError> Report(BracesPair[] bracesPairs)
		{
			foreach (var braces in bracesPairs.Where(pair => IndentsSyntaxExtensions.GetLine(pair.Open) != IndentsSyntaxExtensions.GetLine(pair.Close)))
			{
				var openBraceIndent = new Indent(braces.Open);
				var closeBraceIndent = new Indent(braces.Close);
				if (openBraceIndent.IndentedTokenIsFirstAtLine && !closeBraceIndent.IndentedTokenIsFirstAtLine)
				{
					yield return new SolutionStyleError(StyleErrorType.Indents05, braces.Close);
				}
			}
		}
	}
}