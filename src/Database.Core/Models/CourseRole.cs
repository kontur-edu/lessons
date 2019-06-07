using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	[Table("UserRoles")]
	public class CourseRole
	{
		[Required]
		[Key]
		public int Id { get; set; }

		public virtual ApplicationUser User { get; set; }

		[StringLength(64)]
		public string UserId { get; set; }

		[Required]
		public string CourseId { get; set; }

		[Required]
		public CourseRoleType Role { get; set; }
	}
}