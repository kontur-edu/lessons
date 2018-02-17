using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp.Validators
{
    public class VarInVariableDeclarationValidator : BaseStyleValidator
    {
        protected override IEnumerable<string> ReportAllErrors(SyntaxTree userSolution, SemanticModel semanticModel)
        {
            return InspectAll<VariableDeclarationSyntax>(userSolution, semanticModel, InspectMethodsNames);
        }

        private IEnumerable<string> InspectMethodsNames(VariableDeclarationSyntax variableDeclarationSyntax, SemanticModel semanticModel)
        {
            if (variableDeclarationSyntax.Type.IsVar)
                yield break;

            foreach (var variable in variableDeclarationSyntax.Variables)
            {
                if (variable.Initializer != null)
                    yield return Report(variable, "Испульзуй var при явной инициализации переменой");
            }
        }
    }
}