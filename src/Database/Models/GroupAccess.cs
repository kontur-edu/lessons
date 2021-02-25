using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class GroupAccess
	{
		private const string Group_IsEnabled = "Group_IsEnabled";
		private const string Group_User_IsEnabled = "Group_User_IsEnabled";

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[Index("Group")]
		[Index(Group_IsEnabled, 1)]
		[Index(Group_User_IsEnabled, 1)]
		public int GroupId { get; set; }

		public virtual Group Group { get; set; }

		[StringLength(64)]
		[Index("User")]
		[Index(Group_User_IsEnabled, 2)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[StringLength(64)]
		public string GrantedById { get; set; }

		public virtual ApplicationUser GrantedBy { get; set; }

		[Required]
		public GroupAccessType AccessType { get; set; }

		[Index("GrantTime")]
		public DateTime GrantTime { get; set; }

		[Required]
		[Index(Group_IsEnabled, 2)]
		[Index(Group_User_IsEnabled, 3)]
		public bool IsEnabled { get; set; }
	}

	public enum GroupAccessType : short
	{
		FullAccess = 1,

		/* Can't be stored in database. Only for internal needs */
		Owner = 100,
	}
}