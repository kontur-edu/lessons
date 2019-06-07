﻿using System.Runtime.Serialization;
using Ulearn.Common.Api.Models.Responses;

namespace Ulearn.Web.Api.Models.Responses.Account
{
	[DataContract]
	public class LogoutResponse : SuccessResponse
	{
		[DataMember]
		public bool Logout { get; set; }
	}
}