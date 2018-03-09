using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using uLearn.CSharp;
using uLearn.Model.Blocks;
using Ulearn.Common.Extensions;

namespace uLearn
{
	[TestFixture]
	public class SlideParser_should
	{
		[SetUp]
		public void SetUp()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
		}

		[Test]
		[Explicit("Для отладки на конкретных слайдах из курсов")]
		public void Test()
		{
			var slide =
				(ExerciseSlide)GenerateSlideFromFile(@"..\..\..\courses\BasicProgramming\Slides\U03_Cycles\S041_PowerOfTwo.cs");
			Console.WriteLine(slide.Exercise.BuildSolution("public void T(){}"));
		}

		[Test]
		public void make_markdown_from_comments()
		{
			var slide = GenerateSlide("SingleComment.cs");
			Assert.That(slide.Blocks.Length, Is.EqualTo(1));
			Assert.That(slide.Blocks[0].IsCode(), Is.False);
			Assert.That(slide.Blocks[0].Text(), Is.EqualTo("==Multiline comment\r\nShould become markdown text"));
		}

		[Test]
		public void fail_on_wrong_identation()
		{
			Assert.That(() => GenerateSlide("WrongIdentation.cs"), Throws.Exception);
		}

		[Test]
		public void ignore_wrong_identation_of_empty_lines()
		{
			var slide = GenerateSlide("WrongIdentationOfEmptyLines.cs");
			Assert.That(slide.Blocks.Length, Is.EqualTo(1));
			Assert.That(slide.Blocks[0].Text().SplitToLines().Length, Is.EqualTo(1));
		}

		[Test]
		public void ignore_comments_inside_methods()
		{
			var slide = GenerateSlide("CommentsInsideCodeBlock.cs");
			foreach (var block in slide.Blocks)
			{
				Console.WriteLine(block.ToString());
				Assert.That(block.IsCode(), Is.True);
			}
		}

		[Test]
		public void make_separate_blocks_from_separate_comments()
		{
			Slide slide = GenerateSlide("ManyComments.cs");
			Assert.That(slide.Blocks.Length, Is.EqualTo(3));
			Assert.That(slide.Blocks[1].Text(), Is.EqualTo("2nd block"));
		}

		[Test]
		public void make_ShowOnSlide_method_as_code_block()
		{
			Slide slide = GenerateSlide("Simple.cs");
			Assert.That(slide.Blocks.Length, Is.EqualTo(1));
			Assert.That(slide.Blocks[0].IsCode());
			Assert.That(slide, Is.TypeOf<Slide>());
		}

		[Test]
		public void make_class_as_code_block()
		{
			Slide slide = GenerateSlide("NestedClass.cs");
			Assert.That(slide.Blocks[0].IsCode());
			Assert.That(((CodeBlock)slide.Blocks[0]).Code, Does.Contain("class Point"));
		}

		[Test]
		public void remove_attributes_from_nested_class_members()
		{
			Slide slide = GenerateSlide("NestedClass.cs");
			Assert.That(slide.Blocks[0].Text(), Does.Not.Contain("["));
		}

		[Test]
		public void remove_hidden_members_of_nested_class()
		{
			Slide slide = GenerateSlide("NestedClass.cs");
			Assert.That(slide.Blocks[0].Text(), Does.Not.Contain("Hidden"));
		}

		[Test]
		public void remove_common_nesting_of_nested_class()
		{
			Slide slide = GenerateSlide("NestedClass.cs");
			Assert.That(slide.Blocks[0].Text(), Does.Contain("public class Point"));
		}

		[Test]
		public void text_blocks_from_comments()
		{
			Slide slide = GenerateSlide("Comments.cs");
			var texts = slide.Blocks.Select(b => b.Text().Trim().ToLower()).ToArray();
			Action<string> contains = block => Assert.That(texts, Has.Exactly(1).EqualTo(block));
			contains("before slide class");
			contains("before slide class 2");
			contains("before nested class");
			contains("before nested class 2");
			contains("before method");
			contains("before method 2");
			contains("before slide class ends");
			contains("before slide class ends 2");
			contains("after slide class");
			contains("after slide class 2");
		}


		[Test]
		public void remove_Excluded_members_from_solution()
		{
			var slide = (ExerciseSlide)GenerateSlide("NestedClass.cs");
			var solution = slide.Exercise.BuildSolution("Console.WriteLine(\"Hello world\")").ToString();
			Assert.That(solution, Does.Not.Contain("["));
			Assert.That(solution, Does.Not.Contain("]"));
			Assert.That(solution, Does.Not.Contain("public int X, Y"));
			Assert.That(solution, Does.Not.Contain("public Point(int x, int y)"));
		}


		[Test]
		public void make_code_block_with_method_signature_if_specified()
		{
			Slide slide = GenerateSlide("Simple.cs");
			Assert.That(slide.Blocks[0].Text(), Does.Contain("public void Method()"));
		}

		[Test]
		public void remove_common_nesting_in_method_body()
		{
			Slide slide = GenerateSlide("Simple.cs");
			Assert.That(slide.Blocks[0].Text(), Does.Contain("Console.WriteLine(42);"));
		}

		[Test]
		public void remove_common_nesting_in_method_with_header()
		{
			Slide slide = GenerateSlide("Simple.cs");
			Assert.That(slide.Blocks[0].Text(), Does.Contain("\npublic void Method()"));
		}

		[Test]
		public void remove_method_header_from_code_block()
		{
			Slide slide = GenerateSlide("Simple.cs");
			var blockText = slide.Blocks[0].Text();
			Assert.That(blockText, Does.Contain("Console.WriteLine(42);"));
			Assert.That(blockText, Does.Not.Contain("HiddenMethodHeader"));
		}

		[Test]
		public void remove_hidden_members_from_code_block()
		{
			Slide slide = GenerateSlide("Simple.cs");
			var blockText = slide.Blocks[0].Text();
			Assert.That(blockText, Does.Not.Contain("Hidden"));
		}

		[Test]
		public void not_show_members_of_hidden_class()
		{
			Slide slide = GenerateSlide("HiddenNestedClass.cs");
			var blockText = slide.Blocks[0].Text();
			Assert.That(blockText, Does.Not.Contain("Hidden"));
		}

		[Test]
		public void join_adjacent_code_blocks()
		{
			Slide slide = GenerateSlide("AdjacentCodeBlocks.cs");
			Assert.That(slide.Blocks[0].IsCode(), Is.True);
			Assert.That(slide.Blocks[1].IsCode(), Is.False);
			Assert.That(slide.Blocks[2].IsCode(), Is.True);
			Assert.That(slide.Blocks.Length, Is.EqualTo(3));
		}

		[Test]
		public void preserve_blocks_order_as_in_source_file()
		{
			Slide slide = GenerateSlide("SlideWithComments.cs");
			Assert.That(slide.Blocks[0].Text(), Does.Contain("Comment"));
			Assert.That(slide.Blocks[1].IsCode());
			Assert.That(slide.Blocks[2].Text(), Does.Contain("Final"));
			Assert.That(slide.Blocks.Length, Is.EqualTo(3));
		}

		[Test]
		public void make_excercise_slide_from_method_with_exercise_attribute()
		{
			var slide = (ExerciseSlide)GenerateSlide("Exercise.cs");
			Assert.That(slide.Blocks.First().Text(), Is.EqualTo("Add 2 and 3 please!"));
			Assert.That(slide.Blocks.Skip(1).Single(), Is.InstanceOf<ExerciseBlock>());
			Assert.That(slide.Exercise.ExerciseInitialCode, Does.Contain("public void Add_2_and_3()"));
			Assert.That(slide.Exercise.ExerciseInitialCode, Does.Not.Contain("NotImplementedException"));
			Assert.That(slide.Exercise.HintsMd.Count, Is.EqualTo(0));
		}

		[Test]
		public void not_count_exercise_as_code_block()
		{
			var slide = GenerateSlide("Exercise.cs");
			Assert.That(slide.Blocks.Where(b => b.IsCode()), Is.Empty);
		}

		[Test]
		public void extract_ExpectedOutput()
		{
			var slide = (ExerciseSlide)GenerateSlide("Exercise.cs");
			Assert.That(slide.Exercise.ExpectedOutput, Is.EqualTo("5"));
		}

		[Test]
		public void uncomment_special_comments_with_starter_code()
		{
			var slide = (ExerciseSlide)GenerateSlide("ExerciseWithStarterCode.cs");
			var exerciseLines = slide.Exercise.ExerciseInitialCode.SplitToLines();
			Assert.That(exerciseLines.Length, Is.EqualTo(4), slide.Exercise.ExerciseInitialCode);
			Assert.That(exerciseLines[2], Is.EqualTo("	return x + y;"));
		}

		[Test]
		public void make_hints_from_hint_attributes()
		{
			var slide = (ExerciseSlide)GenerateSlide("ExerciseWithHints.cs");
			Assert.That(slide.Exercise.HintsMd, Is.EqualTo(new[] { "hint1", "hint2" }).AsCollection);
		}

		[Test]
		public void provide_solution_for_server()
		{
			var slide = (ExerciseSlide)GenerateSlide("HelloWorld.cs");
			var userSolution = "/* no solution */";
			var res = slide.Exercise.BuildSolution(userSolution);
			Console.WriteLine(res.ErrorMessage);
			var ans = res.SourceCode;
			StringAssert.DoesNotContain("[", ans);
			StringAssert.Contains("void Main(", ans);
			StringAssert.Contains(userSolution, ans);
			StringAssert.DoesNotContain("void HelloKitty(", ans);
		}

		[Test]
		public void include_video()
		{
			var slide = GenerateSlide("Includes.cs");
			var videoBlock = slide.Blocks.First();
			Assert.That(videoBlock, Is.TypeOf<YoutubeBlock>());
		}

		[Test]
		public void include_code()
		{
			var slide = GenerateSlide("Includes.cs");
			var renderedText = ((CodeBlock)slide.Blocks[1]).Code;
			var expected = "//included(_HelloWorld.cs)";
			Assert.That(renderedText, Contains.Substring(expected));
		}

		[Test]
		public void include_many_classes_on_slide()
		{
			var slide = GenerateSlide("ManyClasses.cs");
			var code = slide.Blocks[0].Text();
			Assert.That(code, Does.Contain("M0()"));
			Assert.That(code, Does.Contain("M()"));
			Assert.That(code, Does.Contain("ManyClasses3"));
			Assert.That(code, Does.Not.Contain("ManyClasses1"));
			Assert.That(code, Does.Not.Contain("ManyClasses2"));
		}

		[Test]
		public void set_initial_exercise_code_even_if_no_exercise_method()
		{
			var slide = (ExerciseSlide)GenerateSlide("ExerciseWithoutExerciseMethod.cs");
			Assert.That(slide.Exercise.ExerciseInitialCode.Trim(), Does.Contain("class MyClass"));
		}

		[Test]
		public void insert_userSolution_outside_class_if_exercise_is_under_class()
		{
			var slide = (ExerciseSlide)GenerateSlide("ExerciseWithoutExerciseMethod.cs");
			var sol = slide.Exercise.BuildSolution("public class MyClass{}").SourceCode;
			Assert.IsNotNull(sol);
			var indexOfMainClass = sol.IndexOf("ExerciseWithoutExerciseMethod");
			var indexOfSolutionClass = sol.IndexOf("class MyClass");
			Assert.That(indexOfSolutionClass, Is.LessThan(indexOfMainClass));
			Assert.That(indexOfSolutionClass, Is.GreaterThanOrEqualTo(0));
			Assert.That(indexOfMainClass, Is.GreaterThanOrEqualTo(0));
		}


		private static Slide GenerateSlide(string name)
		{
			var dir = new DirectoryInfo(@".\tests\stub");
			return SlideParser.ParseSlide(
				new FileInfo(@".\tests\" + name),
				new SlideInfo(new Unit(new UnitSettings(), dir), dir.GetFile(name), 0),
				dir, CourseSettings.DefaultSettings);
		}

		private static Slide GenerateSlideFromFile(string path)
		{
			var file = new FileInfo(path);
			return SlideParser.ParseSlide(
				file,
				new SlideInfo(new Unit(new UnitSettings(), file.Directory), file, 0),
				file.Directory, CourseSettings.DefaultSettings);
		}
	}
}