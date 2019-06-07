using System.IO;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Ulearn.Common.Extensions;
using Ulearn.Core.Courses.Slides.Blocks;
using Ulearn.Core.CSharp;

namespace uLearn.CSharp
{
	[TestFixture]
	public class CsMembersExtractor_should
	{
		private readonly DirectoryInfo dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory).GetSubdirectory("tests");

		private string GetRegion(string region, bool onlyBody = false, string fileName = "OverloadedMethods.cs")
		{
			var code = dir.GetContent(fileName).LineEndingsToUnixStyle();
			var extractor = new CsMembersExtractor(code);
			return extractor.GetRegion(new Label
			{
				Name = region,
				OnlyBody = onlyBody
			});
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void ExtractSingleClass()
		{
			Approvals.Verify(GetRegion("OverloadedMethods"));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void ExtractSingleMethod()
		{
			Approvals.Verify(GetRegion("Main"));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void ExtractSingleInterface()
		{
			Approvals.Verify(GetRegion("IRunnable", fileName: "NonClassTypes.cs"));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void ExtractSingleEnum()
		{
			Approvals.Verify(GetRegion("Enum", fileName: "NonClassTypes.cs"));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void ExtractSingleStruct()
		{
			Approvals.Verify(GetRegion("Struct", fileName: "NonClassTypes.cs"));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void ExtractOverloads()
		{
			Approvals.Verify(GetRegion("Method"));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void ExtractClassBody()
		{
			Approvals.Verify(GetRegion("OverloadedMethods", onlyBody: true));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void ExtractMethodBody()
		{
			Approvals.Verify(GetRegion("Main", onlyBody: true));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void ExtractInterfaceBody()
		{
			Approvals.Verify(GetRegion("IRunnable", onlyBody: true, fileName: "NonClassTypes.cs"));
		}

		[Test, UseReporter(typeof(DiffReporter))]
		public void ExtractStructBody()
		{
			Approvals.Verify(GetRegion("Struct", onlyBody: true, fileName: "NonClassTypes.cs"));
		}
	}
}