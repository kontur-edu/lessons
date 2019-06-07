using System.Collections.Generic;
using System.Runtime.Serialization;
using Ulearn.Common.Api.Models.Responses;

namespace Ulearn.Web.Api.Models.Responses.Groups
{
	[DataContract]
	public class GroupsListResponse : PaginatedResponse
	{
		[DataMember]
		public List<GroupInfo> Groups { get; set; }
	}
}