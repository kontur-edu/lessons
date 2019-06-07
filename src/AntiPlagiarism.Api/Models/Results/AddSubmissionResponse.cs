using System.Runtime.Serialization;
using Ulearn.Common.Api.Models.Responses;

namespace AntiPlagiarism.Api.Models.Results
{
	[DataContract]
	public class AddSubmissionResponse : SuccessResponse
	{
		[DataMember(Name = "submission_id")]
		public int SubmissionId { get; set; }
	}
}