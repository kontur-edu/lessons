using FluentAssertions;
using NUnit.Framework;

namespace uLearn.CSharp
{
    public class BracketValidator_should
    {
        private BracketValidator validator;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            validator = new BracketValidator();
        }

        [TestCase(@"int SomeMethod(){return (1);}")]
        [TestCase(@"bool SomeMethod(){return (true);}")]
        [TestCase(@"int SomeMethod(){return (-1);}")]
        [TestCase(@"bool SomeMethod(bool a, bool b){return ((a) && (b));}")]
        public void warn_return_statment_with_extra_brackets(string code)
        {
            FindErrors(code).Should().NotBeNullOrEmpty();
        }

        [TestCase(@"bool SetX(int x){ return true; }")]
        [TestCase(@"int SetX(int x, int y){ return (x+y)*2; }")]
        [TestCase(@"int SetX(int x, int y){ return (x+y)*x*(x-y); }")]
        [TestCase(@"int SetX(int x, int y){ return 2*(x+y); }")]
        [TestCase(@"int SetX(int x){ return 1; }")]
        [TestCase(@"void SetX(int x){ return; }")]
        public void ignore_correct_return_statment(string code)
        {
            FindErrors(code).Should().BeNullOrEmpty();
        }

        private string FindErrors(string code) =>
            validator.FindError(code);
    }
}