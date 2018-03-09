using System.IO;
using System.Xml.Serialization;
using uLearn.Configuration;

namespace uLearn.Model
{
	public class SlideMetaDescription
	{
		[XmlElement("image")]
		// ReSharper disable once InconsistentNaming
		public string _image { get; set; }
		
		[XmlIgnore]
		public string Image { get; set; }

		[XmlElement("keywords")]
		public string Keywords { get; set; }
		
		[XmlElement("description")]
		public string Description { get; set; }

		public void FixPaths(FileInfo slideFile)
		{
			if (string.IsNullOrEmpty(_image))
				return;
			
			var relativeUrl = CourseUnitUtils.GetDirectoryRelativeWebPath(slideFile);
			var imagePath = Path.Combine(relativeUrl, _image);
			var configuration = ApplicationConfiguration.Read<UlearnConfiguration>();

			Image = configuration.BaseUrl + imagePath;
		}
	}
}