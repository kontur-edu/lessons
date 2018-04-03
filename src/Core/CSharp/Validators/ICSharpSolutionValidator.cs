using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace uLearn.CSharp.Validators
{
	public interface ICSharpSolutionValidator
	{
		[NotNull]
		List<SolutionStyleError> FindErrors(SyntaxTree userSolution, SemanticModel semanticModel);
	}
}