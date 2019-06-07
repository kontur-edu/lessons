using System;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
	public class Visit : ITimedSlideAction
	{
		[Key]
		public int Id { get; set; }

		public virtual ApplicationUser User { get; set; }

		[StringLength(64)]
		[Required]
		public string UserId { get; set; }

		[Required]
		[StringLength(64)]
		public string CourseId { get; set; }

		[Required]
		public Guid SlideId { get; set; }

		///<summary>Первый заход на слайд</summary>
		[Required]
		public DateTime Timestamp { get; set; }

		public int Score { get; set; }
		public bool HasManualChecking { get; set; }
		public int AttemptsCount { get; set; }
		public bool IsSkipped { get; set; }
		public bool IsPassed { get; set; }
		
		public string IpAddress { get; set; }
	}
}