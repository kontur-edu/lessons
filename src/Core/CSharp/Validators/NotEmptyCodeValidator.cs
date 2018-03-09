using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp.Validators
{
	public class NotEmptyCodeValidator : BaseStyleValidator, IStrictValidator
	{
		public override List<SolutionStyleError> FindErrors(SyntaxTree userSolution, SemanticModel semanticModel)
		{
			var hasCode = userSolution.GetRoot().DescendantNodes().Any(n => n is StatementSyntax || n is MemberDeclarationSyntax);
			if (!hasCode)
				return new List<SolutionStyleError>
				{
					new SolutionStyleError(userSolution.GetRoot(), "Пустое решение?!")
				};
			
			return new List<SolutionStyleError>();
		}
	}
}