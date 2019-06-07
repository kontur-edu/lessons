using System.Collections.Generic;
using System.Linq;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.CSharp;

namespace Ulearn.Core.Model
{
	public interface ISingleRegionExtractor
	{
		string GetRegion(Label label, bool withoutAttributes);
	}

	public class RegionsExtractor
	{
		private readonly List<ISingleRegionExtractor> extractors;
		public readonly string Filename;
		public readonly Language? Language;

		public RegionsExtractor(string code, Language? language, string filename= null)
		{
			Filename = filename;
			Language = language;
			
			extractors = new List<ISingleRegionExtractor>
			{
				new CommonSingleRegionExtractor(code)
			};
			
			if (language == Core.Language.CSharp)
				extractors.Add(new CsMembersExtractor(code));
		}

		public string GetRegion(Label label, bool withoutAttributes=false)
		{
			return extractors.Select(extractor => extractor.GetRegion(label, withoutAttributes)).FirstOrDefault(res => res != null);
		}

		public IEnumerable<string> GetRegions(IEnumerable<Label> labels, bool withoutAttributes=false)
		{
			return labels.Select(l => GetRegion(l, withoutAttributes)).Where(s => s != null).Select(s => s.FixExtraEolns());
		}
	}
}