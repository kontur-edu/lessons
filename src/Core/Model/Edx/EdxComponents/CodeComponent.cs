using System.IO;
using System.Xml.Serialization;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;

namespace Ulearn.Core.Model.Edx.EdxComponents
{
	[XmlRoot("html")]
	public class CodeComponent : Component
	{
		[XmlIgnore]
		public string Source;

		[XmlIgnore]
		public Language Language;

		[XmlAttribute("filename")]
		public string Filename;

		[XmlIgnore]
		public override string SubfolderName => "html";

		public CodeComponent()
		{
		}

		public CodeComponent(string urlName, string displayName, string filename, Language language, string source)
		{
			UrlName = urlName;
			DisplayName = displayName;
			Filename = filename;
			Language = language;
			Source = source;
		}

		public override void Save(string folderName)
		{
			base.Save(folderName);
			File.WriteAllText($"{folderName}/{SubfolderName}/{UrlName}.html", AsHtmlString());
		}

		public override void SaveAdditional(string folderName)
		{
			File.WriteAllText(
				$"{folderName}/static/code_{UrlName}.html",
				File.ReadAllText($"{Utils.GetRootDirectory()}/templates/code.html").Replace("{0}", Language.GetXmlEnumName()).Replace("{1}", Source)
			);
		}

		public override EdxReference GetReference()
		{
			return new HtmlComponentReference { UrlName = UrlName };
		}

		public override string AsHtmlString()
		{
			return File.ReadAllText($"{Utils.GetRootDirectory()}/templates/iframe.html")
				.Replace("{0}", "code_" + UrlName)
				.Replace("{1}", "(function (obj) { obj.style.height = obj.contentWindow.document.documentElement.scrollHeight + 'px'; })(this);");
		}
	}
}