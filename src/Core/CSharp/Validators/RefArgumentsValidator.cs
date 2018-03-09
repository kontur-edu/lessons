using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp.Validators
{
	public class RefArgumentsValidator : BaseStyleValidator
	{
		public override List<SolutionStyleError> FindErrors(SyntaxTree userSolution,
			SemanticModel semanticModel)
		{
			var validationResult = userSolution.GetRoot()
				.DescendantNodes()
				.Where(node => node.Kind() == SyntaxKind.MethodDeclaration)
				.Cast<MethodDeclarationSyntax>()
				.Where(MethodHasRefParameter)
				.SelectMany(t => ValidateRefs(t, semanticModel))
				.ToList();

			return validationResult;
		}

		private static bool MethodHasRefParameter(MethodDeclarationSyntax methodDeclaration)
		{
			return methodDeclaration.ParameterList.Parameters.Any(ArgumentIsRef);
		}

		private static bool ArgumentIsRef(ParameterSyntax parameterSyntax)
		{
			return parameterSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.RefKeyword));
		}

		private IEnumerable<SolutionStyleError> ValidateRefs(
			BaseMethodDeclarationSyntax methodDeclaration,
			SemanticModel semanticModel)
		{
			SolutionStyleError GetErrorForParameter(ParameterSyntax parameter)
			{
				return new SolutionStyleError(parameter.GetFirstToken(), $"Разрешено передавать через `ref` только примитивные типы.");
			}

			return methodDeclaration.ParameterList.Parameters
				.Where(ArgumentIsRef)
				.Select(it => (Parameter: it, TypeInfo: semanticModel.GetTypeInfo(it.Type)))
				.Where(it => !IsPrimitive(it.TypeInfo.Type.SpecialType))
				.Select(it => GetErrorForParameter(it.Parameter));
		}

		private static bool IsPrimitive(SpecialType specialType)
		{
			switch (specialType)
			{
				case SpecialType.System_Boolean:
				case SpecialType.System_Byte:
				case SpecialType.System_SByte:
				case SpecialType.System_Int16:
				case SpecialType.System_UInt16:
				case SpecialType.System_Int32:
				case SpecialType.System_UInt32:
				case SpecialType.System_Int64:
				case SpecialType.System_UInt64:
				case SpecialType.System_IntPtr:
				case SpecialType.System_UIntPtr:
				case SpecialType.System_Char:
				case SpecialType.System_Double:
				case SpecialType.System_Single:
					return true;
			}
			return false;
		}
	}
}