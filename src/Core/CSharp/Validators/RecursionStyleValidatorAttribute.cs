using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp.Validators
{
	public class RecursionStyleValidator : ICSharpSolutionValidator, IStrictValidator
	{
		private readonly bool requireRecursion;

		public RecursionStyleValidator(bool requireRecursion)
		{
			this.requireRecursion = requireRecursion;
		}

		public List<SolutionStyleError> FindErrors(SyntaxTree userSolution, SemanticModel semanticModel)
		{
			var recursiveMethods = userSolution.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(IsRecursive).ToList();

			SolutionStyleError error = null;
			if (requireRecursion && !recursiveMethods.Any())
			{
			 	error = new SolutionStyleError(userSolution.GetRoot(), "Решение должно быть рекурсивным");
			}

			if (!requireRecursion && recursiveMethods.Any())
				error = new SolutionStyleError(recursiveMethods.First(), "Решение должно быть нерекурсивным");

			if (error != null)
				return new List<SolutionStyleError> { error };

			return new List<SolutionStyleError>();
		}

		private static bool IsRecursive(MethodDeclarationSyntax method)
		{
			return method.Body != null && method.Body.DescendantNodes()
					.OfType<SimpleNameSyntax>()
					.Any(n => n.Identifier.ValueText == method.Identifier.ValueText);
		}
	}
}