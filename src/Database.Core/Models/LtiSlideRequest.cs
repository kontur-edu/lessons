using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	/* For backward compatibility: EF Core changed table naming convention.
	See https://github.com/aspnet/Announcements/issues/167 for details */
	[Table("LtiSlideRequests")]
	public class LtiSlideRequest
	{
		[Key]
		public int RequestId { get; set; }

		[Required]
		[StringLength(100)]
		public string CourseId { get; set; }
		
		[Required]
		public Guid SlideId { get; set; }

		[Required]
		[StringLength(64)]
		public string UserId { get; set; }

		[Required]
		public string Request { get; set; }
	}
}