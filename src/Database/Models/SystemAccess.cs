using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class SystemAccess
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		
		[Required]
		[StringLength(64)]
		[Index("IDX_SystemAccess_ByUser")]
		[Index("IDX_SystemAccess_ByUserAndIsEnabled", 1)]
		public string UserId { get; set; }
		
		public virtual ApplicationUser User { get; set; }
		
		[Required]
		[StringLength(64)]
		public string GrantedById { get; set; }

		public virtual ApplicationUser GrantedBy { get; set; }
		
		[Required]
		public SystemAccessType AccessType { get; set; }

		[Index("IDX_SystemAccess_ByGrantTime")]
		public DateTime GrantTime { get; set; }

		[Required]
		[Index("IDX_SystemAccess_ByIsEnabled", 1)]
		[Index("IDX_SystemAccess_ByUserAndIsEnabled", 2)]
		public bool IsEnabled { get; set; }
	}
	
	public enum SystemAccessType : short
	{
		[Display(Name = "Видеть профили всех пользователей")]
		ViewAllProfiles = 1,
		
		[Display(Name = "Видеть, в каких группах состоят все студенты")]
		ViewAllGroupMembers = 2,		
	}
}