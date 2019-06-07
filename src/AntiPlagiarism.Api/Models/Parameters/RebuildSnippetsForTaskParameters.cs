using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ulearn.Common.Api.Models.Parameters;

namespace AntiPlagiarism.Api.Models.Parameters
{
	public class RebuildSnippetsForTaskParameters : ApiParameters
	{
		[BindRequired]
		[FromQuery(Name = "task_id")]
		public Guid TaskId { get; set; }
	}
}