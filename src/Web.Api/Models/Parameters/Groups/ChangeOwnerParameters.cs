using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Ulearn.Web.Api.Models.Binders;

namespace Ulearn.Web.Api.Models.Parameters.Groups
{
	[DataContract]
	public class ChangeOwnerParameters
	{
		[DataMember(IsRequired = true)]
		public string OwnerId { get; set; }
	}
}