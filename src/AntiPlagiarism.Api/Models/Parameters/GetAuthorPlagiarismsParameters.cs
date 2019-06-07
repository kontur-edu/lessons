using System;
using AntiPlagiarism.Api.Defaults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ulearn.Common.Api.Models.Parameters;

namespace AntiPlagiarism.Api.Models.Parameters
{
	public class GetAuthorPlagiarismsParameters : ApiParameters
	{
		[BindRequired]
		[FromQuery(Name = "author_id")]
		public Guid AuthorId { get; set; }
		
		[BindRequired]
		[FromQuery(Name = "task_id")]
		public Guid TaskId { get; set; }

		[FromQuery(Name = "last_submissions_count")]
		public int LastSubmissionsCount { get; set; } = GetAuthorPlagiarismsDefaults.LastSubmissionsCount;
	}
}
