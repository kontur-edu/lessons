using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ulearn.Core.CSharp;
using Ulearn.Core.CSharp.Validators;

namespace uLearn.CSharp
{
	[TestFixture]
	public class RedundantIfStyleValidator_should
	{
		[Test]
		public void ignore_when_no_if()
		{
			var errors = FindErrors(@"bool A(){ return true; }");
			Assert.That(errors, Is.Empty);
		}

		[Test]
		public void warn_simple_if()
		{
			var errors = FindErrors(@"bool A(){ if (true) return true; else return false; }");
			Assert.That(errors, Is.Not.Empty);
		}

		[Test]
		public void ignore_if_without_else()
		{
			var errors = FindErrors(@"bool A(){ if (true) return true; Console.WriteLine(42); return false; }");
			Assert.That(errors, Is.Empty);
		}

		[Test]
		public void ignore_if_with_non_bool_return()
		{
			var errors = FindErrors(@"int A(){ if (true) return 1; else return 2; }");
			Assert.That(errors, Is.Empty);
		}

		[Test]
		public void warn_if_with_folowing_return()
		{
			var errors = FindErrors(@"bool A(){ if (true) return true; return false; }");
			Assert.That(errors, Is.Not.Empty);
		}

		[Test]
		public void ignore_if_with_complex_then()
		{
			var errors = FindErrors(@"bool A(){ if (true) return f(); else return false; }");
			Assert.That(errors, Is.Empty);
		}

		[Test]
		public void ignore_if_with_complex_else()
		{
			var errors = FindErrors(@"bool A(){ if (true) return true; else return variable; }");
			Assert.That(errors, Is.Empty);
		}

		[Test]
		public void ignore_if_without_return()
		{
			var errors = FindErrors(@"void A(){ if (true) a=1; else a=2; }");
			Assert.That(errors, Is.Empty);
		}

		[Test]
		public void find_all_warnings()
		{
			var errors = FindErrors(@"
void A(){ if (true) return true; else return false; }
void B(){ if (false) return true; return false; }
");
			var lines = errors.Select(e => e.Span.StartLinePosition.Line).ToList();
			Assert.That(lines, Does.Contain(1));
			Assert.That(lines, Does.Contain(2));
		}

		private static List<SolutionStyleError> FindErrors(string code)
		{
			return new RedundantIfStyleValidator().FindErrors(code);
		}
	}
}