using System;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
	public enum SlideRates
	{
		Good,
		NotUnderstand,
		Trivial,
		NotWatched
	}

	public class SlideRate : ISlideAction
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public SlideRates Rate { get; set; }

		[StringLength(64)]
		[Required]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[Required]
		[StringLength(64)]
		public string CourseId { get; set; }

		[Required]
		public Guid SlideId { get; set; }
	}
}