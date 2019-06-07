using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ulearn.Common.Api.Models.Parameters;
using Ulearn.Common.Api.Models.Validations;
using Ulearn.Web.Api.Authorization;

namespace Ulearn.Web.Api.Models.Parameters.Comments
{
	public class SlideCommentsParameters : IPaginationParameters, ICourseAuthorizationParameters
	{
		[FromQuery(Name = "course_id")]
		[BindRequired]
		public string CourseId { get; set; }
		
		[FromQuery(Name = "slide_id")]
		[BindRequired]
		public Guid SlideId { get; set; }
		
		[FromQuery(Name = "for_instructors")]
		public bool ForInstructors { get; set; }
		
		[FromQuery(Name = "offset")]
		[MinValue(0, ErrorMessage = "Offset should be non-negative")]
		public int Offset { get; set; }

		[FromQuery(Name = "count")]
		[MinValue(0, ErrorMessage = "Count should be non-negative")]
		[MaxValue(200, ErrorMessage = "Count should be at most 200")]
		public int Count { get; set; } = 100;
	}
}