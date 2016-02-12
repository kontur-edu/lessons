using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uLearn.Web.Models
{
	public class CommentLike
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(64)]
		[Index("IDX_CommentLike_ByUserAndComment", 1, IsUnique = true)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Index("IDX_CommentLike_ByComment")]
		[Index("IDX_CommentLike_ByUserAndComment", 2, IsUnique = true)]
		public int CommentId { get; set; }

		public virtual Comment Comment { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }
	}
}