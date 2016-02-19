using System.Collections.Generic;
using System.Linq;

namespace uLearn.SlideIdChecking
{
	public static class SlideIdChecker
	{
		public static List<LocalSlideIdErrorList> FindDuplicatedIds(this Course course)
		{
			return FindDuplicatedIds(course.Slides.Select(GetMinimalInfo));
		}

		public static List<GlobalSlideIdErrorList> FindGlobalDuplicatedIdsFor(this CourseManager courseManager, string courseId)
		{
			return FindDuplicatedSlideIdsFor(courseManager.GetSlidesInfo(), courseId);
		}

		public static List<GlobalSlideIdErrorList> FindAllDuplicatedIds(this CourseManager courseManager)
		{
			return FindDuplicatedSlideIds(courseManager.GetSlidesInfo());
		}

		private static IEnumerable<RequiredSlideInfo> GetSlidesInfo(this CourseManager courseManager)
		{
			return courseManager.GetCourses().SelectMany(GetSlidesInfo).ToList();
		}

		private static IEnumerable<RequiredSlideInfo> GetSlidesInfo(Course course)
		{
			return course.Slides.Select(slide => slide.GetSlideInfo(course));
		}

		public static List<GlobalSlideIdErrorList> FindDuplicatedSlideIds(IEnumerable<RequiredSlideInfo> slides)
		{
			return slides
				.GroupBy(slide => slide.Id)
				.Where(g => g.Count() != 1)
				.Select(g => new GlobalSlideIdErrorList { SlideId = g.Key, SlideDescriptions = g.Select(GetDescription).ToList() })
				.ToList();
		}

		public static List<LocalSlideIdErrorList> FindDuplicatedIds(IEnumerable<MinimalSlideInfo> slides)
		{
			return slides
				.GroupBy(slide => slide.Id)
				.Where(g => g.Count() != 1)
				.Select(g => new LocalSlideIdErrorList { SlideId = g.Key, SlideTitles = g.Select(slide => slide.Title).ToList() })
				.ToList();
		}

		public static List<GlobalSlideIdErrorList> FindDuplicatedSlideIdsFor(IEnumerable<RequiredSlideInfo> slidesInfo, string courseId)
		{
			return FindDuplicatedSlideIds(slidesInfo)
				.Where(list => list.SlideDescriptions.Any(description => description.CourseId == courseId))
				.Where(list => list.SlideDescriptions.Any(description => description.CourseId != courseId))
				.ToList();
		}

		private static MinimalSlideInfo GetMinimalInfo(this Slide slide)
		{
			return new MinimalSlideInfo { Id = slide.Id, Title = slide.Title };
		}

		private static RequiredSlideInfo GetSlideInfo(this Slide slide, Course course)
		{
			return new RequiredSlideInfo
			{
				Id = slide.Id,
				Title = slide.Title,
				CourseId = course.Id,
				CourseTitle = course.Title
			};
		}

		private static SlideDescription GetDescription(this RequiredSlideInfo info)
		{
			return new SlideDescription
			{
				Title = info.Title, 
				CourseId = info.CourseId, 
				CourseTitle = info.CourseTitle
			};
		}
	}
}