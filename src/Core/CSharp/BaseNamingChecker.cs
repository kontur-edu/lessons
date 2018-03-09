using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using uLearn.CSharp.Validators;

namespace uLearn.CSharp
{
	public abstract class BaseNamingChecker : BaseStyleValidator
	{
		public override List<SolutionStyleError> FindErrors(SyntaxTree userSolution, SemanticModel semanticModel)
		{
			return InspectAll<BaseTypeDeclarationSyntax>(userSolution, n => InspectName(n.Identifier))
				.Concat(InspectAll<TypeParameterSyntax>(userSolution, n => InspectName(n.Identifier)))
				.Concat(InspectAll<ParameterSyntax>(userSolution, n => InspectName(n.Identifier)))
				.Concat(InspectAll<EnumMemberDeclarationSyntax>(userSolution, n => InspectName(n.Identifier)))
				.Concat(InspectAll<MethodDeclarationSyntax>(userSolution, n => InspectName(n.Identifier)))
				.Concat(InspectAll<VariableDeclaratorSyntax>(userSolution, n => InspectName(n.Identifier)))
				.ToList();
		}

		protected abstract IEnumerable<SolutionStyleError> InspectName(SyntaxToken identifier);
	}
}