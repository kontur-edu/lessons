using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Ulearn.Common.Extensions;
using Ulearn.Core.CSharp.Validators;

namespace uLearn.CSharp.ArrayGetLengthValidation
{
	[TestFixture]
	public class ArrayLengthStyleValidator_should
	{
		private static DirectoryInfo TestDataDir => new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..",
			"..", "CSharp", "ArrayGetLengthValidation", "TestData"));
		private static DirectoryInfo IncorrectTestDataDir => TestDataDir.GetDirectories("Incorrect").Single();
		private static DirectoryInfo CorrectTestDataDir => TestDataDir.GetDirectories("Correct").Single();

		private static IEnumerable<FileInfo> CorrectFiles => CorrectTestDataDir.EnumerateFiles();
		private static IEnumerable<FileInfo> IncorrectFiles => IncorrectTestDataDir.EnumerateFiles();

		private static DirectoryInfo BasicProgrammingDirectory =>
			new DirectoryInfo(TestPaths.BasicProgrammingDirectoryPath);

		private static IEnumerable<FileInfo> BasicProgrammingFiles =>
			BasicProgrammingDirectory
				.EnumerateFiles("*.cs", SearchOption.AllDirectories)
				.Where(f => !f.Name.Equals("Settings.Designer.cs") &&
							!f.Name.Equals("Resources.Designer.cs") &&
							!f.Name.Equals("AssemblyInfo.cs"));

		private static DirectoryInfo ULearnSubmissionsDirectory =>
			new DirectoryInfo(TestPaths.ULearnSubmissionsDirectoryPath);

		private static IEnumerable<FileInfo> SubmissionsFiles =>ULearnSubmissionsDirectory
			.EnumerateFiles("*.cs", SearchOption.AllDirectories)
			.Where(f => f.Name.Contains("Accepted"));
		
		private readonly ArrayLengthStyleValidator validator = new ArrayLengthStyleValidator();

		[TestCaseSource(nameof(IncorrectFiles))]
		public void FindErrors(FileInfo file)
		{
			var code = file.ContentAsUtf8();
			var errors = validator.FindErrors(code);

			errors.Should().NotBeNullOrEmpty();
		}

		[TestCaseSource(nameof(CorrectFiles))]
		public void NotFindErrors(FileInfo file)
		{
			var code = file.ContentAsUtf8();
			var errors = validator.FindErrors(code);
			if (errors != null && errors.Count != 0)
			{
				Console.WriteLine(errors);
			}

			errors.Should().BeNullOrEmpty();
		}

		[Explicit]
		[TestCaseSource(nameof(BasicProgrammingFiles))]
		public void NotFindErrors_InBasicProgramming(FileInfo file)
		{
			var fileContent = file.ContentAsUtf8();

			var errors = validator.FindErrors(fileContent);

			if (errors != null && errors.Count != 0)
			{
				File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..",
						"..", "CSharp", "ExampleFiles", "errors", $"{file.Name}_errors.txt"),
					$@"{fileContent}

{errors.Join(Environment.NewLine)}");

				Assert.Fail();
			}
		}

		[Explicit]
		[TestCaseSource(nameof(SubmissionsFiles))]
		public void NotFindErrors_InCheckAcceptedFiles(FileInfo file)
		{
			var fileContent = file.ContentAsUtf8();

			var errors = validator.FindErrors(fileContent);
			if (errors != null && errors.Count != 0)
			{
				File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..",
						"..", "CSharp", "ExampleFiles", "submissions_errors", $"{file.Name}_errors.txt"),
					$@"{fileContent}

{errors.Join(Environment.NewLine)}");

				Assert.Fail();
			}
		}
	}
}
