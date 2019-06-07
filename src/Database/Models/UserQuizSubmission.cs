using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class UserQuizSubmission : ITimedSlideAction
	{
		[Required]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(40)]
		[Index("IDX_UserQuizSubmission_BySlideAndUser", 3)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(40)]
		[Index("IDX_UserQuizSubmission_BySlideAndUser", 1)]
		[Index("IDX_UserQuizSubmission_ByCourseAndSlide", 1)]
		[Index("IDX_UserQuizSubmission_BySlideAndTime", 1)]
		public string CourseId { get; set; }

		[Required]
		[Index("IDX_UserQuizSubmission_BySlideAndUser", 2)]
		[Index("IDX_UserQuizSubmission_ByCourseAndSlide", 2)]
		[Index("IDX_UserQuizSubmission_BySlideAndTime", 2)]
		public Guid SlideId { get; set; }

		[Required]
		[Index("IDX_UserQuizSubmission_BySlideAndTime", 3)]
		public DateTime Timestamp { get; set; }
		
		public virtual AutomaticQuizChecking AutomaticChecking { get; set; }
		
		public virtual ManualQuizChecking ManualChecking { get; set; }
	}
}