using System;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
	public class Certificate
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid TemplateId { get; set; }

		public virtual CertificateTemplate Template { get; set; }

		[Required]
		[StringLength(64)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(64)]
		public string InstructorId { get; set; }

		public virtual ApplicationUser Instructor { get; set; }

		[Required]
		public string Parameters { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }

		[Required]
		public bool IsDeleted { get; set; }

		/* Certificates previews are visible only for instructors and not listed in any list */

		[Required]
		public bool IsPreview { get; set; }
	}
}