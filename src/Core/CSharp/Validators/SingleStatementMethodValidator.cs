using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp.Validators
{
	public class SingleStatementMethodValidator : IsStaticMethodValidator
	{
		protected override SolutionStyleError FindErrorInMethodDeclaration(MethodDeclarationSyntax method)
		{
			var statements = method.Body.Statements;
			var hasError = statements.Count != 1 || !(statements.Single() is ReturnStatementSyntax);
			if (hasError)
				return new SolutionStyleError(StyleErrorType.Single01, method.Body);
			return null;
		}
	}
}