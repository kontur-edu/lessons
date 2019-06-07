using System.Xml.Serialization;
using Ulearn.Common.Extensions;

namespace Ulearn.Core.Courses.Slides.Quizzes.Blocks
{
	public class MatchingMatch
	{
		[XmlAttribute("id")]
		public string Id;

		[XmlElement("fixed")]
		public string FixedItem;

		[XmlElement("movable")]
		public string MovableItem;

		public string GetHashForFixedItem()
		{
			return (Id + "MatchingItemFixedItemSalt").GetStableHashCode().ToString();
		}

		public string GetHashForMovableItem()
		{
			return (Id + "MatchingItemMovableItemSalt").GetStableHashCode().ToString();
		}
	}
}