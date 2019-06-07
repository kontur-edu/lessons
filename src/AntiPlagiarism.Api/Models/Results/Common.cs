using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AntiPlagiarism.Api.Models.Results
{
	[DataContract]
	public class Plagiarism
	{
		[DataMember(Name = "submission")]
		public SubmissionInfo SubmissionInfo { get; set; }
	
		[DataMember(Name = "weight")]
		public double Weight { get; set; }
	
		[DataMember(Name = "analyzed_code_units")]
		public List<AnalyzedCodeUnit> AnalyzedCodeUnits { get; set; }
	
		[DataMember(Name = "tokens_positions")]
		public List<TokenPosition> TokensPositions { get; set; }
	
		[DataMember(Name = "matched_snippets")]
		public List<MatchedSnippet> MatchedSnippets { get; set; }
	}

	[DataContract]
	public class SubmissionInfo
	{
		[DataMember(Name = "id")]
		public int Id { get; set; }
	
		[DataMember(Name = "code")]
		public string Code { get; set; }
	
		[DataMember(Name = "task_id")]
		public Guid TaskId { get; set; }
	
		[DataMember(Name = "author_id")]
		public Guid AuthorId { get; set; }

		[DataMember(Name = "additional_info")] // Должно включать данные класса AdditionalInfo
		public string AdditionalInfo { get; set; }
	}

	[DataContract]
	public class AdditionalInfo
	{
		[DataMember(Name = "SubmissionId")]
		public int SubmissionId { get; set; }
	}

	[DataContract]
	public class AnalyzedCodeUnit
	{
		[DataMember(Name = "name")]
		public string Name { get; set; }
	
		[DataMember(Name = "first_token_index")]
		public int FirstTokenIndex { get; set; }
	
		[DataMember(Name = "tokens_count")]
		public int TokensCount { get; set; }
	}

	[DataContract]
	public class TokenPosition
	{
		[DataMember(Name = "token_index")]
		public int TokenIndex { get; set; }
	
		[DataMember(Name = "start_position")]
		public int StartPosition { get; set; }
	
		[DataMember(Name = "length")]
		public int Length { get; set; }
	}

	[DataContract]
	public class MatchedSnippet
	{
		[DataMember(Name = "snippet_type")]
		public SnippetType SnippetType { get; set; }
	
		[DataMember(Name = "snippet_tokens_count")]
		public int TokensCount { get; set; }
	
		[DataMember(Name = "original_submission_first_token_index")]
		public int OriginalSubmissionFirstTokenIndex { get; set; }
	
		[DataMember(Name = "plagiarism_submission_first_token_index")]
		public int PlagiarismSubmissionFirstTokenIndex { get; set; }
		
		[DataMember(Name = "snippet_frequency")]
		public double SnippetFrequency { get; set; }
	}

	[DataContract]
	public class SuspicionLevels
	{
		[DataMember(Name = "faint_suspicion")]
		public double FaintSuspicion { get; set; }
		
		[DataMember(Name = "strong_suspicion")]
		public double StrongSuspicion { get; set; }
	}	
}