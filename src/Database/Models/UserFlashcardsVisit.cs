using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class UserFlashcardsVisit
	{
		private const string User_Course_Unit_Flashcard = "User_Course_Unit_Flashcard";

		[Key]
		public int Id { get; set; }

		[Required]
		[Index(User_Course_Unit_Flashcard, 1, IsUnique = false)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(100)]
		[Index(User_Course_Unit_Flashcard, 2, IsUnique = false)]
		public string CourseId { get; set; }

		[Required]
		[Index(User_Course_Unit_Flashcard, 3, IsUnique = false)]
		public Guid UnitId { get; set; }

		[Required]
		[StringLength(64)]
		[Index(User_Course_Unit_Flashcard, 4, IsUnique = false)]
		public string FlashcardId { get; set; }

		[Required]
		public Rate Rate { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }
	}

	public enum Rate
	{
		NotRated,
		Rate1,
		Rate2,
		Rate3,
		Rate4,
		Rate5
	}
}