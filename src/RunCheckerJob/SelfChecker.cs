using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Vostok.Logging.Abstractions;
using Newtonsoft.Json;
using NUnit.Framework;
using Ulearn.Common;
using Ulearn.Core;
using Ulearn.Core.RunCheckerJobApi;

namespace RunCheckerJob
{
	internal class SelfChecker
	{
		private readonly DockerSandboxRunner sandboxRunner;

		private static ILog log => LogProvider.Get().ForContext(typeof(SelfChecker));

		public SelfChecker(DockerSandboxRunner sandboxRunner)
		{
			this.sandboxRunner = sandboxRunner;
		}

		public RunningResults SelfCheck(DirectoryInfo sandboxDir)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			var imageName = sandboxDir.Name;
			var srcDirectory = new DirectoryInfo(Path.GetFullPath(Path.Combine(sandboxDir.FullName, "sample/src/")));
			byte[] zipBytes;
			using (var stream = ZipUtils.CreateZipFromDirectory(new List<string> { srcDirectory.FullName }, new List<string> { "node_modules", ".idea" }, null, Encoding.UTF8))
				zipBytes = stream.ToArray();
			var submissionFile = new FileInfo(Path.GetFullPath(Path.Combine(sandboxDir.FullName, "sample/submission.json")));
			var submission = JsonConvert.DeserializeObject<CommandRunnerSubmission>(File.ReadAllText(submissionFile.FullName));
			submission.Id = Utils.NewNormalizedGuid();
			submission.ZipFileData = zipBytes;
			submission.DockerImageName = imageName;
			var res = sandboxRunner.Run(submission);
			log.Info("SelfCheck result: {Result}", JsonConvert.SerializeObject(res));
			return res;
		}
	}

	[TestFixture, Explicit]
	internal class SelfCheckerTests
	{
		private static readonly string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

		[Test, Explicit]
		public void SelfCheckTest()
		{
			var sandboxesDirectory = new DirectoryInfo(Path.GetFullPath(Path.Combine(baseDirectory, "../../../../../sandboxes/")));
			foreach (var dir in sandboxesDirectory.GetDirectories())
			{
				var sampleDir = new DirectoryInfo(Path.GetFullPath(Path.Combine(dir.FullName, "sample/")));
				if (!sampleDir.Exists)
					continue;
				var res = new SelfChecker(new DockerSandboxRunner())
					.SelfCheck(dir);
				Assert.AreEqual(Verdict.Ok, res.Verdict);
			}
		}
	}
}