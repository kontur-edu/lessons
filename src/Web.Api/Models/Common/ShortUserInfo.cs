using System.Runtime.Serialization;

namespace Ulearn.Web.Api.Models.Common
{
	[DataContract]
	public class ShortUserInfo
	{
		[DataMember(Name = "id")]
		public string Id { get; set; }
		
		[DataMember(Name = "login")]
		public string Login { get; set; }
		
		[DataMember(Name = "first_name")]	
		public string FirstName { get; set; }
		
		[DataMember(Name = "last_name")]	
		public string LastName { get; set; }
		
		[DataMember(Name = "visible_name")]
		public string VisibleName { get; set; }
	}
}