using System.Xml.Serialization;

namespace Ulearn.Core.Courses.Slides.Blocks
{
	///<summary>Отметка в коде</summary>
	public class Label
	{
		[XmlText]
		public string Name { get; set; }

		[XmlAttribute("onlyBody")]
		public bool OnlyBody { get; set; }
	}
}