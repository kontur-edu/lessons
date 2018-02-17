using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Ulearn.Common.Extensions;

namespace uLearn.CSharp
{
	public abstract class BaseStyleValidator : ICSharpSolutionValidator
	{
		protected FileLinePositionSpan GetSpan(SyntaxNode syntaxNode)
		{
			return syntaxNode.SyntaxTree.GetLineSpan(syntaxNode.Span);
		}

		protected FileLinePositionSpan GetSpan(SyntaxToken syntaxNode)
		{
			return syntaxNode.SyntaxTree.GetLineSpan(syntaxNode.Span);
		}

		protected string Report(SyntaxNode syntaxNode, string message)
		{
			return Report(GetSpan(syntaxNode), message);
		}

		protected string Report(SyntaxToken syntaxToken, string message)
		{
			return Report(GetSpan(syntaxToken), message);
		}

		public static string Report(FileLinePositionSpan span, string message)
		{
			var linePosition = span.StartLinePosition;
			return "Строка {0}, позиция {1}: {2}".WithArgs(linePosition.Line + 1, linePosition.Character, message);
		}

		protected IEnumerable<string> InspectAll<TNode>(SyntaxTree userSolution, SemanticModel semanticModel, Func<TNode, SemanticModel, IEnumerable<string>> inspect)
			where TNode : SyntaxNode
		{
			var nodes = userSolution.GetRoot().DescendantNodes().OfType<TNode>();
			return nodes.SelectMany(node => inspect(node, semanticModel));
		}

		protected IEnumerable<string> InspectAll<TNode>(SyntaxTree userSolution, Func<TNode, IEnumerable<string>> inspect)
			where TNode : SyntaxNode
		{
			var nodes = userSolution.GetRoot().DescendantNodes().OfType<TNode>();
			return nodes.SelectMany(inspect);
		}

		public string FindError(string code)
		{
			var syntaxTree = CSharpSyntaxTree.ParseText(code);
			var compilation = CSharpCompilation.Create("MyCompilation", new[] { syntaxTree }, new[] { mscorlib });
			var semanticModel = compilation.GetSemanticModel(syntaxTree);
			return FindError(syntaxTree, semanticModel);
		}

		public string FindError(SyntaxTree userSolution, SemanticModel semanticModel)
		{
			var errors = ReportAllErrors(userSolution, semanticModel).ToList();
			return errors.Any() ? string.Join("\n", errors) : null;
		}

		protected abstract IEnumerable<string> ReportAllErrors(SyntaxTree userSolution, SemanticModel semanticModel);

		private static readonly PortableExecutableReference mscorlib =
			MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
	}
}