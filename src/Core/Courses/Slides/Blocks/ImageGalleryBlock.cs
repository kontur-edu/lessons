using System.IO;
using System.Xml.Serialization;
using Ulearn.Core.Model.Edx.EdxComponents;

namespace Ulearn.Core.Courses.Slides.Blocks
{
	[XmlType("galleryImages")]
	public class ImageGalleryBlock : SlideBlock
	{
		[XmlElement("image")]
		public string[] ImageUrls { get; set; }

		public ImageGalleryBlock(string[] images)
		{
			ImageUrls = images;
		}

		public ImageGalleryBlock()
		{
		}

		public override string ToString()
		{
			return $"Gallery with images:\n{string.Join("\n", ImageUrls)}";
		}

		public override Component ToEdxComponent(string displayName, string courseId, Slide slide, int componentIndex, string ulearnBaseUrl, DirectoryInfo coursePackageRoot)
		{
			var urlName = slide.NormalizedGuid + componentIndex;
			return new GalleryComponent(urlName, displayName, urlName, slide.Info.SlideFile.Directory.FullName, ImageUrls);
		}
	}
}