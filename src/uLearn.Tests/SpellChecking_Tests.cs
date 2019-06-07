using System;
using System.Linq;
using NUnit.Framework;
using Ulearn.Common.Extensions;
using Ulearn.Core.Properties;
using Ulearn.Core.SpellChecking;

namespace uLearn
{
	class SpellChecking_Tests
	{
		[Test]
		public void Test()
		{
			using (var spellChecker = new SpellChecker())
			{
				CollectionAssert.IsEmpty(spellChecker.SpellCheckString("привет"));
				CollectionAssert.IsNotEmpty(spellChecker.SpellCheckString("превет"));
				var dictionaryLines = Resources.customDictionary.SplitToLines();
				var errors = dictionaryLines.SelectMany(spellChecker.SpellCheckString).ToList();
				Console.WriteLine(string.Join("\r\n", errors));
				CollectionAssert.IsEmpty(errors);
			}
		}
	}
}