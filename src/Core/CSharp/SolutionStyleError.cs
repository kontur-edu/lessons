using Microsoft.CodeAnalysis;

using SyntaxNodeOrToken = uLearn.CSharp.Validators.IndentsValidation.SyntaxNodeOrToken;

namespace uLearn.CSharp
{
	public class SolutionStyleError
	{
		public readonly FileLinePositionSpan Span;

		public readonly string Message;
		
		public SolutionStyleError(FileLinePositionSpan span, string message)
		{
			Span = span;
			Message = message;
		}

		public SolutionStyleError(SyntaxNode node, string message)
			:this(GetFileLinePositionSpan(node), message)
		{
		}

		public SolutionStyleError(SyntaxToken token, string message)
			:this(GetFileLinePositionSpan(token), message)
		{
		}
		
		public SolutionStyleError(SyntaxNodeOrToken nodeOrToken, string message)
			:this(nodeOrToken.GetFileLinePositionSpan(), message)
		{
		}
		
		private static FileLinePositionSpan GetFileLinePositionSpan(SyntaxNode node)
		{
			return node.SyntaxTree.GetLineSpan(node.Span);
		}
		
		private static FileLinePositionSpan GetFileLinePositionSpan(SyntaxToken token)
		{
			return token.SyntaxTree.GetLineSpan(token.Span);
		}

		public override string ToString()
		{
			return $"StyleIssue({Message} on {Span})";
		}

		public string GetMessageWithPositions()
		{
			var positionInfo = "";
			if (Span.StartLinePosition.Line == Span.EndLinePosition.Line)
				positionInfo = $"Строка {Span.StartLinePosition.Line + 1}, позиция {Span.StartLinePosition.Character}";
			else
				positionInfo = $"Строки {Span.StartLinePosition.Line + 1}—{Span.EndLinePosition.Line + 1}";
			return $"{positionInfo}: {Message}";
		}
	}
}