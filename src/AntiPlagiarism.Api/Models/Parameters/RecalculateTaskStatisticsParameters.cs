using System;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Api.Models.Parameters;

namespace AntiPlagiarism.Api.Models.Parameters
{
	public class RecalculateTaskStatisticsParameters : ApiParameters
	{
		[FromQuery(Name = "from_task")]
		public Guid? FromTaskId { get; set; }

		[FromQuery(Name = "task_id")]
		public Guid? TaskId { get; set; }
	}
}