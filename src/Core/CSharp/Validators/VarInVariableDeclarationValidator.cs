using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp.Validators
{
    public class VarInVariableDeclarationValidator : BaseStyleValidator
    {
		public override List<SolutionStyleError> FindErrors(SyntaxTree userSolution, SemanticModel semanticModel)
        {
            return InspectAll<VariableDeclarationSyntax>(userSolution, semanticModel, Inspect).ToList();
        }

        private IEnumerable<SolutionStyleError> Inspect(VariableDeclarationSyntax variableDeclarationSyntax, SemanticModel semanticModel)
        {
            if (variableDeclarationSyntax.Type.IsVar || variableDeclarationSyntax.Parent is FieldDeclarationSyntax)
                yield break;

            foreach (var variable in variableDeclarationSyntax.Variables)
            {
                if (variable.Initializer == null)
                    yield break;

                var value = variable.Initializer.Value;
                if (!(value is LiteralExpressionSyntax) && !(value is ObjectCreationExpressionSyntax))
                    continue;

                var initializerTypeInfo = semanticModel.GetTypeInfo(variable.Initializer.Value);
                var variableTypeInfo = semanticModel.GetTypeInfo(variableDeclarationSyntax.Type);

                if (Equals(initializerTypeInfo.Type, variableTypeInfo.Type))
                    yield return new SolutionStyleError(variable, "Используйте `var` при инициализации локальной переменной.");
            }
        }
    }
}