using System.Collections.Generic;
using NUnit.Framework;
using Ulearn.Common.Extensions;
using Ulearn.Core;

namespace uLearn
{
	[TestFixture]
	public class RegionsParser_should
	{
		[Test]
		public void EmptyCode()
		{
			CollectionAssert.IsEmpty(RegionsParser.GetRegions(""));
		}

		[Test]
		public void SimpleParsing()
		{
			const string code = "region a\nendregion a";
			var regions = RegionsParser.GetRegions(code);
			var expected = new Dictionary<string, RegionsParser.Region>
			{
				{ "a", new RegionsParser.Region(9, 0, 0, code.Length) }
			};
			CollectionAssert.AreEquivalent(expected, regions);
		}
		
		[Test]
		public void SimpleParsingWithLongRegionName()
		{
			const string code = "region abcdef\nendregion abcdef";
			var regions = RegionsParser.GetRegions(code);
			var expected = new Dictionary<string, RegionsParser.Region>
			{
				{ "abcdef", new RegionsParser.Region(14, 0, 0, code.Length) }
			};
			CollectionAssert.AreEquivalent(expected, regions);
		}

		[Test]
		public void Errors()
		{
			CollectionAssert.IsEmpty(RegionsParser.GetRegions("region a"));
			CollectionAssert.IsEmpty(RegionsParser.GetRegions("endregion a"));
			CollectionAssert.IsEmpty(RegionsParser.GetRegions("endregion a\nregion a"));
			CollectionAssert.IsEmpty(RegionsParser.GetRegions("region a\nendregion b"));
		}

		[Test]
		public void LeadingWhiteSpaces()
		{
			const string code = "\tregion a\n\tendregion a";
			var regions = RegionsParser.GetRegions(code);
			var expected = new Dictionary<string, RegionsParser.Region>
			{
				{ "a", new RegionsParser.Region(10, 0, 0, code.Length) }
			};
			CollectionAssert.AreEquivalent(expected, regions);
		}

		[Test]
		public void ExtraData()
		{
			const string code = "aaa\r\n\taaa region a\r\nbb\r\nccc endregion  a \r\nddd";
			var regions = RegionsParser.GetRegions(code);
			var expected = new Dictionary<string, RegionsParser.Region>
			{
				{ "a", new RegionsParser.Region(20, 4, 5, 38) }
			};
			CollectionAssert.AreEquivalent(expected, regions);
		}

		[Test]
		public void MultipleRegion()
		{
			const string code = "region a\nendregion a\nregion a\nendregion a";
			var regions = RegionsParser.GetRegions(code);
			var expected = new Dictionary<string, RegionsParser.Region>
			{
				{ "a", new RegionsParser.Region(30, 0, 21, 20) }
			};
			CollectionAssert.AreEquivalent(expected, regions);
		}

		[Test]
		public void ManyRegions()
		{
			const string code = @"
				endregion error1
				region a
				region b
				endregion a
				region c
				endregion b
				endregion c
				region error2
			";
			var regions = RegionsParser.GetRegions(string.Join("\r\n", code.SplitToLines()));
			var expected = new Dictionary<string, RegionsParser.Region>
			{
				{ "a", new RegionsParser.Region(38, 14, 24, 45) },
				{ "b", new RegionsParser.Region(52, 31, 38, 62) },
				{ "c", new RegionsParser.Region(83, 17, 69, 48) },
			};
			CollectionAssert.AreEquivalent(expected, regions);
		}
	}
}