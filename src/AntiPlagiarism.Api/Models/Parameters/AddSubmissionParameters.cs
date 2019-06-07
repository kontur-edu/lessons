using System;
using System.Runtime.Serialization;
using AntiPlagiarism.Api.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Api.Models.Parameters;

namespace AntiPlagiarism.Api.Models.Parameters
{
	[DataContract]
	[ModelBinder(typeof(JsonModelBinder), Name="parameters")]
	public class AddSubmissionParameters : ApiParameters
	{
		[DataMember(Name = "task_id", IsRequired = true)]
		public Guid TaskId { get; set; }
		
		[DataMember(Name = "author_id", IsRequired = true)]
		public Guid AuthorId { get; set; }
		
		[DataMember(Name = "code", IsRequired = true)]
		public string Code { get; set; }
		
		[DataMember(Name = "language", IsRequired = true)]
		public Language Language { get; set; }

		[DataMember(Name = "additional_info")]
		public string AdditionalInfo { get; set; } = "";

		public override string ToString()
		{
			return $"AddCodeParameters(TaskId={TaskId}, AuthorId={AuthorId}, Code={Code?.Substring(0, Math.Min(Code.Length, 50))?.Replace("\n", @"\\")}...)";
		}
	}
}