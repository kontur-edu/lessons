using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp
{
	public class NamingStyleValidator : BaseStyleValidator
	{
		protected override IEnumerable<string> ReportAllErrors(SyntaxTree userSolution, SemanticModel semanticModel)
		{
			return InspectAll<MethodDeclarationSyntax>(userSolution, InspectMethod);
		}

		private IEnumerable<string> InspectMethod(MethodDeclarationSyntax method)
		{
			var name = method?.Identifier.Text;
			if (name == null || method.AttributeLists.Any()) 
				yield break;
			if (method.AttributeLists.Any()) 
				yield break; // Turn this check off for [Test], [TestCase] and all other special cases marked with attribute
			if (method.IsVoidGetter() && !method.AttributeLists.Any())
				yield return Report(method.Identifier, "'Get' метод без возвращаемого значения — это бессмыслица");
			if (method.IsNoArgsSetter())
				yield return Report(method.Identifier, "'Set' метод без аргументов — это бессмыслица");
			if (name.IsSingleWordGerundIdentifier())
				yield return Report(method.Identifier, "Называйте методы простыми глаголами! Например, Move, а не Moving");
			if (name.IsSingleWordIonIdentifier())
				yield return Report(method.Identifier, "Называйте методы глаголами! Например, Convert, а не Conversion");
		}
	}
}