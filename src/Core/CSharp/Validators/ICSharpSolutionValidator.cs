using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp
{
	public interface ICSharpSolutionValidator
	{
		string FindError(SyntaxTree userSolution, SemanticModel semanticModel);
	}

	public class IsStaticMethodValidator : ICSharpSolutionValidator, IStrictValidator
	{
		public const string ShouldBeMethod = "Решение должно быть корректным определением статического метода";
		public const string ShouldBeSingleMethod = "Решение должно состоять ровно из одного метода";

		public string FindError(SyntaxTree userSolution, SemanticModel semanticModel)
		{
			var cu = userSolution.GetRoot() as CompilationUnitSyntax;
			if (cu == null)
				return ShouldBeMethod;
			if (cu.Members.Count > 1)
				return ShouldBeSingleMethod;
			var method = cu.Members[0] as MethodDeclarationSyntax;
			if (method == null)
				return ShouldBeMethod;
			return FindError(method);
		}

		protected virtual string FindError(MethodDeclarationSyntax method)
		{
			return null;
		}
	}

	public class SingleStatementMethodValidator : IsStaticMethodValidator
	{
		public const string ShouldBeSingleMethodMessage = "Решение этой задачи должно быть в одно выражение 'return ...'";

		protected override string FindError(MethodDeclarationSyntax method)
		{
			var statements = method.Body.Statements;
			return statements.Count != 1
					|| !(statements.Single() is ReturnStatementSyntax)
				? ShouldBeSingleMethodMessage : null;
		}
	}
}