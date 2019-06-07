using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class StepikExportSlideAndStepMap
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string UlearnCourseId { get; set; }

		[Required]
		public int StepikCourseId { get; set; }

		[Required]
		public Guid SlideId { get; set; }

		[Required]
		public int StepId { get; set; }

		[Required]
		public string SlideXml { get; set; }
	}
}
