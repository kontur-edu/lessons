using System;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
	public class UnitAppearance
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(64)]
		public string CourseId { get; set; }

		[Required]
		public Guid UnitId { get; set; }

		[Required]
		public string UserName { get; set; }

		[Required]
		public DateTime PublishTime { get; set; }
	}
}