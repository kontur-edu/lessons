using System.Collections.Generic;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides.Blocks;

namespace Ulearn.Core.Model
{
	public class CommonSingleRegionExtractor : ISingleRegionExtractor
	{
		private readonly Dictionary<string, RegionsParser.Region> regions;
		private readonly string code;

		public CommonSingleRegionExtractor(string code)
		{
			this.code = code;
			regions = RegionsParser.GetRegions(code);
		}

		public string GetRegion(Label label, bool withoutAttributes=false)
		{
			var region = regions.GetOrDefault(label.Name, null);
			if (region == null)
				return null;
			return code.Substring(region.dataStart, region.dataLength).RemoveCommonNesting();
		}
	}
}