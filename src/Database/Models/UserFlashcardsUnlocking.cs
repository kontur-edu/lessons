using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class UserFlashcardsUnlocking
	{
		private const string User_Course_Unit = "User_Course_Unit";

		[Key]
		public int Id { get; set; }

		[Required]
		[Index(User_Course_Unit, 1, IsUnique = false)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(100)]
		[Index(User_Course_Unit, 2, IsUnique = false)]
		public string CourseId { get; set; }

		[Required]
		[Index(User_Course_Unit, 3, IsUnique = false)]
		public Guid UnitId { get; set; }
	}
}