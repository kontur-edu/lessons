using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class CommentLike
	{
		private const string User_Comment = "User_Comment";

		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(64)]
		[Index(User_Comment, 1, IsUnique = true)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Index("Comment")]
		[Index(User_Comment, 2, IsUnique = true)]
		public int CommentId { get; set; }

		public virtual Comment Comment { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }
	}
}