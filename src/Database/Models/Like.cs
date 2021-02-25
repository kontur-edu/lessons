using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class Like
	{
		private const string User_Submission = "User_Submission";

		[Key]
		public int Id { get; set; }

		public virtual UserExerciseSubmission Submission { get; set; }

		[Required]
		[Index(User_Submission, 2)]
		[Index("Submission")]
		public int SubmissionId { get; set; }

		[Required]
		[StringLength(64)]
		[Index(User_Submission, 1)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }
	}
}