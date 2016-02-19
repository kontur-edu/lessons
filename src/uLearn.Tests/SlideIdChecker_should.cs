using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using uLearn.SlideIdChecking;

namespace uLearn
{
	[TestFixture]
	public class SlideIdChecker_should
	{
		[Test]
		public void CheckOneCourseWithoutErrors()
		{
			var slides = GetMinimalSlidesInfo(new[] { "1", "1" }, new[] { "2", "2" });
			CollectionAssert.IsEmpty(SlideIdChecker.FindDuplicatedIds(slides));
		}

		[Test]
		public void CheckOneCourse()
		{
			var slides = GetMinimalSlidesInfo(new[] { "1", "1" }, new[] { "1", "2" }, new[] { "2", "3" });
			var errors = SlideIdChecker.FindDuplicatedIds(slides);
			Assert.AreEqual(1, errors.Count);
			Assert.AreEqual("1", errors[0].SlideId);
			CollectionAssert.AreEqual(new List<string> {"1", "2"}, errors[0].SlideTitles);
		}

		[Test]
		public void CheckOneCourseAllEquals()
		{
			var titles = Enumerable.Range(0, 10).Select(i => i.ToString(CultureInfo.InvariantCulture)).ToList();
			var slides = GetMinimalSlidesInfo(titles.Select(i => new[] { "1", i }));
			var errors = SlideIdChecker.FindDuplicatedIds(slides);
			Assert.AreEqual(1, errors.Count);
			Assert.AreEqual("1", errors[0].SlideId);
			CollectionAssert.AreEqual(titles, errors[0].SlideTitles);
		}

		[Test]
		public void CheckOneCourseManyGroups()
		{
			var slides = GetMinimalSlidesInfo(Enumerable.Range(1, 15).Select(i => new[] { ((2 * i + 1) % 3).ToString(CultureInfo.InvariantCulture), i.ToString(CultureInfo.InvariantCulture) }));
			var errors = SlideIdChecker.FindDuplicatedIds(slides);
			errors = errors.OrderBy(list => list.SlideId).ToList();
			Assert.AreEqual(3, errors.Count);
			for (var i = 0; i < 3; ++i)
			{
				Assert.AreEqual(i.ToString(CultureInfo.InvariantCulture), errors[i].SlideId);
				var actual = errors[i].SlideTitles.Select(int.Parse).OrderBy(j => j);
				var expected = Enumerable.Range(1, 15).Where(j => (2 * j + 1) % 3 == i).ToList();
				CollectionAssert.AreEqual(expected, actual);
			}
		}

		[Test]
		public void CheckManyCoursesWithoutErrors()
		{
			var slides = GetRequiredSlidesInfo(new[] { "1", "1", "1" }, new[] { "2", "2", "1" });
			CollectionAssert.IsEmpty(SlideIdChecker.FindDuplicatedSlideIdsFor(slides, "0"));
			CollectionAssert.IsEmpty(SlideIdChecker.FindDuplicatedSlideIdsFor(slides, "1"));
		}

		[Test]
		public void CheckManyCoursesWithoutOuterErrors()
		{
			var slides = GetRequiredSlidesInfo(new[] { "1", "1", "1" }, new[] { "1", "2", "1" }, new[] { "3", "3", "2" });
			CollectionAssert.IsEmpty(SlideIdChecker.FindDuplicatedSlideIdsFor(slides, "1"));
			CollectionAssert.IsEmpty(SlideIdChecker.FindDuplicatedSlideIdsFor(slides, "2"));
		}

		[Test]
		public void CheckManyCourses()
		{
			var slides = GetRequiredSlidesInfo(new[] { "1", "1", "1" }, new[] { "1", "2", "1" }, new[] { "1", "3", "2" }, new[] { "2", "4", "3" });
			var errors = SlideIdChecker.FindDuplicatedSlideIdsFor(slides, "1");
			Assert.AreEqual(1, errors.Count);
			Assert.AreEqual("1", errors[0].SlideId);
			Assert.AreEqual(3, errors[0].SlideDescriptions.Count);
			var descriptions = errors[0].SlideDescriptions.OrderBy(description => description.Title).ToList();
			for (var i = 1; i < 4; ++i)
			{
				var description = descriptions[i - 1];
				Assert.AreEqual(i.ToString(CultureInfo.InvariantCulture), description.Title);
				var courseId = (i / 3 + 1).ToString(CultureInfo.InvariantCulture);
				Assert.AreEqual(courseId, description.CourseId);
				Assert.AreEqual(courseId, description.CourseTitle);
			}
		}

		[Test]
		public void CheckManyCoursesOthers()
		{
			var slides = GetRequiredSlidesInfo(
				new[] { "1", "1", "1" },
				new[] { "2", "2", "2" },
				new[] { "2", "3", "3" }
				);
			var errors = SlideIdChecker.FindDuplicatedSlideIdsFor(slides, "1");
			CollectionAssert.IsEmpty(errors);
		}

		private static IEnumerable<MinimalSlideInfo> GetMinimalSlidesInfo(IEnumerable<string[]> strs)
		{
			return strs.Select(strings => new MinimalSlideInfo { Id = strings[0], Title = strings[1] });
		}

		private static IEnumerable<MinimalSlideInfo> GetMinimalSlidesInfo(params string[][] strs)
		{
			return GetMinimalSlidesInfo(strs.ToList());
		}

		private static List<RequiredSlideInfo> GetRequiredSlidesInfo(params string[][] strs)
		{
			return strs.Select(s => new RequiredSlideInfo { Id = s[0], Title = s[1], CourseId = s[2], CourseTitle = s[2] }).ToList();
		}
	}
}