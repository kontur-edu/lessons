﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using uLearn.Extensions;

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
                return new SolutionStyleError(StyleErrorType.RefArguments01, parameter.GetFirstToken());
            }

            return methodDeclaration.ParameterList.Parameters
                .Where(ArgumentIsRef)
                .Select(it => (Parameter: it, TypeInfo: semanticModel.GetTypeInfo(it.Type)))
                .Where(it => !it.TypeInfo.IsPrimitive())
                .Select(it => GetErrorForParameter(it.Parameter));
        }
    }
}