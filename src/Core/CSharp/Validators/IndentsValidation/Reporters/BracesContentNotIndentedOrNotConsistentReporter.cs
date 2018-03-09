using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace uLearn.CSharp.Validators.IndentsValidation.Reporters
{
	internal static class BracesContentNotIndentedOrNotConsistentReporter
	{
		public static IEnumerable<SolutionStyleError> Report(BracesPair[] bracesPairs)
		{
			foreach (var braces in bracesPairs.Where(pair => pair.Open.GetLine() != pair.Close.GetLine()))
			{
				var childLineIndents = braces.Open.Parent.ChildNodes()
					.Select(node => node.DescendantTokens().First())
					.Where(t => braces.TokenInsideBraces(t))
					.Select(t => new Indent(t))
					.Where(i => i.IndentedTokenIsFirstAtLine)
					.ToList();
				if (!childLineIndents.Any())
					continue;
				var firstTokenOfLineWithMinimalIndent = Indent.TokenIsFirstAtLine(braces.Open)
					? braces.Open
					: braces.Open.GetFirstTokenOfCorrectOpenbraceParent();
				if (firstTokenOfLineWithMinimalIndent == default(SyntaxToken))
					continue;
				var minimalIndentAfterOpenbrace = new Indent(firstTokenOfLineWithMinimalIndent);
				var firstChild = childLineIndents.First();
				if (firstChild.LengthInSpaces <= minimalIndentAfterOpenbrace.LengthInSpaces)
					yield return new SolutionStyleError(firstChild.IndentedToken,
						$"Содержимое парных фигурных скобок ({braces}) должно иметь дополнительный отступ.");
				var badLines = childLineIndents.Where(t => t.LengthInSpaces != firstChild.LengthInSpaces);
				foreach (var badIndent in badLines)
				{
					yield return new SolutionStyleError(badIndent.IndentedToken,
						$"Содержимое парных фигурных скобок ({braces}) должно иметь одинаковый отступ.");
				}
			}
		}
	}
}