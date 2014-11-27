using System;
using System.CodeDom.Compiler;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace uLearn.Web
{
	public static class CreateListOfUsedTokens
	{
		[Explicit]
		[Test]
		public static void CreateList()
		{
			var solutions = WebCourseManager.Instance
				.GetCourses()
				.SelectMany(course => course.Slides.OfType<ExerciseSlide>())
				.Select(slide => slide.EthalonSolution);
			var domProvider = CodeDomProvider.CreateProvider("C#");
			var tokensDict = solutions
				.SelectMany(s => CSharpSyntaxTree
					.ParseText(s)
					.GetRoot()
					.DescendantTokens())
				.Select(token => token.Text)
				.Where(domProvider.IsValidIdentifier)
				.Where(s => Char.IsUpper(s[0]))
				.GroupBy(token => token)
				.ToDictionary(grouping => grouping.Key, grouping => grouping.Count());
			Console.Out.WriteLine("var date = '{0}';", DateTime.Now.ToString("dd.MM.yy"));
			Console.Out.WriteLine("var tokens = {");
			var output = "\t";
			foreach (var str in tokensDict.Select(pair => String.Format("'{0}': {1}", pair.Key, pair.Value)))
			{
				if (output.Length > 100)
				{
					Console.Out.WriteLine(output);
					output = "\t";
				}
				output += str + ", ";
			}
			Console.Out.WriteLine("};");
		}
	}
}