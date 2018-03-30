using System.Collections.Generic;
using System.Runtime.Serialization;
using Ulearn.Web.Api.Models.Common;

namespace Ulearn.Web.Api.Models.Results.Users
{
	[DataContract]
	public class InstructorsListResult
	{
		[DataMember(Name = "instructors")]
		public List<ShortUserInfo> Instructors { get; set; }
	}
}