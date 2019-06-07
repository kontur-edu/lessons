using System.Xml.Serialization;
using Ulearn.Common.Extensions;

namespace Ulearn.Core.Courses.Slides.Quizzes.Blocks
{
	public class OrderingItem
	{
		[XmlAttribute("id")]
		public string Id;

		[XmlText]
		public string Text;

		public string GetHash()
		{
			return (Id + "OrderingItemSalt").GetStableHashCode().ToString();
		}
	}
}