using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Ulearn.Core.CSharp;
using Ulearn.Core.CSharp.Validators.VerbInMethodNameValidation;

namespace uLearn.CSharp
{
	[TestFixture]
	public class VerbInMethodNameValidator_should
	{
		private VerbInMethodNameValidator validator;
		
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			validator = new VerbInMethodNameValidator();
		}
		
		[TestCase(@"void SomeMethod(int a){}")]
		[TestCase(@"void A(string s){}")]
		[TestCase(@"public int DrawingPriority() => 0;")]
		[TestCase(@"public int getdrawingpriority() => 0;")]
		public void warn_method_name_without_verb(string code)
		{
			FindErrors(code).Should().NotBeNullOrEmpty();
		}

		[TestCase(@"bool SetX(int x){ return true; }")]
		[TestCase(@"int GetX(){ return 1; }")]
		[TestCase(@"bool IsCorrectMethod(){ return true; }")]
		[TestCase(@"public int GetDrawingPriority() => 0;")]
		[TestCase(@"public int getDrawingPriority() => 0;")]
		[TestCase(@"public void Dispose(){}")]
		[TestCase(@"public int ToInt(){}")]
		[TestCase(@"public string FromInt(){}")]
		[TestCase(@"public static void With(){}")]
		[TestCase(@"public static void DoYourThing(){}")]
		public void ignore_correct_method_name(string code)
		{
			FindErrors(code).Should().BeNullOrEmpty();
		}

		[TestCase(@"public static Rational operator *(Rational r){}")]
		[TestCase(@"public static Rational operator +(Rational r){}")]
		[TestCase(@"public static Rational operator double(Rational r){}")]
		[TestCase(@"public static Rational operator int(Rational r){}")]
		public void ignore_operator(string code)
		{
			FindErrors(code).Should().BeNullOrEmpty();
		}

		[TestCase(@"public class SomeClass{public SomeClass(){ }}")]
		[TestCase(@"public class Point{public Point(){ }}")]
		public void ignore_constructors(string code)
		{
			FindErrors(code).Should().BeNullOrEmpty();
		}

		private List<SolutionStyleError> FindErrors(string code) => validator.FindErrors(code);
	}
}