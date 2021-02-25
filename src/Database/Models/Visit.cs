using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class Visit : ITimedSlideAction
	{
		private const string Slide_User = "Slide_User";
		private const string Course_Slide_User = "Course_Slide_User";
		private const string Slide_Time = "Slide_Time";

		[Key]
		public int Id { get; set; }

		public virtual ApplicationUser User { get; set; }

		[StringLength(64)]
		[Required]
		[Index(Slide_User, 1)]
		[Index(Course_Slide_User, 3)]
		public string UserId { get; set; }

		[Required]
		[StringLength(100)]
		[Index(Course_Slide_User, 1)]
		public string CourseId { get; set; }

		[Required]
		[Index(Slide_User, 2)]
		[Index(Slide_Time, 1)]
		[Index(Course_Slide_User, 2)]
		public Guid SlideId { get; set; }

		///<summary>Первый заход на слайд</summary>
		[Required]
		[Index(Slide_Time, 2)]
		public DateTime Timestamp { get; set; }

		public int Score { get; set; }
		public bool HasManualChecking { get; set; }
		[Obsolete("Фактически не используется")]
		public int AttemptsCount { get; set; }
		public bool IsSkipped { get; set; }
		// Условие выставления в SlideCheckingsRepo.IsSlidePassed
		public bool IsPassed { get; set; }

		public string IpAddress { get; set; }
	}
}