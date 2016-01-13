using System.ComponentModel.DataAnnotations;

namespace uLearn.Web.Models
{
	public class UserRole
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
		public CourseRoles Role { get; set; }
	}
}