using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Ulearn.Core.Model.Edx.EdxComponents
{
	[XmlRoot("html")]
	public class HtmlComponent : Component
	{
		[XmlIgnore]
		public string Source;

		[XmlIgnore]
		public string LocalFolder;

		[XmlIgnore]
		public List<string> LocalFiles;

		[XmlAttribute("filename")]
		public string Filename;

		[XmlIgnore]
		public override string SubfolderName
		{
			get { return "html"; }
		}

		[XmlIgnore]
		public Component[] Subcomponents;

		public HtmlComponent()
		{
		}

		public HtmlComponent(string urlName, string displayName, string filename, string source)
		{
			UrlName = urlName;
			DisplayName = displayName;
			Filename = filename;
			Source = source;
		}

		public HtmlComponent(string urlName, string displayName, string filename, string source, string localFolder, List<string> localFiles)
		{
			UrlName = urlName;
			DisplayName = displayName;
			Filename = filename;
			Source = source;
			LocalFolder = localFolder;
			LocalFiles = localFiles;
		}

		public override void Save(string folderName)
		{
			base.Save(folderName);
			File.WriteAllText(string.Format("{0}/{1}/{2}.html", folderName, SubfolderName, UrlName), Source);
		}

		public override void SaveAdditional(string folderName)
		{
			if (Subcomponents != null)
				foreach (var subcomponent in Subcomponents)
					subcomponent.SaveAdditional(folderName);
			try
			{
				foreach (var localFile in LocalFiles ?? new List<string>())
					File.Copy(
						Path.Combine(LocalFolder, localFile),
						string.Format("{0}/static/{1}_{2}", folderName, UrlName, localFile.Replace("/", "_")),
						overwrite: true);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public override EdxReference GetReference()
		{
			return new HtmlComponentReference { UrlName = UrlName };
		}

		public override string AsHtmlString()
		{
			return Source;
		}

		public static HtmlComponent Load(string folderName, string urlName, EdxLoadOptions options)
		{
			return Load<HtmlComponent>(folderName, "html", urlName, options,
				c => { c.Source = File.ReadAllText(string.Format("{0}/html/{1}.html", folderName, c.Filename)); });
		}
	}
}