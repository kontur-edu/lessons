using System.Collections.Generic;
using System.Runtime.Serialization;
using Ulearn.Web.Api.Models.Common;

namespace Ulearn.Web.Api.Models.Results.Courses
{
	[DataContract]
	public class CoursesListResult
	{
		[DataMember(Name = "courses")]
		public List<ShortCourseInfo> Courses { get; set; }
	}
}