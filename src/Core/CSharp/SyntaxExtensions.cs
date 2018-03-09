using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using uLearn.CSharp.Validators.IndentsValidation;
using Ulearn.Common.Extensions;

namespace uLearn.CSharp
{
	public static class SyntaxExtensions
	{
		public static bool IsVoid(this MethodDeclarationSyntax method)
		{
			var predefinedTypeSyntax = method.ReturnType as PredefinedTypeSyntax;
			return predefinedTypeSyntax != null && predefinedTypeSyntax.Keyword.IsKind(SyntaxKind.VoidKeyword);
		}

		public static IEnumerable<MemberDeclarationSyntax> GetMembers(this SyntaxNode node)
		{
			return node.DescendantNodes()
				.OfType<MemberDeclarationSyntax>()
				.Where(n => n is BaseTypeDeclarationSyntax || n is MethodDeclarationSyntax);
		}

		public static SyntaxList<SyntaxNode> GetBody(this SyntaxNode node)
		{
			var method = node as BaseMethodDeclarationSyntax;
			if (method != null)
			{
				var body = method.Body;
				if (body != null)
					return body.Statements;
				return new SyntaxList<SyntaxNode>();
			}
			var type = node as TypeDeclarationSyntax;
			if (type != null)
				return type.Members;
			return new SyntaxList<SyntaxNode>();
		}

		public static IEnumerable<SyntaxNode> GetParents(this SyntaxNode node)
		{
			while (node.Parent != null)
				yield return node = node.Parent;
		}

		public static IEnumerable<SyntaxNode> GetParents(this SyntaxToken token)
		{
			return new[] { token.Parent }.Concat(token.Parent.GetParents());
		}

		public static IEnumerable<SyntaxNode> GetParents(this SyntaxTrivia trivia)
		{
			return trivia.Token.GetParents();
		}

		public static SyntaxToken Identifier(this MemberDeclarationSyntax syntax)
		{
			return ((dynamic)syntax).Identifier;
		}

		public static bool HasAttribute<TAttr>(this MemberDeclarationSyntax node) where TAttr : Attribute
		{
			return node.GetAttributes<TAttr>().Any();
		}

		public static object GetObjArgument(this AttributeSyntax attribute, int index)
		{
			var expr = (LiteralExpressionSyntax)attribute.ArgumentList.Arguments[index].Expression;
			return expr.Token.Value;
		}

		public static string GetArgument(this AttributeSyntax attribute, int index)
		{
			return (string)attribute.GetObjArgument(index);
		}

		private static SyntaxList<AttributeListSyntax> AttributeLists(this MemberDeclarationSyntax node)
		{
			return (SyntaxList<AttributeListSyntax>)((dynamic)node).AttributeLists;
		}

		public static IEnumerable<AttributeSyntax> GetAttributes<TAttr>(this MemberDeclarationSyntax node)
			where TAttr : Attribute
		{
			string attrShortName = GetAttributeShortName<TAttr>();
			return node.AttributeLists()
				.SelectMany(a => a.Attributes)
				.Where(a => a.Name.ToString() == attrShortName);
		}

		public static string GetAttributeShortName<TAttr>()
		{
			string attrName = typeof(TAttr).Name;
			return attrName.EndsWith("Attribute") ? attrName.Substring(0, attrName.Length - "Attribute".Length) : attrName;
		}

		public static MethodDeclarationSyntax WithoutAttributes(this MethodDeclarationSyntax node)
		{
			return node.WithAttributeLists(new SyntaxList<AttributeListSyntax>());
		}

		public static MemberDeclarationSyntax WithoutAttributes(this MemberDeclarationSyntax node)
		{
			return (MemberDeclarationSyntax)((dynamic)node).WithAttributeLists(new SyntaxList<AttributeListSyntax>());
		}

		private static string PrettyString(SyntaxNode node, SyntaxToken tokenToAlignLeft)
		{
			int bodyNestingSize = node.SyntaxTree.GetLineSpan(tokenToAlignLeft.Span).StartLinePosition.Character;
			return (new string('\t', bodyNestingSize) + node).RemoveCommonNesting();
		}

		private static string PrettyString(MethodDeclarationSyntax node)
		{
			var body = node.Body;
			if (body == null)
				return node.ToFullString().RemoveCommonNesting();
			return PrettyString(node, body.CloseBraceToken);
		}

		private static string PrettyString(BaseTypeDeclarationSyntax node)
		{
			return PrettyString(node, node.CloseBraceToken);
		}

		private static string PrettyString(MemberDeclarationSyntax node)
		{
			if (node is FieldDeclarationSyntax)
				return node.ToString().RemoveCommonNesting();
			return node.ToFullString().RemoveCommonNesting();
		}

		public static string ToPrettyString(this SyntaxNode node)
		{
			return PrettyString((dynamic)node);
		}

		public static string ToNotIdentedString(this SyntaxNode node)
		{
			return node.ToString().RemoveCommonNesting();
		}

		public static MethodDeclarationSyntax TransformExercise(this MethodDeclarationSyntax method)
		{
			return method
				.WithoutAttributes()
				.WithBody(method.Body.WithStatements(new SyntaxList<StatementSyntax>()));
		}

		public static int GetLine(this SyntaxToken token)
		{
			return token.GetLocation().GetLineSpan().StartLinePosition.Line;
		}

		public static IEnumerable<BracesPair> BuildBracesPairs(this SyntaxTree tree)
		{
			var braces = tree.GetRoot().DescendantTokens()
				.Where(t => t.IsKind(SyntaxKind.OpenBraceToken) || t.IsKind(SyntaxKind.CloseBraceToken));
			var openbracesStack = new Stack<SyntaxToken>();
			foreach (var brace in braces)
			{
				if (brace.IsKind(SyntaxKind.OpenBraceToken))
					openbracesStack.Push(brace);
				else
					yield return new BracesPair(openbracesStack.Pop(), brace);
			}
		}
	}
}