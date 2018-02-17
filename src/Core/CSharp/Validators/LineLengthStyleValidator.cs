using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace uLearn.CSharp
{
	public class LineLengthStyleValidator : BaseStyleValidator
	{
		private readonly int maxLineLen;

		public LineLengthStyleValidator(int maxLineLen = 120)
		{
			this.maxLineLen = maxLineLen;
		}

		protected override IEnumerable<string> ReportAllErrors(SyntaxTree userSolution, SemanticModel semanticModel)
		{
			var text = userSolution.GetText();
			var longLines = text.Lines.Where(line => line.End - line.Start > maxLineLen).ToList();
			if (longLines.Count == 0)
				yield break;
			var position = userSolution.GetLineSpan(longLines[0].Span);
			yield return Report(
				position,
				"Слишком длинная строка. Не заставляйте людей использовать горизонтальный скролл");
		}
	}
}