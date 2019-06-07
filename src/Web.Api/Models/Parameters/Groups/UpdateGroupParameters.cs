using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Api.Models.Validations;
using Ulearn.Web.Api.Models.Binders;

namespace Ulearn.Web.Api.Models.Parameters.Groups
{
	[DataContract]
	public class UpdateGroupParameters
	{
		[DataMember]
		[NotEmpty(ErrorMessage = "Group name can not be empty", CanBeNull = true)]
		public string Name { get; set; }
		
		[DataMember]
		public bool? IsArchived { get; set; }
		
		[DataMember]
		public bool? IsInviteLinkEnabled { get; set; }
		
		
		[DataMember]
		public bool? IsManualCheckingEnabled { get; set; }
		
		[DataMember]
		public bool? IsManualCheckingEnabledForOldSolutions { get; set; }
		
		[DataMember]
		public bool? DefaultProhibitFurtherReview { get; set; }
		
		[DataMember]
		public bool? CanStudentsSeeGroupProgress { get; set; }
	}
}