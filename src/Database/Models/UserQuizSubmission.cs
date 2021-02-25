using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class UserQuizSubmission : ITimedSlideAction
	{
		private const string Course_Slide_User = "Course_Slide_User";
		private const string Course_Slide = "Course_Slide";
		private const string Course_Slide_Time = "Course_Slide_Time";

		[Required]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(40)]
		[Index(Course_Slide_User, 3)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(100)]
		[Index(Course_Slide_User, 1)]
		[Index(Course_Slide, 1)]
		[Index(Course_Slide_Time, 1)]
		public string CourseId { get; set; }

		[Required]
		[Index(Course_Slide_User, 2)]
		[Index(Course_Slide, 2)]
		[Index(Course_Slide_Time, 2)]
		public Guid SlideId { get; set; }

		[Required]
		[Index(Course_Slide_Time, 3)]
		public DateTime Timestamp { get; set; }

		public virtual AutomaticQuizChecking AutomaticChecking { get; set; }

		public virtual ManualQuizChecking ManualChecking { get; set; }
	}
}