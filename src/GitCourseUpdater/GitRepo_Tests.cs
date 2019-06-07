using System.IO;
using System.Reflection;
using NUnit.Framework;
using Serilog;

namespace GitCourseUpdater
{
	[TestFixture, Explicit]
	public class GitRepo_Tests
	{
		[Test, Explicit]
		public void Test()
		{
			var log = new LoggerConfiguration().CreateLogger();
			var url = "git@github.com:vorkulsky/git_test.git";
			var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var reposBaseDir = Path.Combine(assemblyPath, "repos");
			var privateKeyPath = new FileInfo(Path.Combine(assemblyPath, "../../../TestResources/test_key"));
			var publicKeyPath = new FileInfo(Path.Combine(assemblyPath, "../../../TestResources/test_key.pub"));
			var keysTempDirectory = new DirectoryInfo(Path.Combine(assemblyPath, "../../../TestResources/"));
			var privateKey = File.ReadAllText(privateKeyPath.FullName);
			var publicKey = File.ReadAllText(publicKeyPath.FullName);
			using (IGitRepo repo = new GitRepo(url, new DirectoryInfo(reposBaseDir), publicKey, privateKey, keysTempDirectory, log))
			{
				var zip = repo.GetCurrentStateAsZip();
				File.WriteAllBytes(Path.Combine(assemblyPath, "result.zip"), zip.ToArray());
				var lastCommitInfo = repo.GetCurrentCommitInfo();
				Assert.AreEqual(lastCommitInfo.Hash, "bd2f024b57ea7b603447c5dd6e650d0e72a8c6d0");
				var info = repo.GetCommitInfo("37d6b0857fd3ae6135dcd2cec6899c1e318f9040");
				Assert.AreEqual(info.Message, "Initial commit");
				var files = repo.GetChangedFiles("37d6b0857fd3ae6135dcd2cec6899c1e318f9040", "bd2f024b57ea7b603447c5dd6e650d0e72a8c6d0");
				CollectionAssert.AreEqual(files, new[] {"test.txt"});
				repo.Checkout("test_branch");
			}
		}
	}
}