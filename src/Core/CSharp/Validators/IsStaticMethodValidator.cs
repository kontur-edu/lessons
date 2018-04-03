using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp.Validators
{
	public class IsStaticMethodValidator : ICSharpSolutionValidator, IStrictValidator
	{
		public List<SolutionStyleError> FindErrors(SyntaxTree userSolution, SemanticModel semanticModel)
		{
			var error = FindSingleError(userSolution);
			if (error == null)
				return new List<SolutionStyleError>();

			return new List<SolutionStyleError> { error };
		}

		private SolutionStyleError FindSingleError(SyntaxTree userSolution)
		{
			var cu = userSolution.GetRoot() as CompilationUnitSyntax;
			if (cu == null)
				return new SolutionStyleError(StyleErrorType.Static01, userSolution.GetRoot());
			if (cu.Members.Count > 1)
				return new SolutionStyleError(StyleErrorType.Static02, cu.Members[1]);
			var method = cu.Members[0] as MethodDeclarationSyntax;
			if (method == null)
				return new SolutionStyleError(StyleErrorType.Static01, cu.Members[0]);
			return FindErrorInMethodDeclaration(method);
		}

		protected virtual SolutionStyleError FindErrorInMethodDeclaration(MethodDeclarationSyntax method)
		{
			return null;
		}
	}
}