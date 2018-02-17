using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using uLearn.CSharp.VerbInMethodNameValidation;

namespace uLearn.CSharp
{
	public class CSharpSolutionValidator : ISolutionValidator
	{
		private readonly List<ICSharpSolutionValidator> validators = new List<ICSharpSolutionValidator>
		{
			new NotEmptyCodeValidator(),
			new BlockLengthStyleValidator(),
			new LineLengthStyleValidator(),
			new NamingCaseStyleValidator(),
			new VerbInMethodNameValidator(),
			new RedundantIfStyleValidator(),
			new NamingStyleValidator(),
			new ExponentiationValidator(),
			new BoolCompareValidator(),
			new IndentsValidator() // Выводит дополнительный текст в конце, поэтому лучше ему быть последним
		};

		public CSharpSolutionValidator(bool removeDefaults=false)
		{
			if (removeDefaults)
				validators.Clear();
		}

		public void AddValidator(ICSharpSolutionValidator validator)
		{
			validators.RemoveAll(item => item.GetType() == validator.GetType());
			validators.Add(validator);
		}

		public string FindFullSourceError(string userCode)
		{
			userCode = userCode.Trim();
			if (userCode.StartsWith("using") || userCode.StartsWith("namespace"))
				return "Не нужно писать весь исходный файл целиком — пишите только метод / класс, который необходим в задаче.";
			return null;
		}

		public string FindSyntaxError(string solution)
		{
			IEnumerable<Diagnostic> diagnostics = CSharpSyntaxTree.ParseText(solution).GetDiagnostics();
			var error = diagnostics.FirstOrDefault();
			return error?.ToString();
		}

		public string FindValidatorErrors(string userCode, string solution)
		{
			try
			{
				var solutionTree = CSharpSyntaxTree.ParseText(userCode);
				var compilation = CSharpCompilation.Create("MyCompilation", new[] { solutionTree }, new[] { mscorlib });
				var semanticModel = compilation.GetSemanticModel(solutionTree);
				var errors = validators
					.Where(v => !(v is IStrictValidator))
					.Select(v => v.FindError(solutionTree, semanticModel))
					.Where(err => err != null)
					.ToArray();
				return errors.Any() ? string.Join("\n\n", errors) : null;
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}

		public string FindStrictValidatorErrors(string userCode, string solution)
		{
			try
			{
				var solutionTree = CSharpSyntaxTree.ParseText(userCode);
				var compilation = CSharpCompilation.Create("MyCompilation", new[] { solutionTree }, new[] { mscorlib });
				var semanticModel = compilation.GetSemanticModel(solutionTree);
				return validators
					.Where(v => v is IStrictValidator)
					.Select(v => v.FindError(solutionTree, semanticModel))
					.FirstOrDefault(err => err != null);
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}

		private static readonly PortableExecutableReference mscorlib =
			MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
	}
}