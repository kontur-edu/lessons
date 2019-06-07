using System.Collections.Generic;
using System.Runtime.Serialization;
using Database.Models;
using Ulearn.Common.Api.Models.Responses;
using Ulearn.Web.Api.Models.Common;

namespace Ulearn.Web.Api.Models.Responses.Account
{
	[DataContract]
	public class GetMeResponse : SuccessResponse
	{
		[DataMember]
		public bool IsAuthenticated { get; set; }
		
		[DataMember(EmitDefaultValue = false)]
		public ShortUserInfo User { get; set; }

		[DataMember(EmitDefaultValue = false)]
		public List<AccountProblem> AccountProblems { get; set; }
		
		[DataMember]
		public List<SystemAccessType> SystemAccesses { get; set; }
	}

	[DataContract]
	public class AccountProblem
	{
		[DataMember]
		public string Title { get; set; }
		
		[DataMember]
		public string Description { get; set; }

		public AccountProblem()
		{
		}
		
		public AccountProblem(string title, string description)
		{
			Title = title;
			Description = description;
		}
	}
}