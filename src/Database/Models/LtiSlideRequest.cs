using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class LtiSlideRequest
	{
		private const string SlideUserIndexName = "IDX_LtiSlideRequest_SlideAndUser";

		[Key]
		public int RequestId { get; set; }

		[Required]
		[StringLength(100)]
		[Index(SlideUserIndexName, 1)]
		public string CourseId { get; set; }
		
		[Required]
		[Index(SlideUserIndexName, 2)]
		public Guid SlideId { get; set; }

		[Required]
		[StringLength(64)]
		[Index(SlideUserIndexName, 3)]
		public string UserId { get; set; }

		[Required]
		public string Request { get; set; }
	}
}