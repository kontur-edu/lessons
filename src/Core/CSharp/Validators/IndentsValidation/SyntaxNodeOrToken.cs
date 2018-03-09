using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace uLearn.CSharp.Validators.IndentsValidation
{
	public class SyntaxNodeOrToken
	{
		public SyntaxTree RootTree { get; }

		private SyntaxNodeOrToken(SyntaxTree rootTree)
		{
			RootTree = rootTree;
		}

		public SyntaxNode SyntaxNode { get; set; }
		public SyntaxToken SyntaxToken { get; set; }
		public bool? OnSameIndentWithParent { get; private set; }

		public static SyntaxNodeOrToken Create(SyntaxTree rootTree, SyntaxNode syntaxNode,
			bool? onSameIndentWithParent = false)
		{
			return new SyntaxNodeOrToken(rootTree)
			{
				SyntaxNode = syntaxNode,
				OnSameIndentWithParent = onSameIndentWithParent
			};
		}

		public static SyntaxNodeOrToken Create(SyntaxTree rootTree, SyntaxToken syntaxToken,
			bool? onSameIndentWithParent = false)
		{
			return new SyntaxNodeOrToken(rootTree)
			{
				SyntaxToken = syntaxToken,
				OnSameIndentWithParent = onSameIndentWithParent
			};
		}

		public SyntaxKind Kind
		{
			get
			{
				if (SyntaxToken != default(SyntaxToken))
					return SyntaxToken.Kind();
				if (SyntaxNode != null)
					return SyntaxNode.Kind();
				return SyntaxKind.None;
			}
		}

		public FileLinePositionSpan GetFileLinePositionSpan()
		{
			Location location = null;
			if (SyntaxNode != null)
				location = SyntaxNode.GetLocation();
			else if (SyntaxToken != default(SyntaxToken))
				location = SyntaxToken.GetLocation();

			if (location == null)
				return default(FileLinePositionSpan);
			return location.GetLineSpan();
		}

		public SyntaxNodeOrToken GetParent()
		{
			if (SyntaxNode != null)
				return Create(RootTree, SyntaxNode.Parent);
			if (SyntaxToken != default(SyntaxToken))
				return Create(RootTree, SyntaxToken.Parent);
			return null;
		}

		public SyntaxTriviaList GetLeadingSyntaxTrivias()
		{
			if (SyntaxNode != null)
				return SyntaxNode.GetLeadingTrivia();
			if (SyntaxToken != default(SyntaxToken))
				return SyntaxToken.LeadingTrivia;
			return default(SyntaxTriviaList);
		}
		
		public override string ToString()
		{
			if (SyntaxNode != null)
				return SyntaxNode.ToString();
			if (SyntaxToken != default(SyntaxToken))
				return SyntaxToken.ToString();
			return base.ToString();
		}
	}
}