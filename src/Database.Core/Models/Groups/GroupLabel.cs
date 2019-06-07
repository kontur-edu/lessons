using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class GroupLabel
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(64)]
		public string OwnerId { get; set; }

		public virtual ApplicationUser Owner { get; set; }

		[StringLength(100)]
		public string Name { get; set; }

		[StringLength(6)]
		public string ColorHex { get; set; }

		[Required]
		public bool IsDeleted { get; set; }
	}

	/* For backward compatibility: EF Core changed table naming convention.
	   See https://github.com/aspnet/Announcements/issues/167 for details */
	[Table("LabelOnGroups")]
	public class LabelOnGroup
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public int GroupId { get; set; }

		public virtual Group Group { get; set; }

		[Required]
		public int LabelId { get; set; }
		
		public virtual GroupLabel Label { get; set; }
	}
}
