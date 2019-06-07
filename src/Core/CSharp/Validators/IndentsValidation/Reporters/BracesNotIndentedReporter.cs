using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Ulearn.Core.CSharp.Validators.IndentsValidation.Reporters
{
	internal static class BracesNotIndentedReporter
	{
		public static IEnumerable<SolutionStyleError> Report(BracesPair[] bracesPairs)
		{
			foreach (var braces in bracesPairs.Where(pair => pair.Open.GetLine() != pair.Close.GetLine() &&
															Indent.TokenIsFirstAtLine(pair.Open)))
			{
				var correctOpenbraceParent = braces.Open.GetFirstTokenOfCorrectOpenbraceParent();
				if (correctOpenbraceParent == default(SyntaxToken))
					continue;
				var parentLineIndent = new Indent(correctOpenbraceParent);
				var openbraceLineIndent = new Indent(braces.Open);
				if (openbraceLineIndent.LengthInSpaces < parentLineIndent.LengthInSpaces)
					yield return new SolutionStyleError(StyleErrorType.Indents04, braces.Open, braces);
			}
		}
	}
}