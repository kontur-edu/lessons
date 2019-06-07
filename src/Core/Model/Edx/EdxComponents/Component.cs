using System.Xml;

namespace Ulearn.Core.Model.Edx.EdxComponents
{
	public abstract class Component : EdxItem
	{
		public virtual XmlElement AsXmlElement()
		{
			var doc = new XmlDocument();
			doc.LoadXml("<p>" + AsHtmlString() + "</p>");
			return doc.DocumentElement;
		}

		public abstract EdxReference GetReference();
		public abstract string AsHtmlString();
	}
}