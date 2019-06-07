using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ulearn.Common.Api.Models.Parameters;

namespace AntiPlagiarism.Api.Models.Parameters
{
	public class GetSubmissionPlagiarismsParameters: ApiParameters
	{
		[BindRequired]
		[FromQuery(Name = "submission_id")]
		public int SubmissionId { get; set; }
	}
}
