using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Ulearn.Core.Model.Edx.EdxComponents
{
	[XmlRoot("html")]
	public class GalleryComponent : Component
	{
		[XmlIgnore]
		public string[] Images;

		[XmlIgnore]
		public string LocalFolder;

		[XmlAttribute("filename")]
		public string Filename;

		[XmlIgnore]
		public override string SubfolderName => "html";

		public GalleryComponent()
		{
		}

		public GalleryComponent(string urlName, string displayName, string filename, string localFolder, string[] images)
		{
			UrlName = urlName;
			DisplayName = displayName;
			Filename = filename;
			LocalFolder = localFolder;
			Images = images;
		}

		public override void Save(string folderName)
		{
			base.Save(folderName);
			File.WriteAllText($"{folderName}/{SubfolderName}/{UrlName}.html", AsHtmlString());
		}

		public override void SaveAdditional(string folderName)
		{
			foreach (var image in Images)
				File.Copy($"{LocalFolder}/{image}", $"{folderName}/static/{UrlName}_{image.Replace("/", "_")}");
			File.WriteAllText($"{folderName}/static/gallery_{UrlName}.html",
				File.ReadAllText($"{Utils.GetRootDirectory()}/templates/gallery.html")
					.Replace("{0}", string.Join("", Images.Select(x => "<li><img src='" + UrlName + "_" + x.Replace("/", "_") + "' alt=''/></li>"))));
		}

		public override EdxReference GetReference()
		{
			return new HtmlComponentReference { UrlName = UrlName };
		}

		public override string AsHtmlString()
		{
			return File.ReadAllText($"{Utils.GetRootDirectory()}/templates/iframe.html")
				.Replace("{0}", "gallery_" + UrlName)
				.Replace("{1}", "(function (obj) { obj.style.height = '600px'; })(this);");
		}
	}
}