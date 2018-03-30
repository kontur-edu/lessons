using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ulearn.Web.Api.Models.Common
{
	[DataContract]
	public class CourseInfo
	{
		[DataMember(Name = "id")]
		public string Id { get; set; }
		
		[DataMember(Name = "title")]
		public string Title { get; set; }
		
		[DataMember(Name = "units")]
		public List<UnitInfo> Units { get; set; }
	}
}