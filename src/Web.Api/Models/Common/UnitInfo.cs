using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ulearn.Web.Api.Models.Common
{
	[DataContract]
	public class UnitInfo
	{
		[DataMember(Name = "id")]
		public Guid Id { get; set; }
		
		[DataMember(Name = "title")]
		public string Title { get; set; }
		
		[DataMember(Name = "slides")]
		public List<SlideInfo> Slides { get; set; }
	}
}