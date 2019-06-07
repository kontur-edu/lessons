using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class SlideHint : ISlideAction
	{
		[Key]
		public int Id { get; set; }

		public virtual ApplicationUser User { get; set; }

		[StringLength(64)]
		[Required]
		[Index("FullIndex", 3)]
		public string UserId { get; set; }

		[Required]
		[Index("FullIndex", 4)]
		public int HintId { get; set; }

		[Required]
		[StringLength(64)]
		[Index("FullIndex", 1)]
		public string CourseId { get; set; }

		[Required]
		[Index("FullIndex", 2)]
		public Guid SlideId { get; set; }

		[Index("FullIndex", 5)]
		public bool IsHintHelped { get; set; }
	}
}