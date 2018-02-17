using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp
{
	public class ExponentiationValidator: BaseStyleValidator
	{
		private static readonly HashSet<double> forbiddenDegrees = 
			new HashSet<double>{2.0, 3.0};
		
		protected override IEnumerable<string> ReportAllErrors(SyntaxTree userSolution, SemanticModel semanticModel)
		{
			return InspectAll<InvocationExpressionSyntax>(userSolution, semanticModel, InspectMethod);
		}

		private IEnumerable<string> InspectMethod(InvocationExpressionSyntax methodInvocation, SemanticModel semanticModel)
		{
			var symbol = semanticModel.GetSymbolInfo(methodInvocation).Symbol;
			if (methodInvocation.IsMathPow(symbol) &&
				methodInvocation.ArgumentList.ContainsForbiddenDegrees(forbiddenDegrees))
				yield return Report(methodInvocation, "Неэффективный код. Если число нужно возвести в квадрат или куб, лучше сделать это с помощью умножения, не используя более общий, но менее быстрый Math.Pow");
		}
	}
}