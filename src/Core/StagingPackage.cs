using System;

namespace Ulearn.Core
{
	public class StagingPackage
	{
		public StagingPackage(string name, DateTime timestamp)
		{
			Name = name;
			Timestamp = timestamp;
		}

		public string Name;
		public DateTime Timestamp;
	}
}