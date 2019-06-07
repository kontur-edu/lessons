﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using Ulearn.Common.Api.Models.Responses;

namespace AntiPlagiarism.Api.Models.Results
{
	[DataContract]
	public class GetSubmissionPlagiarismsResponse : SuccessResponse
	{
		[DataMember(Name = "submission")]
		public SubmissionInfo SubmissionInfo { get; set; }
		
		[DataMember(Name = "plagiarisms")]
		public List<Plagiarism> Plagiarisms { get; set; }
		
		[DataMember(Name = "tokens_positions")]
		public List<TokenPosition> TokensPositions { get; set; }
		
		[DataMember(Name = "suspicion_levels")]
		public SuspicionLevels SuspicionLevels { get; set; } 
		
		[DataMember(Name = "analyzed_code_units")]
		public List<AnalyzedCodeUnit> AnalyzedCodeUnits { get; set; }

		public GetSubmissionPlagiarismsResponse()
		{
			Plagiarisms = new List<Plagiarism>();
		}
	}
}
