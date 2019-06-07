using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	[Table("CourseGitRepos")]
	public class CourseGit
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		
		[StringLength(64)]
		public string CourseId { get; set; }
		
		public string RepoUrl { get; set; }
		
		public string Branch { get; set; } // Или хэш коммита
		
		public string PublicKey { get; set; }
		
		public string PrivateKey { get; set; }
		
		public bool IsWebhookEnabled { get; set; }
		
		public string PathToCourseXml { get; set; }
		
		[Required]
		public DateTime CreateTime { get; set; }
	}
}