using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp.Validators
{
	public interface ICSharpSolutionValidator
	{
		[NotNull]
		List<SolutionStyleError> FindErrors(SyntaxTree userSolution, SemanticModel semanticModel);
	}

	public class IsStaticMethodValidator : ICSharpSolutionValidator, IStrictValidator
	{
		public const string ShouldBeMethod = "Решение должно быть корректным определением статического метода.";
		public const string ShouldBeSingleMethod = "Решение должно состоять ровно из одного метода.";

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
				return new SolutionStyleError(userSolution.GetRoot(), ShouldBeMethod);
			if (cu.Members.Count > 1)
				return new SolutionStyleError(cu.Members[1], ShouldBeSingleMethod);
			var method = cu.Members[0] as MethodDeclarationSyntax;
			if (method == null)
				return new SolutionStyleError(cu.Members[0], ShouldBeMethod);
			return FindErrorInMethodDeclaration(method);
		}

		protected virtual SolutionStyleError FindErrorInMethodDeclaration(MethodDeclarationSyntax method)
		{
			return null;
		}
	}

	public class SingleStatementMethodValidator : IsStaticMethodValidator
	{
		public const string ShouldBeSingleMethodMessage = "Решение этой задачи должно быть в одно выражение 'return ...'";

		protected override SolutionStyleError FindErrorInMethodDeclaration(MethodDeclarationSyntax method)
		{
			var statements = method.Body.Statements;
			var hasError = statements.Count != 1 || !(statements.Single() is ReturnStatementSyntax);
			if (hasError)
				return new SolutionStyleError(method.Body, ShouldBeSingleMethodMessage);
			return null;
		}
	}
}