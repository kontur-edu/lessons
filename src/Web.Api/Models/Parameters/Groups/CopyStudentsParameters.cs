using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Common.Api.Models.Validations;
using Ulearn.Web.Api.Models.Binders;

namespace Ulearn.Web.Api.Models.Parameters.Groups
{
	[DataContract]
	public class CopyStudentsParameters
	{
		[DataMember(IsRequired = true)]
		public List<string> StudentIds { get; set; }
	}
}