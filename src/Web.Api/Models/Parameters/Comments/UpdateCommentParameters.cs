using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Database.Models;
using Database.Models.Comments;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Api.Models.Validations;
using Ulearn.Web.Api.Models.Binders;

namespace Ulearn.Web.Api.Models.Parameters.Comments
{
	[DataContract]
	public class UpdateCommentParameters
	{
		[CanBeNull]
		[DataMember]
		[NotEmpty(CanBeNull = true, ErrorMessage = "Text can not be empty")]
		[MaxLength(CommentsPolicy.MaxCommentLength, ErrorMessage = "Comment is too large. Max allowed length is 10000 chars")]
		public string Text { get; set; }
		
		[DataMember]
		public bool? IsApproved { get; set; }
		
		[DataMember]
		public bool? IsPinnedToTop { get; set; }
		
		[DataMember]
		public bool? IsCorrectAnswer { get; set; }
	}
}