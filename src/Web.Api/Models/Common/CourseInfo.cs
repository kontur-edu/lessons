using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ulearn.Web.Api.Models.Common
{
	[DataContract]
	public class CourseInfo
	{
		[DataMember]
		public string Id { get; set; }
		
		[DataMember]
		public string Title { get; set; }
		
		[DataMember]
		public List<UnitInfo> Units { get; set; }
	}
}