using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ulearn.Common.Api.Models.Validations;
using Ulearn.Web.Api.Authorization;
using Ulearn.Web.Api.Models.Binders;

namespace Ulearn.Web.Api.Models.Parameters.Groups
{
	[DataContract]
	public class CreateGroupParameters
	{
		[DataMember(IsRequired = true)]
		[NotEmpty(ErrorMessage = "Group name can not be empty")]
		public string Name { get; set; }
	}
}