using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Ulearn.Core.CSharp.Validators.IndentsValidation
{
	internal static class SyntaxNodeOrTokenExtensions
	{
		public static int GetConditionEndLine(this SyntaxNodeOrToken nodeOrToken)
		{
			var condition = nodeOrToken.SyntaxNode.GetCondition();
			if (condition == null)
				return nodeOrToken.GetStartLine();
			return condition.GetEndLine();
		}

		public static int GetEndLine(this SyntaxNodeOrToken nodeOrToken)
		{
			var linePositionSpan = nodeOrToken.GetFileLinePositionSpan();
			if (linePositionSpan.Equals(default(FileLinePositionSpan)))
				return -1;
			return linePositionSpan.EndLinePosition.Line + 1;
		}

		public static int GetStartLine(this SyntaxNodeOrToken nodeOrToken)
		{
			var linePositionSpan = nodeOrToken.GetFileLinePositionSpan();
			if (linePositionSpan.Equals(default(FileLinePositionSpan)))
				return -1;
			return linePositionSpan.StartLinePosition.Line + 1;
		}

		public static int GetValidationStartIndexInSpaces(this SyntaxNodeOrToken nodeOrToken)
		{
			var currentLine = nodeOrToken.GetStartLine();
			var parent = nodeOrToken.GetParent();
			var parentLine = parent.GetStartLine();
			if (parent.Kind == SyntaxKind.ElseClause
				&& (nodeOrToken.Kind == SyntaxKind.IfStatement
					|| nodeOrToken.Kind == SyntaxKind.ExpressionStatement)
				&& currentLine == parentLine)
				return parent.GetValidationStartIndexInSpaces();
			var sourceText = nodeOrToken.RootTree.GetText();
			var syntaxTrivias = nodeOrToken.GetLeadingSyntaxTrivias();
			var textSpan = syntaxTrivias.Count == 1
				? syntaxTrivias.FullSpan
				: syntaxTrivias.LastOrDefault().FullSpan;
			var subText = sourceText.GetSubText(textSpan).ToString();
			return GetRealTriviaLength(subText);
		}

		public static bool HasExcessNewLines(this SyntaxNodeOrToken nodeOrToken)
		{
			var syntaxTrivias = nodeOrToken.GetLeadingSyntaxTrivias();
			return syntaxTrivias.Count > 1;
		}

		private static int GetRealTriviaLength(string trivia)
		{
			var count = 0;
			var currentTabSpaces = 0;
			for (var i = 0; i < trivia.Length; ++i)
			{
				if (trivia[i] == '\t')
				{
					if (currentTabSpaces == 0)
						count += 4;
					else
						count += currentTabSpaces + (4 - currentTabSpaces);
					currentTabSpaces = 0;
				}
				else if (trivia[i] == ' ')
				{
					currentTabSpaces++;
				}
				if (currentTabSpaces == 4)
				{
					count += 4;
					currentTabSpaces = 0;
				}
			}
			return count + currentTabSpaces;
		}
	}
}