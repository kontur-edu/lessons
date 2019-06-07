using NUnit.Framework;

namespace GitCourseUpdater
{
	public class SshKeyGen_Tests
	{
		[TestFixture, Explicit]
		public class GitRepo_Tests
		{
			[Test, Explicit]
			public void Test()
			{
				var rsa = SshKeyGenerator.Generate();
			}
		}
	}
}