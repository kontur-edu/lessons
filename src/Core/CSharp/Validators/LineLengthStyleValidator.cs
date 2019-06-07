using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Ulearn.Core.CSharp.Validators
{
	public class LineLengthStyleValidator : BaseStyleValidator
	{
		private readonly int maxLineLen;

		public LineLengthStyleValidator(int maxLineLen = 120)
		{
			this.maxLineLen = maxLineLen;
		}

		public override List<SolutionStyleError> FindErrors(SyntaxTree userSolution, SemanticModel semanticModel)
		{
			var text = userSolution.GetText();
			var longLines = text.Lines.Where(line => line.End - line.Start > maxLineLen).ToList();
			if (longLines.Count == 0)
				return new List<SolutionStyleError>();
			
			var position = userSolution.GetLineSpan(longLines.First().Span);

			return new List<SolutionStyleError>
			{
				new SolutionStyleError(StyleErrorType.LineLength01, position)
			};
		}
	}
}