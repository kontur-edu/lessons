using System;
using System.ComponentModel.DataAnnotations;

namespace uLearn.Web.Models
{
	public class GraderClient
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		[StringLength(64)]
		public string CourseId { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		[Required]
		[StringLength(64)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }
	}
}