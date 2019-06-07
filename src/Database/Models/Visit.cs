using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class Visit : ITimedSlideAction
	{
		[Key]
		public int Id { get; set; }

		public virtual ApplicationUser User { get; set; }

		[StringLength(64)]
		[Required]
		[Index("IDX_Visits_BySlideAndUser", 1)]
		[Index("IDX_Visits_ByCourseSlideAndUser", 3)]
		public string UserId { get; set; }

		[Required]
		[StringLength(64)]
		[Index("IDX_Visits_ByCourseSlideAndUser", 1)]
		public string CourseId { get; set; }

		[Required]
		[Index("IDX_Visits_BySlideAndUser", 2)]
		[Index("IDX_Visits_BySlideAndTime", 1)]
		[Index("IDX_Visits_ByCourseSlideAndUser", 2)]
		public Guid SlideId { get; set; }

		///<summary>Первый заход на слайд</summary>
		[Required]
		[Index("IDX_Visits_BySlideAndTime", 2)]
		public DateTime Timestamp { get; set; }

		public int Score { get; set; }
		public bool HasManualChecking { get; set; }
		public int AttemptsCount { get; set; }
		public bool IsSkipped { get; set; }
		public bool IsPassed { get; set; }
		
		public string IpAddress { get; set; } 
	}
}