using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SharpLessons.CSharp
{
	public static class SyntaxExtensions
	{
		public static bool HasAttribute<TAttr>(this MethodDeclarationSyntax node) where TAttr : Attribute
		{
			return node.GetAttributes<TAttr>().Any();
		}

		public static bool HasAttribute<TAttr>(this ClassDeclarationSyntax node) where TAttr : Attribute
		{
			return node.GetAttributes<TAttr>().Any();
		}

		public static string GetHint(this AttributeSyntax attribute)
		{
			if (attribute.Name.ToString() != GetAttributeShortName<HintAttribute>()) throw new Exception("Not a HintAttribute");
			var expr = (LiteralExpressionSyntax) attribute.ArgumentList.Arguments[0].Expression;
			return (string) expr.Token.Value;
		}

		public static IEnumerable<AttributeSyntax> GetAttributes<TAttr>(this MethodDeclarationSyntax node)
			where TAttr : Attribute
		{
			return GetAttributes<TAttr>(node.AttributeLists);
		}

		public static IEnumerable<AttributeSyntax> GetAttributes<TAttr>(this ClassDeclarationSyntax node)
			where TAttr : Attribute
		{
			return GetAttributes<TAttr>(node.AttributeLists);
		}

		public static IEnumerable<AttributeSyntax> GetAttributes<TAttr>(SyntaxList<AttributeListSyntax> attributes)
			where TAttr : Attribute
		{
			string attrShortName = GetAttributeShortName<TAttr>();
			return attributes
				.SelectMany(a => a.Attributes)
				.Where(a => a.Name.ToString() == attrShortName);
		}

		public static string GetAttributeShortName<TAttr>()
		{
			string attrName = typeof (TAttr).Name;
			return attrName.EndsWith("Attribute") ? attrName.Substring(0, attrName.Length - "Attribute".Length) : attrName;
		}

		public static MethodDeclarationSyntax WithoutAttributes(this MethodDeclarationSyntax node)
		{
			return node.WithAttributeLists(new SyntaxList<AttributeListSyntax>());
		}

		public static string ToNotIdentedString(this SyntaxNode node)
		{
			return node.ToFullString().RemoveCommonNesting();
		}

		public static MethodDeclarationSyntax TransformExercise(this MethodDeclarationSyntax method)
		{
			return method
				.WithoutAttributes()
				.WithBody(method.Body.WithStatements(new SyntaxList<StatementSyntax>()));
		}
	}
}