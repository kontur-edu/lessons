using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace uLearn.CSharp.Validators
{
	public class NamingCaseStyleValidator : BaseNamingChecker
	{
		protected override IEnumerable<SolutionStyleError> InspectName(SyntaxToken identifier)
		{
			var name = identifier.Text;
			if (string.IsNullOrEmpty(name))
				yield break;
			var mustStartWithUpper = MustStartWithUpper(identifier.Parent);
			var mustStartWithLower = MustStartWithLower(identifier.Parent);
			var isUpper = char.IsUpper(name[0]);
			var isLower = char.IsLower(name[0]);

			if (mustStartWithLower && !isLower)
				yield return new SolutionStyleError(StyleErrorType.NamingCase01, identifier);
			if (mustStartWithUpper && !isUpper)
				yield return new SolutionStyleError(StyleErrorType.NamingCase02, identifier);
		}

		private bool MustStartWithUpper(SyntaxNode node)
		{
			return
				node is BaseTypeDeclarationSyntax
				|| node is TypeParameterSyntax
				|| node is EnumMemberDeclarationSyntax
				|| node is MethodDeclarationSyntax && ((MethodDeclarationSyntax)node).Modifiers.Any(t => t.Kind() == SyntaxKind.PublicKeyword)
				|| node is VariableDeclaratorSyntax && MustStartWithUpper((VariableDeclaratorSyntax)node);
		}

		private bool MustStartWithUpper(VariableDeclaratorSyntax variableDeclarator)
		{
			var field = AsField(variableDeclarator);
			if (field == null) return false;
			// Публичные поля и константы → с большой
			bool isStatic = field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
			bool isReadonly = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));
			bool isConstant = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword));
			bool isPublic = field.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
			// статические ридонли могут быть какие угодно.
			return (isPublic || isConstant) && !(isStatic && isReadonly);
		}

		private bool MustStartWithLower(VariableDeclaratorSyntax variableDeclarator)
		{
			var field = AsField(variableDeclarator);
			if (field == null) return true;
			bool isStatic = field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
			bool isReadonly = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));
			bool isConstant = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword));
			bool isPrivate = field.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)) || 
							!field.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword) || m.IsKind(SyntaxKind.InternalKeyword) || m.IsKind(SyntaxKind.ProtectedKeyword));
			// статические ридонли могут быть какие угодно.
			return isPrivate && !isConstant && !(isStatic && isReadonly);
		}

		private static BaseFieldDeclarationSyntax AsField(VariableDeclaratorSyntax variableDeclarator)
		{
			// Первый родитель, но не выше блока.
			var parent = variableDeclarator.GetParents().FirstOrDefault(p => (p is BaseFieldDeclarationSyntax) || (p is BlockSyntax));
			return parent as BaseFieldDeclarationSyntax;
		}

		private bool MustStartWithLower(SyntaxNode node)
		{
			return
				node is ParameterSyntax
				|| node is VariableDeclaratorSyntax && MustStartWithLower((VariableDeclaratorSyntax)node);
		}
	}
}