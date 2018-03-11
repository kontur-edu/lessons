using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp.Validators
{
	public class ExcessLinesValidator : BaseStyleValidator
	{
		public override List<SolutionStyleError> FindErrors(SyntaxTree userSolution, SemanticModel semanticModel)
		{
			var bracesPairs = userSolution.BuildBracesPairs().ToArray();

			return bracesPairs
				.Select(ReportWhenExistExcessLineBetweenDeclaration)
				.Concat(bracesPairs.SelectMany(ReportWhenExistExcessLineBetweenBraces))
				.Concat(bracesPairs.Select(ReportWhenNotExistLineBetweenBlocks))
				.Where(x => x != null)
				.OrderBy(e => e.Span.StartLinePosition.Line)
				.ToList();
		}

		private IEnumerable<SolutionStyleError> ReportWhenExistExcessLineBetweenBraces(BracesPair bracesPair)
		{
			var openBraceLine = GetStartLine(bracesPair.Open);
			var closeBraceLine = GetStartLine(bracesPair.Close);
			var statements = GetStatements(bracesPair.Open.Parent);
			if (statements.Length == 0)
				yield break;

			var firstStatement = statements.First();
			var lastStatement = statements.Last();

			var firstStatementLine = GetStartLine(firstStatement);
			var lastStatementLine = GetEndLine(lastStatement);

			if (openBraceLine != firstStatementLine
				&& openBraceLine + 1 != firstStatementLine
				&& !IsComment(bracesPair.Open.Parent, openBraceLine + 1))
				yield return new SolutionStyleError(bracesPair.Open, "После открывающей скобки не должно быть лишнего переноса строки.");

			if (closeBraceLine != lastStatementLine
				&& closeBraceLine - 1 != lastStatementLine
				&& !IsComment(bracesPair.Open.Parent, closeBraceLine - 1))
				yield return new SolutionStyleError(bracesPair.Close, "Перед закрывающей скобкой не должно быть лишнего переноса строки.");
		}

		private SolutionStyleError ReportWhenExistExcessLineBetweenDeclaration(BracesPair bracesPair)
		{
			var openBraceLine = GetStartLine(bracesPair.Open);
			var declarationLine = GetEndDeclaraionLine(bracesPair.Open.Parent);
			if (openBraceLine == declarationLine || declarationLine == -1)
				return null;

			if (declarationLine + 1 != openBraceLine)
				return new SolutionStyleError(bracesPair.Open, "Между объявлением и открывающей скобкой не должно быть лишнего переноса строки.");

			return null;
		}

		private SolutionStyleError ReportWhenNotExistLineBetweenBlocks(BracesPair bracesPair)
		{
			var closeBraceLine = GetStartLine(bracesPair.Close);

			var nextSyntaxNode = GetNextNode(bracesPair.Open.Parent);
			if (nextSyntaxNode == null || nextSyntaxNode is StatementSyntax)
				return null;
			var nextSyntaxNodeLine = GetStartLine(nextSyntaxNode);
			if (closeBraceLine + 1 == nextSyntaxNodeLine)
				return new SolutionStyleError(bracesPair.Close, "После закрывающей скобки нужно оставить одну пустую строку.");

			return null;
		}

		private static SyntaxNode GetNextNode(SyntaxNode syntaxNode)
		{
			SyntaxNode[] statements;
			SyntaxNode target;
			if (syntaxNode.Parent is MethodDeclarationSyntax || syntaxNode.Parent is StatementSyntax)
			{
				statements = GetStatements(syntaxNode.Parent.Parent);
				target = syntaxNode.Parent;
			}
			else
			{
				statements = GetStatements(syntaxNode.Parent);
				target = syntaxNode;
			}
			if (statements.Length == 0)
				return null;

			for (var i = 0; i < statements.Length; ++i)
			{
				var statement = statements[i];
				if (statement == target && i < statements.Length - 1)
					return statements[i + 1];
			}

			return null;
		}

		private static bool IsComment(SyntaxNode syntaxNode, int line)
		{
			return syntaxNode.DescendantTrivia()
				.Where(x => x.Kind() == SyntaxKind.MultiLineCommentTrivia
							|| x.Kind() == SyntaxKind.SingleLineCommentTrivia)
				.Any(x =>
				{
					var startLine = x.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
					var endLine = x.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
					return line >= startLine && line <= endLine;
				});
		}

		private static SyntaxNode[] GetStatements(SyntaxNode syntaxNode)
		{
			switch (syntaxNode)
			{
				case BlockSyntax blockSyntax:
					return blockSyntax.Statements.Cast<SyntaxNode>().ToArray();
				case ClassDeclarationSyntax classDeclarationSyntax:
					return classDeclarationSyntax.Members.Cast<SyntaxNode>().ToArray();
				case NamespaceDeclarationSyntax namespaceDeclarationSyntax:
					return namespaceDeclarationSyntax.Members.Cast<SyntaxNode>().ToArray();
				case CompilationUnitSyntax compilationUnitSyntax:
					return compilationUnitSyntax.Members.Cast<SyntaxNode>().ToArray();
			}
			return new SyntaxNode[0];
		}

		private static int GetEndDeclaraionLine(SyntaxNode syntaxNode)
		{
			switch (syntaxNode)
			{
				case ClassDeclarationSyntax classDeclarationSyntax:
					var baseListSyntax = classDeclarationSyntax.BaseList;
					if (baseListSyntax == null)
						return GetStartLine(syntaxNode);
					return GetEndLine(baseListSyntax);
				case BlockSyntax blockSyntax:
					if (blockSyntax.Parent.Kind() == SyntaxKind.Block)
						return -1;
					return GetEndDeclaraionLine(blockSyntax.Parent);
				case MethodDeclarationSyntax methodDeclarationSyntax:
					var constraintClauseSyntaxs = methodDeclarationSyntax.ConstraintClauses;
					if (!constraintClauseSyntaxs.Any())
						return GetEndArgumentsLine(methodDeclarationSyntax);
					return GetEndLine(constraintClauseSyntaxs.Last());
				default:
					return GetStartLine(syntaxNode);
			}
		}

		private static int GetStartLine(SyntaxToken syntaxToken)
		{
			return syntaxToken.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
		}

		private static int GetStartLine(SyntaxNode syntaxNode)
		{
			return syntaxNode.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
		}

		private static int GetEndLine(SyntaxNode syntaxNode)
		{
			return syntaxNode.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
		}

		private static int GetEndArgumentsLine(MethodDeclarationSyntax declarationSyntax)
		{
			var parametersCloseToken = declarationSyntax.ParameterList.CloseParenToken;
			return parametersCloseToken.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
		}
	}
}