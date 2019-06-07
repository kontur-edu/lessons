using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Model.Edx.EdxComponents;

namespace Ulearn.Core.Courses.Slides.Quizzes.Blocks
{
	[XmlType("question.match")]
	public class MatchingBlock : AbstractQuestionBlock
	{
		[XmlAttribute("shuffleFixed")]
		public bool ShuffleFixed;

		[XmlAttribute("explanation")]
		public string Explanation;

		[XmlElement("match")]
		public MatchingMatch[] Matches;

		private readonly Random random = new Random();

		public List<MatchingMatch> GetMatches(bool shuffle = false)
		{
			if (shuffle)
				return Matches.Shuffle(random).ToList();

			return Matches.ToList();
		}

		public override Component ToEdxComponent(string displayName, string courseId, Slide slide, int componentIndex, string ulearnBaseUrl, DirectoryInfo coursePackageRoot)
		{
			throw new NotSupportedException();
		}

		public override bool HasEqualStructureWith(SlideBlock other)
		{
			var block = other as MatchingBlock;
			if (block == null)
				return false;
			if (Matches.Length != block.Matches.Length)
				return false;
			var idsSet = Matches.Select(m => m.Id).ToImmutableHashSet();
			var blockIdsSet = block.Matches.Select(m => m.Id).ToImmutableHashSet();
			return idsSet.SetEquals(blockIdsSet);
		}
	}
}