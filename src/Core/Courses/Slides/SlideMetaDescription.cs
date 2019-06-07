using System;
using System.IO;
using System.Xml.Serialization;
using Ulearn.Core.Configuration;

namespace Ulearn.Core.Courses.Slides
{
	public class SlideMetaDescription
	{
		[XmlElement("image")]
		public string Image { get; set; }
		
		[XmlIgnore]
		public string AbsoluteImageUrl { get; private set; }

		[XmlElement("keywords")]
		public string Keywords { get; set; }
		
		[XmlElement("description")]
		public string Description { get; set; }

		public void FixPaths(FileInfo slideFile)
		{
			if (string.IsNullOrEmpty(Image))
				return;

			string relativeUrl;
			try
			{
				relativeUrl = CourseUnitUtils.GetDirectoryRelativeWebPath(slideFile);
			}
			catch (Exception e)
			{
				/* It's ok if courses web directory is not found, i.e. when we run this code on course.exe tool
				 * Just show error as warning to console and set relativeUrl to ""
				 */
				Console.WriteLine("Warning: " + e.Message);
				Console.WriteLine("It's ok if you are using course.exe tool, but it's not ok if you are using ulearn web server.");
				relativeUrl = "";
			}
			
			var imagePath = Path.Combine(relativeUrl, Image);
			var configuration = ApplicationConfiguration.Read<UlearnConfiguration>();

			AbsoluteImageUrl = configuration.BaseUrl + imagePath;
		}
	}
}