using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ulearn.Core.CSharp.Validators
{
	public class BlockLengthStyleValidator : BaseStyleValidator
	{
		private readonly int maxLen;

		public BlockLengthStyleValidator(int maxLen = 25)
		{
			this.maxLen = maxLen;
		}

		public override List<SolutionStyleError> FindErrors(SyntaxTree userSolution, SemanticModel semanticModel)
		{
			return InspectAll<BlockSyntax>(userSolution, Inspect).ToList();
		}

		private IEnumerable<SolutionStyleError> Inspect(BlockSyntax block)
		{
			var startPosition = GetSpan(block.OpenBraceToken).StartLinePosition;
			var endPosition = GetSpan(block.CloseBraceToken).EndLinePosition;
			if (endPosition.Line - startPosition.Line >= maxLen)
				yield return new SolutionStyleError(StyleErrorType.BlockLength01, new FileLinePositionSpan("", startPosition, endPosition));
		}
	}
}