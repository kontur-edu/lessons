using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class StepikExportSlideAndStepMap
	{
		private const string UlearnCourse_StepikCourse = "UlearnCourse_StepikCourse";
		private const string UlearnCourse_Slide = "UlearnCourse_Slide";

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		[Index("UlearnCourseId")]
		[Index(UlearnCourse_StepikCourse, 1)]
		[Index(UlearnCourse_Slide, 1)]
		public string UlearnCourseId { get; set; }

		[Required]
		[Index(UlearnCourse_StepikCourse, 2)]
		public int StepikCourseId { get; set; }

		[Required]
		[Index(UlearnCourse_Slide, 2)]
		public Guid SlideId { get; set; }

		[Required]
		public int StepId { get; set; }

		[Required]
		public string SlideXml { get; set; }
	}
}