using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class CertificateTemplate
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		[StringLength(40)]
		[Index("IDX_CertificateTemplate_ByCourse")]
		public string CourseId { get; set; }

		[Required]
		public string Name { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }

		[Required]
		public bool IsDeleted { get; set; }

		[StringLength(128)]
		public string ArchiveName { get; set; }

		public virtual ICollection<Certificate> Certificates { get; set; }

		public bool IsUploaded => !string.IsNullOrEmpty(ArchiveName);
	}
}