using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AntiPlagiarism.Web.CodeAnalyzing.CSharp;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Ulearn.Common.Extensions;

namespace AntiPlagiarism.Tests.CodeAnalyzing.CSharp
{
	[TestFixture]
	public class CodeUnitsExtractor_should
	{
		private CodeUnitsExtractor extractor;
		
		/* TODO (andgein): make this code more straighted and clear? */
		private static DirectoryInfo TestDataDir => new DirectoryInfo(
			Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", 
				"CodeAnalyzing", "CSharp", "TestData")
		);

		[SetUp]
		public void SetUp()
		{
			extractor = new CodeUnitsExtractor();
		}

		[Test]
		public void ExtractCodeUnits()
		{
			var codeUnits = extractor.Extract(CommonTestData.SimpleProgramWithMethodAndProperty);
			
			Assert.AreEqual(3, codeUnits.Count);
			CollectionAssert.AreEqual(new List<int> { 31, 45, 51 }, codeUnits.Select(u => u.FirstTokenIndex));

			var methodBodyUnit = codeUnits[0];
			var methodBodyExpectedKinds = new List<SyntaxKind>
			{
				SyntaxKind.OpenBraceToken,
				SyntaxKind.IdentifierToken,
				SyntaxKind.DotToken,
				SyntaxKind.IdentifierToken,
				SyntaxKind.OpenParenToken,
				SyntaxKind.StringLiteralToken,
				SyntaxKind.CloseParenToken,
				SyntaxKind.SemicolonToken,
				SyntaxKind.CloseBraceToken,
			};
			Assert.AreEqual(methodBodyExpectedKinds.Count, methodBodyUnit.Tokens.Count);
			CollectionAssert.AreEqual(methodBodyExpectedKinds, methodBodyUnit.Tokens.Select(t => t.Kind()));
		}

		[Test]
		public void GiveCorrectNodeNames()
		{
			var syntaxTree = CSharpSyntaxTree.ParseText(CommonTestData.SimpleProgramWithMethodAndProperty);
			var syntaxTreeRoot = syntaxTree.GetRoot();
			
			Assert.AreEqual("ROOT", CodeUnitsExtractor.GetNodeName(syntaxTreeRoot));
			
			var namespaceDeclaration = syntaxTreeRoot.ChildNodes().First(n => n.Kind() == SyntaxKind.NamespaceDeclaration);
			Assert.AreEqual("HelloWorld.Namespace", CodeUnitsExtractor.GetNodeName(namespaceDeclaration));
			
			var classDeclaration = namespaceDeclaration.ChildNodes().First(n => n.Kind() == SyntaxKind.ClassDeclaration);
			Assert.AreEqual("Program", CodeUnitsExtractor.GetNodeName(classDeclaration));
			
			var methodDeclaration = classDeclaration.ChildNodes().First(n => n.Kind() == SyntaxKind.MethodDeclaration);
			Assert.AreEqual("Main", CodeUnitsExtractor.GetNodeName(methodDeclaration));
			
			var propertyDeclaration = classDeclaration.ChildNodes().First(n => n.Kind() == SyntaxKind.PropertyDeclaration);
			Assert.AreEqual("A", CodeUnitsExtractor.GetNodeName(propertyDeclaration));
		}

		[Test]
		public void ExtractInnerClass()
		{
			var codeUnits = ExtractCodeUnitsFromTestFile("NestedClasses.cs");
			
			/* At least one code unit is inside InnerClass */
			Assert.IsTrue(codeUnits.Any(u => u.Path.Parts.Any(p => p.Name == "InnerClass")));
		}

		[Test]
		public void ExtractConstructors()
		{
			var codeUnits = ExtractCodeUnitsFromTestFile("Constructors.cs");
			
			Assert.AreEqual(5, codeUnits.Count);
			
			/* One code unit should be ThisConstructorInitializer */
			Assert.AreEqual(1, codeUnits.Count(u => u.Path.Parts.Any(p => p.Name == "ThisConstructorInitializer")));
		}

		[Test]
		public void ExtractOperators()
		{
			var codeUnits = ExtractCodeUnitsFromTestFile("Operators.cs");
			
			Assert.AreEqual(4, codeUnits.Count(u => u.Path.Parts.Any(p => p.Name.StartsWith("Operator"))));
			Assert.AreEqual(3, codeUnits.Count(u => u.Path.Parts.Any(p => p.Name.StartsWith("Conversion-"))));
		}

		private List<CodeUnit> ExtractCodeUnitsFromTestFile(string filename)
		{
			var testFile = TestDataDir.GetFile(filename);
			var testContent = File.ReadAllText(testFile.FullName);
			return extractor.Extract(testContent);
		}
	}
}