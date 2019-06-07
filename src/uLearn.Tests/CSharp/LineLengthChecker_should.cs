using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ulearn.Core.CSharp;
using Ulearn.Core.CSharp.Validators;

namespace uLearn.CSharp
{
	[TestFixture]
	public class LineLengthChecker_should
	{
		[TestCase("protected override void getX(ref int x) {}")]
		[TestCase("01234567891")]
		[TestCase("										1")]
		public void Warn_LongLines(string code)
		{
			CheckIncorrect(code, "длинная");
		}

		[TestCase("0123456789")]
		[TestCase("int a=142;")]
		[TestCase("								1;")]
		public void Pass_ShortLines(string code)
		{
			CheckCorrect(code);
		}

		private static void CheckCorrect(string code)
		{
			Assert.That(FindErrors(code), Is.Empty, code);
		}

		private void CheckIncorrect(string incorrectCode, string messageSubstring)
		{
			var errors = FindErrors(incorrectCode);
			Assert.That(errors.Select(e => e.Span.StartLinePosition.Line), Does.Contain(0), incorrectCode);
			errors.ForEach(e => Assert.That(e.Message, Does.Contain(messageSubstring)));
		}

		private static List<SolutionStyleError> FindErrors(string code)
		{
			return new LineLengthStyleValidator(10).FindErrors(code);
		}
	}
}