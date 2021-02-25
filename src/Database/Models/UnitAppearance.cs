using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class UnitAppearance
	{
		private const string Course_Time = "Course_Time";

		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		[Index(Course_Time, 1)]
		public string CourseId { get; set; }

		[Required]
		public Guid UnitId { get; set; }

		[Required]
		public string UserName { get; set; }

		[Required]
		[Index(Course_Time, 2)]
		public DateTime PublishTime { get; set; }
	}
}