using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Ulearn.Core.Model.Edx.EdxComponents;

namespace Ulearn.Core.Courses.Slides.Blocks
{
	[XmlType("galleryImages")]
	public class ImageGalleryBlock : SlideBlock
	{
		[XmlElement("image")]
		public string[] RelativeToUnitDirectoryImagePaths { get; set; }

		public ImageGalleryBlock(string[] relativeToUnitDirectoryImagePaths)
		{
			RelativeToUnitDirectoryImagePaths = relativeToUnitDirectoryImagePaths;
		}

		public IEnumerable<string> GetAbsoluteImageUrls(string baseUrlApi, string courseId, string unitPathRelativeToCourse)
		{
			return RelativeToUnitDirectoryImagePaths.Select(p => CourseUrlHelper.GetAbsoluteUrlToFile(baseUrlApi, courseId, unitPathRelativeToCourse, p));
		}

		public ImageGalleryBlock()
		{
		}

		public override IEnumerable<SlideBlock> BuildUp(SlideBuildingContext context, IImmutableSet<string> filesInProgress)
		{
			yield return this;
		}

		public override string ToString()
		{
			return $"Gallery with images:\n{string.Join("\n", RelativeToUnitDirectoryImagePaths)}";
		}

		public override Component ToEdxComponent(EdxComponentBuilderContext context)
		{
			var urlName = context.Slide.NormalizedGuid + context.ComponentIndex;
			return new GalleryComponent(urlName, context.DisplayName, urlName, GetAbsoluteImageUrls(context.UlearnBaseUrlApi, context.CourseId, context.Slide.Unit.UnitDirectoryRelativeToCourse).ToArray());
		}
	}
}