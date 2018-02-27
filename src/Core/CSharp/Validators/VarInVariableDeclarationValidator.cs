using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp.Validators
{
    public class VarInVariableDeclarationValidator : BaseStyleValidator
    {
        protected override IEnumerable<string> ReportAllErrors(SyntaxTree userSolution, SemanticModel semanticModel)
        {
            return InspectAll<VariableDeclarationSyntax>(userSolution, semanticModel, Inspect);
        }

        private IEnumerable<string> Inspect(VariableDeclarationSyntax variableDeclarationSyntax, SemanticModel semanticModel)
        {
            if (variableDeclarationSyntax.Type.IsVar || variableDeclarationSyntax.Parent is FieldDeclarationSyntax)
                yield break;

            foreach (var variable in variableDeclarationSyntax.Variables)
            {
                if (variable.Initializer == null)
                    yield break;

                var initializerTypeInfo = semanticModel.GetTypeInfo(variable.Initializer.Value);
                var variableTypeInfo = semanticModel.GetTypeInfo(variableDeclarationSyntax.Type);

                if (Equals(initializerTypeInfo.Type, variableTypeInfo.Type))
                    yield return Report(variable, "Испульзуй var при инициализации локальной переменной");
            }
        }
    }
}