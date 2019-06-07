using System.Collections.Generic;
using System.Runtime.Serialization;
using Ulearn.Common.Api.Models.Responses;

namespace AntiPlagiarism.Api.Models.Results
{
	[DataContract]
	public class RebuildSnippetsForTaskResponse : SuccessResponse
	{
		[DataMember(Name = "submissions_ids")]
		public List<int> SubmissionsIds { get; set; }
	}
}