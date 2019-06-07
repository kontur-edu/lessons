using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class CertificateTemplateArchive
	{
		[Key]
		[StringLength(128)]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public string ArchiveName { get; set; }
		
		[Required]
		[Index("IDX_CertificateTemplateArchives_CertificateTemplateId")]
		public Guid CertificateTemplateId { get; set; }
		
		[Required]
		public byte[] Content { get; set; }
		
		public virtual CertificateTemplate CertificateTemplate { get; set; }
	}
}