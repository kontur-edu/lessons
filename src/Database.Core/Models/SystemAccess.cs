using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Database.Models
{
	public class SystemAccess
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		
		[Required]
		[StringLength(64)]
		public string UserId { get; set; }
		
		public virtual ApplicationUser User { get; set; }
		
		[Required]
		[StringLength(64)]
		public string GrantedById { get; set; }

		public virtual ApplicationUser GrantedBy { get; set; }
		
		[Required]
		public SystemAccessType AccessType { get; set; }

		public DateTime GrantTime { get; set; }

		[Required]
		public bool IsEnabled { get; set; }
	}
	
	[JsonConverter(typeof(StringEnumConverter), true)]
	public enum SystemAccessType : short
	{
		[Display(Name = "Видеть профили всех пользователей")]
		ViewAllProfiles = 1,
		
		[Display(Name = "Видеть, в каких группах состоят все студенты")]
		ViewAllGroupMembers = 2,
	}
}