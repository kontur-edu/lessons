using System;
using System.Collections.Generic;
using System.Linq;
using AntiPlagiarism.Web.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Ulearn.Common.Extensions;

namespace AntiPlagiarism.Web.CodeAnalyzing.CSharp
{
	public class CodeUnitsExtractor
	{
		public List<CodeUnit> Extract(string program)
		{
			var syntaxTree = CSharpSyntaxTree.ParseText(program);
			var syntaxTreeRoot = syntaxTree.GetRoot();
			var codeUnits = GetCodeUnitsFrom(syntaxTreeRoot as CompilationUnitSyntax, new Stack<CodePathPart>()).ToList();

			var tokens = syntaxTreeRoot.GetTokens();
			var tokenIndexByPosition = tokens.Enumerate().ToDictionary(
				t => t.Item.SpanStart,
				t => t.Index
			);

			foreach (var unit in codeUnits)
			{
				unit.FirstTokenIndex = tokenIndexByPosition[unit.Tokens[0].SpanStart];
			}

			return codeUnits;
		}
		
		private IEnumerable<CodeUnit> GetCodeUnitsFrom(CSharpSyntaxNode obj, Stack<CodePathPart> currentCodePath)
		{
			var codeUnits = new List<CodeUnit>();
			
			codeUnits.AddRange(GetCodeUnitsFromChilds(obj as CompilationUnitSyntax, currentCodePath, z => z.Members));
			codeUnits.AddRange(GetCodeUnitsFromChilds(obj as NamespaceDeclarationSyntax, currentCodePath, z => z.Members));
			codeUnits.AddRange(GetCodeUnitsFromChilds(obj as ClassDeclarationSyntax, currentCodePath, z => z.Members));
			codeUnits.AddRange(GetCodeUnitsFromChilds(obj as PropertyDeclarationSyntax, currentCodePath, PropertyEnumerator));
			codeUnits.AddRange(GetCodeUnitsFromChilds(obj as MethodDeclarationSyntax, currentCodePath, MethodEnumerator));
			codeUnits.AddRange(GetCodeUnitsFromChilds(obj as ConstructorDeclarationSyntax, currentCodePath, MethodEnumerator));
			codeUnits.AddRange(GetCodeUnitsFromChilds(obj as OperatorDeclarationSyntax, currentCodePath, MethodEnumerator));
			codeUnits.AddRange(GetCodeUnitsFromChilds(obj as ConversionOperatorDeclarationSyntax, currentCodePath, MethodEnumerator));

			codeUnits.AddRange(GetCodeUnitFrom(obj as AccessorDeclarationSyntax, currentCodePath, z => z.Body));
			codeUnits.AddRange(GetCodeUnitFrom(obj as ArrowExpressionClauseSyntax, currentCodePath, z => z.Expression));
			codeUnits.AddRange(GetCodeUnitFrom(obj as BlockSyntax, currentCodePath, z => z));
			codeUnits.AddRange(GetCodeUnitFrom(obj as ConstructorInitializerSyntax, currentCodePath, z => z));

			return codeUnits;
		}
		
		private IEnumerable<CodeUnit> GetCodeUnitsFromChilds<T>(T node, Stack<CodePathPart> currentCodePath, Func<T, IEnumerable<CSharpSyntaxNode>> childEnumeratorFunction)
            where T : CSharpSyntaxNode
        {
			if (node == null)
				yield break;
			
			var nodeName = GetNodeName(node);
			currentCodePath.Push(new CodePathPart(node, nodeName));

			foreach (var children in childEnumeratorFunction(node))
			{
				foreach (var codeUnit in GetCodeUnitsFrom(children, currentCodePath))
					yield return codeUnit;
			}

			currentCodePath.Pop();
		}

        private static IEnumerable<CodeUnit> GetCodeUnitFrom<T>(T node, Stack<CodePathPart> currentCodePath, Func<T, CSharpSyntaxNode> getEntryFunction)
            where T : CSharpSyntaxNode
        {
			if (node == null)
				yield break;
			
			var nodeName = GetNodeName(node);
			currentCodePath.Push(new CodePathPart(node, nodeName));

			var codePath = new CodePath(currentCodePath.Reverse().ToList());
			var entry = getEntryFunction(node);
			if (entry != null)
			{
				var tokens = entry.GetTokens().ToList();
				yield return new CodeUnit(codePath, tokens);
			}
			
			currentCodePath.Pop();
		}

        private static IEnumerable<CSharpSyntaxNode> PropertyEnumerator(PropertyDeclarationSyntax z)
        {
            if (z.AccessorList == null)
            {
                if (z.ExpressionBody != null)
                    yield return z.ExpressionBody;
            }
            else
            {
                foreach (var e in z.AccessorList.Accessors)
                    yield return e;
            }
        }

		private static IEnumerable<CSharpSyntaxNode> MethodEnumerator(BaseMethodDeclarationSyntax z)
		{
			if (z is ConstructorDeclarationSyntax constructor)
				yield return constructor.Initializer;
			
            if (z.Body != null)
                yield return z.Body;
            else if (z.ExpressionBody != null)
                yield return z.ExpressionBody;
        }

		private static string InternalGetNodeName(CompilationUnitSyntax node) => "ROOT";
		private static string InternalGetNodeName(NamespaceDeclarationSyntax node) => node.Name.ToString();
		private static string InternalGetNodeName(BaseTypeDeclarationSyntax node) => node.Identifier.ValueText;
		private static string InternalGetNodeName(PropertyDeclarationSyntax node) => node.Identifier.ValueText;
		private static string InternalGetNodeName(MethodDeclarationSyntax node) => node.Identifier.ToString();
		private static string InternalGetNodeName(ConstructorDeclarationSyntax node) => node.Identifier.ToString();
		private static string InternalGetNodeName(OperatorDeclarationSyntax node) => "Operator" + node.OperatorToken;
		private static string InternalGetNodeName(ConversionOperatorDeclarationSyntax node) => 
			"Conversion-" + node.Type + "-from-" + string.Join("-", node.ParameterList.Parameters.Select(p => p.Type));
		private static string InternalGetNodeName(CSharpSyntaxNode node) => node.Kind().ToString();

		public static string GetNodeName(SyntaxNode node)
		{
			if (! (node is CSharpSyntaxNode))
				throw new InvalidOperationException("node should be CSharpSyntaxNode");
			return InternalGetNodeName((dynamic)node);
		}
	}
}