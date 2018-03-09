using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using uLearn.Model.Blocks;
using uLearn.Model.Edx;
using uLearn.Model.Edx.EdxComponents;

namespace uLearn.CourseTool
{
	[TestFixture]
	[Explicit]
	public class Converter_should
	{
		private Course course;
		private static readonly Unit unit = new Unit(UnitSettings.CreateByTitle("u1", CourseSettings.DefaultSettings), new DirectoryInfo("u1"));
		private readonly Slide aTextSlide = new Slide(new[] { new MdBlock("hello"), }, new SlideInfo(unit, new FileInfo("file"), 0), "title", Guid.NewGuid(), null);
		private readonly Slide exerciseSlide = new Slide(new ExerciseBlock[] { new ProjectExerciseBlock(), new SingleFileExerciseBlock() }, new SlideInfo(unit, new FileInfo("file"), 0), "title", slideIdFromCourse, null);
		private const string youtubeIdFromCourse = "GZS36w_fxdg";
		private static readonly Guid slideIdFromCourse = Guid.Parse("108C89D9-36F0-45E3-BBEE-B93AC971063F");
		private CourseManager courseManager;
		private const string slideUrl = "https://192.168.33.1:44300/Course/{0}/LtiSlide/{1}";
		private const string solutionsUrl = "https://192.168.33.1:44300/Course/{0}/AcceptedAlert/{1}";
		private const string ltiId = "edx";
		private const string testFolderName = "test";

		[SetUp]
		public void SetUp()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			if (!Directory.Exists(testFolderName))
				Directory.CreateDirectory(testFolderName);
			courseManager = new CourseManager(new DirectoryInfo("."));
			course = courseManager.LoadCourseFromDirectory(new DirectoryInfo(@"..\..\..\..\courses\ForTests\Slides"));
		}

		[TearDown]
		public void TearDown()
		{
			Utils.DeleteDirectoryIfExists(testFolderName);
		}

		private EdxCourse ConvertForTestsCourseToEdx(Dictionary<string, string> youtubeId2UlearnVideoIds = null)
		{
			var config = new Config
			{
				Organization = "org",
				LtiId = ""
			};
			return Converter.ToEdxCourse(course, config, slideUrl, solutionsUrl,
				youtubeId2UlearnVideoIds ?? new Dictionary<string, string>());
		}

		private IEnumerable<VideoComponent> GetVideoComponentFromDictionary(Dictionary<string, Tuple<string, string>> dict)
		{
			return dict.Select(x => new VideoComponent(x.Key, x.Value.Item2, x.Value.Item1));
		}

		public HashSet<string> GetDirectoryFiles(string directory)
		{
			var files = Directory.GetFiles(directory).Select(Path.GetFileName);
			var dirFiles = Directory.GetDirectories(directory).SelectMany(x => Directory.GetFiles(x).Select(Path.GetFileName));
			var hs = new HashSet<string>(files.Concat(dirFiles));
			return hs;
		}

		[Test]
		public void convert_saveAndLoadEdxCourse()
		{
			var edxCourse = ConvertForTestsCourseToEdx();

			var f1 = $"{testFolderName}/{1}";
			var f2 = $"{testFolderName}/{2}";
			edxCourse.Save(f1);
			EdxCourse.Load(f1).Save(f2);

			CollectionAssert.AreEqual(GetDirectoryFiles(f1), GetDirectoryFiles(f2));
		}

		[Test]
		public void convert_assign_SlideIds_to_EdxUrlNames()
		{
			var edxCourse = ConvertForTestsCourseToEdx();
			var ulearnSlideIds = course.Slides.Select(x => x.NormalizedGuid);
			var edxVerticals = edxCourse.CourseWithChapters.Chapters[0].Sequentials
				.SelectMany(x => x.Verticals)
				.ToList();
			foreach (var vertical in edxVerticals)
				Console.WriteLine(vertical.DisplayName);
			CollectionAssert.IsSubsetOf(ulearnSlideIds, edxVerticals.Select(x => x.UrlName));
		}

		[Test]
		public void convert_assign_VideoIds_accordingToPassedDictionary()
		{
			var ulearnVideoGuid = Utils.NewNormalizedGuid();
			var edxCourse = ConvertForTestsCourseToEdx(new Dictionary<string, string> { { youtubeIdFromCourse, ulearnVideoGuid } });
			Assert.AreEqual(youtubeIdFromCourse, edxCourse.GetVideoById(ulearnVideoGuid).NormalSpeedVideoId);
		}

		[Test]
		public void patch_updates_YoutubeIds()
		{
			var ulearnVideoGuid = Utils.NewNormalizedGuid();
			var edxCourse = ConvertForTestsCourseToEdx(new Dictionary<string, string> { { youtubeIdFromCourse, ulearnVideoGuid } });
			var olxPath = string.Format("{0}/{1}", testFolderName, course.Id);
			edxCourse.Save(olxPath);

			var videoDict = new Dictionary<string, Tuple<string, string>> { { ulearnVideoGuid, Tuple.Create("QWFuk3ymXxc", "") } };

			new OlxPatcher(olxPath)
				.PatchComponents(edxCourse, GetVideoComponentFromDictionary(videoDict));

			Assert.That(File.ReadAllText(string.Format("{0}/video/{1}.xml", olxPath, ulearnVideoGuid)).Contains("QWFuk3ymXxc"));
		}

		[Test]
		public void patch_putsNewVideos_toExistingUnsortedChapter()
		{
			var videoGuid = Utils.NewNormalizedGuid();
			var videoGuid2 = Utils.NewNormalizedGuid();
			var edxCourse = ConvertForTestsCourseToEdx(new Dictionary<string, string> { { youtubeIdFromCourse, videoGuid } });
			var olxPath = string.Format("{0}/{1}", testFolderName, course.Id);
			edxCourse.Save(olxPath);

			var patcher = new OlxPatcher(olxPath);

			var videoDict = new Dictionary<string, Tuple<string, string>>
			{
				{ videoGuid, Tuple.Create("QWFuk3ymXxc", "") },
				{ videoGuid2, Tuple.Create("w8_GlqSkG-U", "") }
			};

			patcher.PatchComponents(edxCourse, GetVideoComponentFromDictionary(videoDict));

			videoDict = new Dictionary<string, Tuple<string, string>>
			{
				{ videoGuid, Tuple.Create("QWFuk3ymXxc", "") },
				{ videoGuid2, Tuple.Create("w8_GlqSkG-U", "") },
				{ Utils.NewNormalizedGuid(), Tuple.Create("qTnKi67AAlg", "") }
			};

			patcher.PatchComponents(edxCourse, GetVideoComponentFromDictionary(videoDict));

			var edxCourse2 = EdxCourse.Load(olxPath);
			Assert.AreEqual("Unsorted", edxCourse2.CourseWithChapters.Chapters[1].DisplayName);
			Assert.AreEqual(2, edxCourse2.CourseWithChapters.Chapters[1].Sequentials.Length);
		}

		[Test]
		public void patch_doesNotCreateUnsortedChapter_ifNoNewSlides()
		{
			var edxCourse = ConvertForTestsCourseToEdx();
			var olxPath = string.Format("{0}/{1}", testFolderName, course.Id);
			edxCourse.Save(olxPath);

			new OlxPatcher(olxPath).PatchVerticals(edxCourse, course.Slides
				.Select(x => x.ToVerticals(
					course.Id,
					slideUrl,
					solutionsUrl,
					new Dictionary<string, string>(),
					ltiId
				).ToArray()));

			var edxCourse2 = EdxCourse.Load(olxPath);
			Assert.IsFalse(edxCourse2.CourseWithChapters.Chapters.Any(c => c.DisplayName == "Unsorted"));
		}

		[Test]
		public void patch_createsUnsortedChapter_withNewSlides()
		{
			var edxCourse = ConvertForTestsCourseToEdx();
			var olxPath = $"{testFolderName}/{course.Id}";
			edxCourse.Save(olxPath);

			new OlxPatcher(olxPath).PatchVerticals(edxCourse, new[] { aTextSlide }
				.Select(x => x.ToVerticals(
					course.Id,
					slideUrl,
					solutionsUrl,
					new Dictionary<string, string>(),
					ltiId
				).ToArray()));

			var edxCourse2 = EdxCourse.Load(olxPath);
			Assert.AreEqual("Unsorted", edxCourse2.CourseWithChapters.Chapters.Last().DisplayName);
		}

		[Test]
		public void patch_updatesOrdinarySlide_withExerciseSlide()
		{
			var edxCourse = ConvertForTestsCourseToEdx();
			var olxPath = string.Format("{0}/{1}", testFolderName, course.Id);
			edxCourse.Save(olxPath);

			var slidesCount = edxCourse.CourseWithChapters.Chapters[0].Sequentials[0].Verticals.Count();

			new OlxPatcher(olxPath).PatchVerticals(edxCourse, new[] { exerciseSlide }
				.Select(x => x.ToVerticals(
					course.Id,
					slideUrl,
					solutionsUrl,
					new Dictionary<string, string>(),
					ltiId
				).ToArray()));

			var edxCourse2 = EdxCourse.Load(olxPath);
			var patchedSlidesCount = edxCourse2.CourseWithChapters.Chapters[0].Sequentials[0].Verticals.Count();
			Assert.AreEqual(slidesCount + 1, patchedSlidesCount);
		}
	}
}