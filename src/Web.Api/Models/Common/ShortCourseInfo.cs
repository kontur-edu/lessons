using System.Runtime.Serialization;

namespace Ulearn.Web.Api.Models.Common
{
	[DataContract]
	public class ShortCourseInfo
	{
		[DataMember(Name = "id")]
		public string Id { get; set; }
		
		[DataMember(Name = "title")]
		public string Title { get; set; }
		
		[DataMember(Name = "api_url")]
		public string ApiUrl { get; set; }
	}
}