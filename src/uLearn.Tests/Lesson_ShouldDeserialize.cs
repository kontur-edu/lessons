using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.Courses.Slides.Exercises.Blocks;
using Ulearn.Core.Courses.Units;

namespace uLearn
{
	[TestFixture]
	public class Lesson_ShouldDeserialize
	{
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
		}
		
		[Test]
		public void SimpleMdBlock()
		{
			var blocks = DeserializeBlocks("<markdown>asd</markdown>");
			blocks.ShouldBeEquivalentTo(new[] { new MarkdownBlock("asd") });
		}

		[Test]
		public void SeveralMdBlocks()
		{
			var blocks = DeserializeBlocks("<markdown>1</markdown><markdown>2</markdown>");
			blocks.ShouldBeEquivalentTo(new[] { new MarkdownBlock("1"), new MarkdownBlock("2") });
		}

		[Test]
		public void MixedContentInsideMd()
		{
			var blocks = DeserializeBlocks("<markdown>1<code>2</code>3</markdown>");
			blocks.ShouldBeEquivalentTo(new SlideBlock[] { new MarkdownBlock("1"), new CodeBlock("2", null), new MarkdownBlock("3") });
		}

		[Test]
		public void OtherTypesOfBlocksAreOK()
		{
			var blocks = DeserializeBlocks("<markdown>1</markdown><code>2</code><youtube>sdfsdfsdf</youtube>");
			blocks.ShouldBeEquivalentTo(new SlideBlock[] { new MarkdownBlock("1"), new CodeBlock("2", null), new MarkdownBlock("3") });
		}

		[Test]
		public void InstructorNotesInsideMd()
		{
			var blocks = DeserializeBlocks("<markdown>1<note>secret</note>2<code>3</code>4</markdown>");
			blocks.ShouldBeEquivalentTo(new SlideBlock[]
			{
				new MarkdownBlock("1"),
				new MarkdownBlock("secret") { Hide = true },
				new MarkdownBlock("2"),
				new CodeBlock("3", null),
				new MarkdownBlock("4")
			});
		}

		[Test]
		public void UserCodeFilePath()
		{
			var ex = DeserializeLesson("<exercise.csproj>" +
										"<userCodeFile>user-code-name-111</userCodeFile>" +
										"</exercise.csproj>").Blocks.Single();
			((CsProjectExerciseBlock)ex).UserCodeFilePath.Should().Be("user-code-name-111");
		}

		[Test]
		public void ExerciseHints()
		{
			var ex = DeserializeLesson("<exercise.csproj>" +
										"<hint>Hint 1</hint>" +
										"<hint>Hint 2</hint>" +
										"</exercise.csproj>").Blocks.Single() as CsProjectExerciseBlock;
			if (ex == null)
				Assert.Fail("Can't parse exercise with hints");
			ex.Hints.Should().HaveCount(2);
			ex.Hints.Should().Contain(new List<string> { "Hint 1", "Hint 2" });
			ex.HintsMd.Should().HaveSameCount(ex.Hints);
		}

		private static Slide DeserializeLesson(string blocksXml)
		{
			var input = $@"
<slide xmlns='https://ulearn.me/schema/v2'>
{blocksXml}
</slide>";
			File.WriteAllText("temp.xml", input);
			var fileInfo = new FileInfo("temp.xml");
			return fileInfo.DeserializeXml<Slide>();
		}

		private static SlideBlock[] DeserializeBlocks(string blocksXml)
		{
			var buildUpContext = new SlideBuildingContext("Test", new Unit(null, new DirectoryInfo(".")), CourseSettings.DefaultSettings, null);
			var blocks = DeserializeLesson(blocksXml).Blocks;
			return blocks.SelectMany(b => b.BuildUp(buildUpContext, ImmutableHashSet<string>.Empty)).ToArray();
		}
	}
	
}