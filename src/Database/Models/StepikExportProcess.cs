using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class StepikExportProcess
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string UlearnCourseId { get; set; }

		[Required]
		public int StepikCourseId { get; set; }

		[StringLength(100)]
		public string StepikCourseTitle { get; set; }

		[Required]
		public bool IsFinished { get; set; }

		[Required]
		public bool IsInitialExport { get; set; }

		public bool IsSuccess { get; set; }

		public string Log { get; set; }

		[Required]
		[StringLength(64)]
		[Index("Owner")]
		public string OwnerId { get; set; }

		public virtual ApplicationUser Owner { get; set; }

		[Required]
		public DateTime StartTime { get; set; }

		public DateTime? FinishTime { get; set; }
	}
}