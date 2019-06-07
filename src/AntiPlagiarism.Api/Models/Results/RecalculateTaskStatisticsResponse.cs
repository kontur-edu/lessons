using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ulearn.Common.Api.Models.Responses;

namespace AntiPlagiarism.Api.Models.Results
{
	[DataContract]
	public class RecalculateTaskStatisticsResponse : SuccessResponse
	{
		[DataMember(Name = "task_ids")]
		public List<Guid> TaskIds { get; set; }
		
		[DataMember(Name = "weights")]
		public Dictionary<Guid, List<double>> Weights { get; set; } 
	}
}