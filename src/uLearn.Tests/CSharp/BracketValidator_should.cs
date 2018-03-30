using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using uLearn.CSharp.Validators;

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
        public void warn_return_statement_with_extra_brackets(string code)
        {
            var errors = FindErrors(code);

            errors.Select(e => e.Span.StartLinePosition.Line).Should().Contain(0);
        }

        [TestCase(@"bool SetX(int x){ return true; }")]
        [TestCase(@"int SetX(int x, int y){ return (x+y)*2; }")]
        [TestCase(@"int SetX(int x, int y){ return (x+y)*x*(x-y); }")]
        [TestCase(@"int SetX(int x, int y){ return 2*(x+y); }")]
        [TestCase(@"int SetX(int x){ return 1; }")]
        [TestCase(@"void SetX(int x){ return; }")]
        public void ignore_correct_return_statement(string code)
        {
            FindErrors(code).Should().BeNullOrEmpty();
        }

        private List<SolutionStyleError> FindErrors(string code) =>
            validator.FindErrors(code);
    }
}