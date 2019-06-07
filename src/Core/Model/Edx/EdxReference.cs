using System.Xml.Serialization;
using Ulearn.Core.Model.Edx.EdxComponents;

namespace Ulearn.Core.Model.Edx
{
	public class EdxReference
	{
		[XmlAttribute("url_name")]
		public string UrlName;

		public virtual Component LoadComponent(string folderName, string urlName, EdxLoadOptions options)
		{
			return null;
		}
	}

	public class ChapterReference : EdxReference
	{
	}

	public class SequentialReference : EdxReference
	{
	}

	public class VerticalReference : EdxReference
	{
	}

	public class HtmlComponentReference : EdxReference
	{
		public override Component LoadComponent(string folderName, string urlName, EdxLoadOptions options)
		{
			return HtmlComponent.Load(folderName, urlName, options);
		}
	}

	public class LtiComponentReference : EdxReference
	{
		public override Component LoadComponent(string folderName, string urlName, EdxLoadOptions options)
		{
			return LtiComponent.Load(folderName, urlName, options);
		}
	}

	public class VideoComponentReference : EdxReference
	{
		public override Component LoadComponent(string folderName, string urlName, EdxLoadOptions options)
		{
			return VideoComponent.Load(folderName, urlName, options);
		}
	}

	public class ProblemComponentReference : EdxReference
	{
		public override Component LoadComponent(string folderName, string urlName, EdxLoadOptions options)
		{
			return SlideProblemComponent.Load(folderName, urlName, options);
		}
	}
}