using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

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
		
		[CanBeNull]
		public string RepoUrl { get; set; }
		
		[CanBeNull]
		public string Branch { get; set; } // Или хэш коммита
		
		[CanBeNull]
		public string PublicKey { get; set; }
		
		[CanBeNull]
		public string PrivateKey { get; set; }
		
		public bool IsWebhookEnabled { get; set; }
		
		[CanBeNull]
		public string PathToCourseXml { get; set; }
		
		[Required]
		public DateTime CreateTime { get; set; }
	}
}