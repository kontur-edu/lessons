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
				.Select(slide => slide.EthalonSolution)
				.Select(solution => solution.Split(new[] { '\r', '\n' }, 2)[1]); // remove method declarations
			var domProvider = CodeDomProvider.CreateProvider("C#");
			var tokensDict = solutions
				.SelectMany(s => CSharpSyntaxTree
					.ParseText(s)
					.GetRoot()
					.DescendantTokens())
				.Select(token => token.Text)
				.Where(domProvider.IsValidIdentifier)
				.GroupBy(token => token)
				.ToDictionary(grouping => grouping.Key, grouping => grouping.Count());
			Console.Out.WriteLine("var date = '{0}';", DateTime.Now.ToString("dd.MM.yy"));
			Console.Out.WriteLine("var tokens = {{{0}}};", String.Join(", ", tokensDict.Select(pair => String.Format("'{0}': {1}", pair.Key, pair.Value))));
		}
	}
}