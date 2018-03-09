using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using uLearn.CSharp.Validators.IndentsValidation;
using uLearn.CSharp.Validators.VerbInMethodNameValidation;

namespace uLearn.CSharp.Validators
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
			new ArrayLengthStyleValidator(),
			new ExcessLinesValidator(),
			new RefArgumentsValidator(),
			new VarInVariableDeclarationValidator(),
			new IndentsValidator()
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

		public List<SolutionStyleError> FindValidatorErrors(string userCode, string solution)
		{
			var solutionTree = CSharpSyntaxTree.ParseText(userCode);
			try
			{
				var compilation = CSharpCompilation.Create("MyCompilation", new[] { solutionTree }, new[] { mscorlib });
				var semanticModel = compilation.GetSemanticModel(solutionTree);
				return validators
					.Where(v => !(v is IStrictValidator))
					.SelectMany(v => v.FindErrors(solutionTree, semanticModel))
					.ToList();
			}
			catch (Exception e)
			{
				return new List<SolutionStyleError> { new SolutionStyleError(solutionTree.GetRoot(), e.Message)};
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
					.SelectMany(v => v.FindErrors(solutionTree, semanticModel))
					.FirstOrDefault()
					?.Message;
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