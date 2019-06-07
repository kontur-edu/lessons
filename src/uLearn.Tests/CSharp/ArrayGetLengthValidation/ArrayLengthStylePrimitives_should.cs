using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using Ulearn.Core.CSharp;

namespace uLearn.CSharp.ArrayGetLengthValidation
{
	[TestFixture]
	public class ArrayLengthStylePrimitives_should
	{
		private readonly PortableExecutableReference mscorlib =
			MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
		
		[Test]
		public void ContainsAssignmentOf_Should_FindAssignmentInCycle()
		{
			var code = @"
using System;

namespace uLearn.CSharp.ArrayGetLengthValidation.TestData.Incorrect
{
	public class GetLengthInDoWhileCycle
	{
		public void GetLengthInBody()
		{
			var arr = new int[2, 2];
			int count = 0;
			do
			{
				count = arr.GetLength(1);  
				Console.WriteLine(count);
				arr = new int[2, 5];
			} while (count < 2);
		}
	}
}
";
			var syntaxTree = CSharpSyntaxTree.ParseText(code);
			var compilation = CSharpCompilation.Create("MyCompilation", new[] { syntaxTree },
				new[] { mscorlib });
			var semanticModel = compilation.GetSemanticModel(syntaxTree);
			var nodes = syntaxTree.GetRoot().DescendantNodes().ToList();
			var cycleNode = nodes.OfType<DoStatementSyntax>().First();
			cycleNode.ContainsAssignmentOf("arr", semanticModel).Should().Be(true);
		}
		
		[Test]
		public void ContainsAssignmentOf_Should_FindDeclarationInCycle()
		{
			var code = @"
using System;

namespace uLearn.CSharp.ArrayGetLengthValidation.TestData.Incorrect
{
	public class GetLengthInDoWhileCycle
	{
		public void GetLengthInBody()
		{
			int count = 0;
			do
			{
				var arr = new int[2, 5];
			} while (count++ < 2);
		}
	}
}
";
			var syntaxTree = CSharpSyntaxTree.ParseText(code);
			var compilation = CSharpCompilation.Create("MyCompilation", new[] { syntaxTree },
				new[] { mscorlib });
			var semanticModel = compilation.GetSemanticModel(syntaxTree);
			var nodes = syntaxTree.GetRoot().DescendantNodes().ToList();
			var cycleNode = nodes.OfType<DoStatementSyntax>().First();
			cycleNode.ContainsAssignmentOf("arr", semanticModel).Should().Be(true);
		}
	}
}