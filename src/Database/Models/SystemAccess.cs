using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class SystemAccess
	{
		private const string User_IsEnabled = "User_IsEnabled";

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(64)]
		[Index("User")]
		[Index(User_IsEnabled, 1)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(64)]
		public string GrantedById { get; set; }

		public virtual ApplicationUser GrantedBy { get; set; }

		[Required]
		public SystemAccessType AccessType { get; set; }

		[Index("GrantTime")]
		public DateTime GrantTime { get; set; }

		[Required]
		[Index("IsEnabled", 1)]
		[Index(User_IsEnabled, 2)]
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