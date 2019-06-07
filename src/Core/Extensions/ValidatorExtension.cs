using System.Linq;

namespace Ulearn.Core.Extensions
{
	internal static class ValidatorExtension
	{
		public static SolutionBuildResult ValidateSolution(this ISolutionValidator validator, string userWrittenCode, string fullCodeFile)
		{
			string message;

			if ((message = validator.FindSyntaxError(fullCodeFile)) != null ||
				(message = validator.FindStrictValidatorErrors(userWrittenCode, fullCodeFile)) != null)
			{
				return new SolutionBuildResult(fullCodeFile, message);
			}

			var styleErrors = validator.FindValidatorErrors(userWrittenCode, fullCodeFile);
			if (styleErrors.Any())
			{
				return new SolutionBuildResult(fullCodeFile, styleErrors: styleErrors);
			}

			return new SolutionBuildResult(fullCodeFile);
		}

		public static SolutionBuildResult ValidateSingleFileSolution(this ISolutionValidator validator, string userWrittenCode, string fullCodeFile)
		{
			string message;
			if ((message = validator.FindFullSourceError(userWrittenCode)) != null)
				return new SolutionBuildResult(fullCodeFile, message);
			return validator.ValidateSolution(userWrittenCode, fullCodeFile);
		}
	}
}