using FluentAssertions;
using NUnit.Framework;
using uLearn.CSharp.Validators;

namespace uLearn.CSharp
{
    [TestFixture]
    public class VarInVariableDeclarationValidator_should
    {
        private VarInVariableDeclarationValidator validator;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            validator = new VarInVariableDeclarationValidator();
        }

        
        [TestCase(@"class A {void SomeMethod(){ int a = 0;}}")]
        [TestCase(@"class A {void SomeMethod(){ for (int i=0; i<10; ++i){}}}")]
        [TestCase(@"class A {void SomeMethod(){ for (var i=0; i<10; ++i){for (int j=0; j<10; ++j){}}}}")]
        public void Warn_declaration_with_type(string code)
        {
            var errors = validator.FindError(code);

            errors.Should().Contain("Строка 1");
        }


        [TestCase(@"class A {void SomeMethod(){ int b;}}")]
        [TestCase(@"class A {void SomeMethod(){ var c = 0;}}")]
        [TestCase(@"class A {void SomeMethod(){ for (var i=0; i<10; ++i){}}}")]
        public void ignore_correct_declaration(string code)
        {
            validator.FindError(code).Should().BeNullOrEmpty();
        }
    }
}
