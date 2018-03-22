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
			string positionInfo;
			if (Span.StartLinePosition.Line == Span.EndLinePosition.Line)
				positionInfo = $"Строка {Span.StartLinePosition.Line + 1}, позиция {Span.StartLinePosition.Character}";
			else
				positionInfo = $"Строки {Span.StartLinePosition.Line + 1}—{Span.EndLinePosition.Line + 1}";
			return $"{positionInfo}: {Message}";
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals((SolutionStyleError)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				// ReSharper disable once ImpureMethodCallOnReadonlyValueField
				return (Span.GetHashCode() * 397) ^ (Message != null ? Message.GetHashCode() : 0);
			}
		}

		private bool Equals(SolutionStyleError other)
		{
			return Equals(Span, other.Span) && string.Equals(Message, other.Message);
		}
	}
}