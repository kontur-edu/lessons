﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Ulearn.Common.Extensions;
using Ulearn.Core.Properties;

namespace Ulearn.Core.CSharp.Validators.VerbInMethodNameValidation
{
    public class VerbInMethodNameValidator : BaseStyleValidator
    {
        private readonly HashSet<string> englishVerbs;

        public VerbInMethodNameValidator(string[] exceptions = null)
		{
			exceptions = exceptions ?? new string[0];
            exceptionsMethodNames = new HashSet<string>(exceptions.Concat(new[] { "Main" }), StringComparer.InvariantCultureIgnoreCase);
            exceptionsPreposition = new HashSet<string>(new[] { "For", "To", "With", "From", "At" }, StringComparer.InvariantCultureIgnoreCase);
            englishVerbs = new HashSet<string>(Resources.englishVerbs.SplitToLines(), StringComparer.InvariantCultureIgnoreCase);
        }

        public override List<SolutionStyleError> FindErrors(SyntaxTree userSolution, SemanticModel semanticModel)
        {
            return InspectAll<MethodDeclarationSyntax>(userSolution, InspectMethodsNames).ToList();
        }

        private IEnumerable<SolutionStyleError> InspectMethodsNames(MethodDeclarationSyntax methodDeclarationSyntax)
        {
            var syntaxToken = methodDeclarationSyntax.Identifier();
            if (exceptionsMethodNames.Contains(syntaxToken.ValueText))
                yield break;

            var wordsInName = SplitMethodName(syntaxToken.ValueText).ToList();

            if (exceptionsPreposition.Contains(wordsInName.First()))
                yield break;

            foreach (var word in wordsInName)
            {
                if (englishVerbs.Contains(word))
                    yield break;
            }

			yield return new SolutionStyleError(StyleErrorType.VerbInMethod01, syntaxToken);
        }

        private IEnumerable<string> SplitMethodName(string methodName)
        {
            var word = "";
            foreach (var letter in methodName)
            {
                if ((char.IsUpper(letter) || !char.IsLetter(letter)) && word != "")
                {
                    yield return word;
                    word = "";
                }
				if (char.IsLetter(letter))
					word += letter;
            }
            yield return word;
        }

        private readonly HashSet<string> exceptionsMethodNames;
        private readonly HashSet<string> exceptionsPreposition;
    }
}