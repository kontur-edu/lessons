using AntiPlagiarism.Web.Configuration;
using Ulearn.Core.Configuration;

namespace AntiPlagiarism.UpdateDb.Configuration
{
	public class AntiPlagiarismUpdateDbConfiguration: AbstractConfiguration
	{
		public string Database { get; set; }
		
		public HostLogConfiguration HostLog { get; set; }
		
		public string GraphiteServiceName { get; set; }
		
		public AntiPlagiarismConfiguration AntiPlagiarism { get; set; }
	}
}