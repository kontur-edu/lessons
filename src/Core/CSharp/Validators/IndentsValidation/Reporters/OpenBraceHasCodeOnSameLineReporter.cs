using System.Collections.Generic;
using System.Linq;

namespace uLearn.CSharp.Validators.IndentsValidation.Reporters
{
	internal static class OpenBraceHasCodeOnSameLineReporter
	{
		public static IEnumerable<SolutionStyleError> Report(BracesPair[] bracesPairs)
		{
			foreach (var braces in bracesPairs.Where(pair => pair.Open.GetLine() != pair.Close.GetLine()))
			{
				var openBraceHasCodeOnSameLine = braces.Open.Parent.ChildNodes()
					.Select(node => node.GetFirstToken())
					.Any(t => braces.TokenInsideBraces(t) && t.GetLine() == braces.Open.GetLine());
				if (openBraceHasCodeOnSameLine)
					yield return new SolutionStyleError(braces.Open, "После открывающей фигурной скобки на той же строке не должно быть кода.");
			}
		}
	}
}