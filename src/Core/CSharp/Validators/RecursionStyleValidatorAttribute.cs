using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp
{
	public class RecursionStyleValidator : ICSharpSolutionValidator, IStrictValidator
	{
		private readonly bool requireRecursion;

		public RecursionStyleValidator(bool requireRecursion)
		{
			this.requireRecursion = requireRecursion;
		}

		public string FindError(SyntaxTree userSolution, SemanticModel semanticModel)
		{
			var recursiveMethods = userSolution.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Where(IsRecursive).ToList();

			if (requireRecursion && !recursiveMethods.Any())
				return "Решение должно быть рекурсивным";

			if (!requireRecursion && recursiveMethods.Any())
				return "Решение должно быть нерекурсивным";
			
			return null;
		}

		private static bool IsRecursive(MethodDeclarationSyntax method)
		{
			return
				method.Body != null
				&& method.Body.DescendantNodes()
					.OfType<SimpleNameSyntax>()
					.Any(n => n.Identifier.ValueText == method.Identifier.ValueText);
		}
	}
}