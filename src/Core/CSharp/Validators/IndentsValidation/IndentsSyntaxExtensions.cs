﻿using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ulearn.Core.CSharp.Validators.IndentsValidation
{
	internal static class IndentsSyntaxExtensions
	{
		public static int GetLine(this SyntaxToken t)
		{
			return t.GetLocation().GetLineSpan().StartLinePosition.Line;
		}

		public static FileLinePositionSpan GetFileLinePositionSpan(this SyntaxNode syntaxNode)
		{
			var location = syntaxNode.GetLocation();
			return location.GetLineSpan();
		}

		public static bool OnSameLine(this SyntaxNode firstNode, SyntaxNode secondNode)
		{
			var firstPositionSpan = firstNode.GetFileLinePositionSpan();
			var secondPositionSpan = secondNode.GetFileLinePositionSpan();
			return secondPositionSpan.StartLinePosition.Line == firstPositionSpan.StartLinePosition.Line;
		}

		public static SyntaxNodeOrToken GetCondition(this SyntaxNode syntaxNode)
		{
			if (syntaxNode == null)
				return null;
			switch (syntaxNode)
			{
				case DoStatementSyntax doStatementSyntax:
					return SyntaxNodeOrToken.Create(syntaxNode.SyntaxTree, doStatementSyntax.Condition);
				case IfStatementSyntax ifStatementSyntax:
					return SyntaxNodeOrToken.Create(syntaxNode.SyntaxTree, ifStatementSyntax.Condition);
				case ForStatementSyntax forStatement:
					return SyntaxNodeOrToken.Create(syntaxNode.SyntaxTree, forStatement.Condition);
				case ForEachStatementSyntax foreachStatement:
					return SyntaxNodeOrToken.Create(syntaxNode.SyntaxTree, foreachStatement.Expression);
				case WhileStatementSyntax whileStatement:
					return SyntaxNodeOrToken.Create(syntaxNode.SyntaxTree, whileStatement.Condition);
			}
			return null;
		}

		public static IEnumerable<SyntaxNodeOrToken> CreateStatements(this SyntaxNode syntaxNode)
		{
			if (syntaxNode == null)
				yield break;
			var rootTree = syntaxNode.SyntaxTree;
			switch (syntaxNode)
			{
				case DoStatementSyntax doStatementSyntax:
					yield return SyntaxNodeOrToken.Create(rootTree, doStatementSyntax.Statement);
					yield return SyntaxNodeOrToken.Create(rootTree, doStatementSyntax.WhileKeyword, true);
					yield break;
				case ElseClauseSyntax elseClauseSyntax:
					var elseClauseStatement = elseClauseSyntax.Statement;
					yield return SyntaxNodeOrToken.Create(rootTree, elseClauseStatement, elseClauseStatement is IfStatementSyntax
																						&& elseClauseStatement.OnSameLine(elseClauseSyntax));
					yield break;
				case IfStatementSyntax ifStatementSyntax:
					yield return SyntaxNodeOrToken.Create(rootTree, ifStatementSyntax.Statement);
					if (ifStatementSyntax.Else != null)
						yield return SyntaxNodeOrToken.Create(rootTree, ifStatementSyntax.Else, true);
					yield break;
				case ForStatementSyntax forStatement:
					var innerForStatement = forStatement.Statement;
					yield return SyntaxNodeOrToken.Create(rootTree, forStatement.Statement,
						innerForStatement is ForStatementSyntax ? (bool?)null : false);
					yield break;
				case ForEachStatementSyntax foreachStatement:
					var innerForeachStatement = foreachStatement.Statement;
					yield return SyntaxNodeOrToken.Create(rootTree, foreachStatement.Statement,
						innerForeachStatement is ForEachStatementSyntax ? (bool?)null : false);
					yield break;
				case WhileStatementSyntax whileStatement:
					var innerWhileStatement = whileStatement.Statement;
					yield return SyntaxNodeOrToken.Create(rootTree, whileStatement.Statement,
						innerWhileStatement is WhileStatementSyntax ? (bool?)null : false);
					yield break;
				case UsingStatementSyntax usingStatementSyntax:
					var innerUsingStatement = usingStatementSyntax.Statement;
					yield return SyntaxNodeOrToken.Create(rootTree, usingStatementSyntax.Statement,
						innerUsingStatement is UsingStatementSyntax ? (bool?)null : false);
					yield break;
				default:
					yield break;
			}
		}

		public static SyntaxToken GetFirstTokenOfCorrectOpenbraceParent(this SyntaxToken openbrace)
		{
			if (openbrace.Parent is BaseTypeDeclarationSyntax ||
				openbrace.Parent is AccessorListSyntax ||
				openbrace.Parent is SwitchStatementSyntax ||
				openbrace.Parent is AnonymousObjectCreationExpressionSyntax ||
				openbrace.Parent.IsKind(SyntaxKind.ComplexElementInitializerExpression))
			{
				return openbrace.Parent.GetFirstToken();
			}
			if (openbrace.Parent is InitializerExpressionSyntax ||
				openbrace.Parent is BlockSyntax && !(openbrace.Parent.Parent is BlockSyntax))
			{
				return openbrace.Parent.Parent.GetFirstToken();
			}
			return default(SyntaxToken);
		}
	}
}