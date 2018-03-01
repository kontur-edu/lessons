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


        [TestCase("class A {void SomeMethod(){ string a = \"some string\";}}")]
        [TestCase("class A {void SomeMethod(){ bool a = true;}}")]
        [TestCase(@"class A {void SomeMethod(){ for (var i=0; i<10; ++i){for (int j=0; j<10; ++j){}}}}")]
        [TestCase(@"class A {void SomeMethod(){ List<string> c = new List<string>();}}")]
        public void Warn_declaration_with_type(string code)
        {
            var errors = validator.FindError(code);

            errors.Should().Contain("Строка 1");
        }


        [TestCase(@"class A {void SomeMethod(){ int b;}}")]
        [TestCase(@"class A {public int Value = 1;}")]
        [TestCase(@"class A {void SomeMethod(){ double b = 0;}}")]
        [TestCase(@"class A {void SomeMethod(){ int? i = 1; bool? flag = true;}}")]
        [TestCase(@"class A {void SomeMethod(){ var c = 0;}}")]
        [TestCase(@"class A {void SomeMethod(){ Exception a = new ArgumentNullException();}}")]
        [TestCase(@"class A {void SomeMethod(){ IList<string> c = new List<string>();}}")]
        [TestCase("class A {" +
                  "void SomeMethod(){string a = GetString();}" +
                  "private string GetString(){return \"abc\";}}")]
        public void ignore_correct_declaration(string code)
        {
            validator.FindError(code).Should().BeNullOrEmpty();
        }
    }
}