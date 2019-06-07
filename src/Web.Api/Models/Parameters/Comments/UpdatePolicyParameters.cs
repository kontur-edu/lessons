using System.Runtime.Serialization;
using Database.Models;
using Database.Models.Comments;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Web.Api.Models.Binders;

namespace Ulearn.Web.Api.Models.Parameters.Comments
{
	[DataContract]
	public class UpdatePolicyParameters
	{
		[DataMember]
		public bool? AreCommentsEnabled { get; set; }
		
		[DataMember]
		public CommentModerationPolicy? ModerationPolicy { get; set; }
		
		[DataMember]
		public bool? OnlyInstructorsCanReply { get; set; }
	}
}