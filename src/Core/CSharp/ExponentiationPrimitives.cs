using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ulearn.Core.CSharp
{
	public static class ExponentiationPrimitives
	{
		public static bool IsMathPow(this InvocationExpressionSyntax invocation, ISymbol symbol = null)
		{
			var invokedMethodName = symbol?.ToString();
			return IsMathPowMethodName(invokedMethodName) && invocation.ArgumentList.Arguments.Count == 2;
		}

		public static bool ContainsForbiddenDegrees(this ArgumentListSyntax argumentListSyntax,
			HashSet<double> forbiddenDegrees)
		{
			var degree = argumentListSyntax.Arguments[1].ToString();
			return double.TryParse(degree, out var degreeValue) &&
				forbiddenDegrees.Contains(degreeValue);
		}

		private static bool IsMathPowMethodName(string methodName)
		{
			return methodName == "System.Math.Pow(double, double)";
		}
	}
}