using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp
{
	public class BoolCompareValidator : BaseStyleValidator
	{
		protected override IEnumerable<string> ReportAllErrors(SyntaxTree userSolution, SemanticModel semanticModel)
		{
			return InspectAll<BinaryExpressionSyntax>(userSolution, semanticModel, Inspect);
		}

		private IEnumerable<string> Inspect(BinaryExpressionSyntax binaryExpression, SemanticModel semanticModel)
		{
			var operatorKind = binaryExpression.OperatorToken.Kind();
			if (operatorKind != SyntaxKind.EqualsEqualsToken && operatorKind != SyntaxKind.ExclamationEqualsToken)
				yield break;
			var leftNodeTypeInfo = semanticModel.GetTypeInfo(binaryExpression.Left);
			var leftNodeTree = binaryExpression.Left as LiteralExpressionSyntax;
			var rightNodeTypeInfo = semanticModel.GetTypeInfo(binaryExpression.Right);
			var rightNodeTree = binaryExpression.Right as LiteralExpressionSyntax;
			if (IsBooleanType(rightNodeTypeInfo) && IsBooleanType(leftNodeTypeInfo)
				&& (IsBoolLiteral(leftNodeTree) || IsBoolLiteral(rightNodeTree)))
				yield return Report(binaryExpression, "Ненужное сравнение с переменной типа bool. Вместо x == true лучше писать просто x, а вместо x != true лучше писать !x.");
		}

		private static bool IsBoolLiteral(LiteralExpressionSyntax node)
		{
			if (node == null)
				return false;
			var token = node.Token.Kind();
			return token == SyntaxKind.TrueKeyword || token == SyntaxKind.FalseKeyword;
		}

		private bool IsBooleanType(TypeInfo node) => node.Type?.Name == "Boolean";
	}
}