using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class AdditionalScore
	{
		private const string Course_User = "Course_User";
		private const string Course_Unit_ScoringGroup_User = "Course_Unit_ScoringGroup_User";
		private const string Unit_User = "Unit_User";

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		[Index(Course_User, 1)]
		[Index(Course_Unit_ScoringGroup_User, 1, IsUnique = true)]
		public string CourseId { get; set; }

		[Required]
		[Index("Unit")]
		[Index(Unit_User, 1)]
		[Index(Course_Unit_ScoringGroup_User, 2, IsUnique = true)]
		public Guid UnitId { get; set; }

		[Required]
		[StringLength(64)]
		[Index(Course_Unit_ScoringGroup_User, 3, IsUnique = true)]
		public string ScoringGroupId { get; set; }

		[Required]
		[StringLength(64)]
		[Index(Unit_User, 2)]
		[Index(Course_User, 2)]
		[Index(Course_Unit_ScoringGroup_User, 4, IsUnique = true)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		public int Score { get; set; }

		[Required]
		[StringLength(64)]
		public string InstructorId { get; set; }

		public virtual ApplicationUser Instructor { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }
	}
}