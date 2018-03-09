using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp.Validators
{
	public class ExponentiationValidator: BaseStyleValidator
	{
		private static readonly HashSet<double> forbiddenDegrees = 
			new HashSet<double>{2.0, 3.0};

		public override List<SolutionStyleError> FindErrors(SyntaxTree userSolution, SemanticModel semanticModel)
		{
			return InspectAll<InvocationExpressionSyntax>(userSolution, semanticModel, InspectMethod).ToList();
		}

		private IEnumerable<SolutionStyleError> InspectMethod(InvocationExpressionSyntax methodInvocation, SemanticModel semanticModel)
		{
			var symbol = semanticModel.GetSymbolInfo(methodInvocation).Symbol;
			if (methodInvocation.IsMathPow(symbol) &&
				methodInvocation.ArgumentList.ContainsForbiddenDegrees(forbiddenDegrees))
				yield return new SolutionStyleError(
					methodInvocation,
					"Неэффективный код. Если число нужно возвести в квадрат или куб, лучше сделать это с помощью умножения, не используя более общий, но менее быстрый `Math.Pow`."
				);
		}
	}
}