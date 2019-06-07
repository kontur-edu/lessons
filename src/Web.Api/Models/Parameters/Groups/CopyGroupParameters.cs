using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Ulearn.Web.Api.Models.Parameters.Groups
{
	public class CopyGroupParameters
	{
		[FromQuery(Name = "destination_course_id")]
		[BindRequired]
		public string DestinationCourseId { get; set; }

		[FromQuery(Name = "make_me_owner")]
		public bool ChangeOwner { get; set; } = false;
	}
}