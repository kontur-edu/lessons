using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class UserQuizAnswer
	{
		private const string Submission_Block = "Submission_Block";

		[Required]
		[Key]
		public int Id { get; set; }

		[Index(Submission_Block, 1)]
		public int SubmissionId { get; set; }

		public virtual UserQuizSubmission Submission { get; set; }

		[StringLength(64)]
		[Index(Submission_Block, 2)]
		public string BlockId { get; set; }

		[StringLength(64)]
		[Index("Item")]
		public string ItemId { get; set; }

		[StringLength(8192)]
		public string Text { get; set; }

		[Required]
		// Корректный ли вариант ItemId
		public bool IsRightAnswer { get; set; }

		[Required]
		// Количество баллов за весь блок
		public int QuizBlockScore { get; set; }

		[Required]
		// Максимально возможное количество баллов за весь блок
		public int QuizBlockMaxScore { get; set; }

		public bool IsQuizBlockScoredMaximum => QuizBlockScore == QuizBlockMaxScore;
	}
}