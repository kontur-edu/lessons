using NUnit.Framework;
using Ulearn.Common.Extensions;

namespace uLearn
{
	[TestFixture]
	public class StringExtensions_should
	{
		[Test]
		public void not_produce_extra_lines()
		{
			Assert.That("hello\r\nworld".SplitToLines(), Is.EqualTo(new[] { "hello", "world" }).AsCollection);
		}

		[Test]
		public void split_text_with_unix_line_endings()
		{
			Assert.That("hello\nworld".SplitToLines(), Is.EqualTo(new[] { "hello", "world" }).AsCollection);
		}

		[Test]
		public void not_produce_extra_lines_with_eoln()
		{
			Assert.That("hello\r\nworld\r\n".SplitToLinesWithEoln(), Is.EqualTo(new[] { "hello\r\n", "world\r\n" }).AsCollection);
		}

		[Test]
		public void split_text_with_unix_line_endings_with_eoln()
		{
			Assert.That("hello\nworld\n".SplitToLinesWithEoln(), Is.EqualTo(new[] { "hello\n", "world\n" }).AsCollection);
		}
		
		[Test]
		public void left_empty_lines()
		{
			Assert.That("hello\r\n\r\nworld\r\n".SplitToLinesWithEoln(), Is.EqualTo(new[] { "hello\r\n", "\r\n", "world\r\n" }).AsCollection);
		}
		
		[Test]
		public void left_empty_lines_with_unix_line_endings()
		{
			Assert.That("hello\n\nworld\n".SplitToLinesWithEoln(), Is.EqualTo(new[] { "hello\n", "\n", "world\n" }).AsCollection);
		}

		[Test]
		public void test_remove_extra_eolns()
		{
			Assert.AreEqual("", "\r\n\r\n".FixExtraEolns());
			Assert.AreEqual("Hello\r\n\r\nworld", "Hello\r\n\r\nworld".FixExtraEolns());
			Assert.AreEqual("Hello\r\n\r\nworld", "Hello\r\n\r\n\r\nworld".FixExtraEolns());
			Assert.AreEqual("Hello\r\n\r\n\tworld", "\tHello\r\n\t\r\n\t\r\n\tworld".FixExtraEolns());
			Assert.AreEqual("Hello\r\n\r\nworld\nHello", "Hello\r\n\r\n\r\nworld\nHello".FixExtraEolns());
			Assert.AreEqual("Hello\r\n\r\nworld\r\n\r\nworld\r\n\r\n\tworld", "Hello\r\n\t\r\n\t\t\r\nworld\r\n\t\r\n\t\r\nworld\r\n\t\r\n\t\t\r\n\tworld".FixExtraEolns());
		}
	}
}