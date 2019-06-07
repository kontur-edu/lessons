using System;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
	public class CourseVersion
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		[StringLength(64)]
		public string CourseId { get; set; }

		[Required]
		public DateTime LoadingTime { get; set; }

		public DateTime? PublishTime { get; set; }

		[Required]
		[StringLength(64)]
		public string AuthorId { get; set; }

		public virtual ApplicationUser Author { get; set; }
		
		public string RepoUrl { get; set; }

		[StringLength(40)]
		public string CommitHash { get; set; }

		public string Description { get; set; }

		// Устанавливается из настройки курса или как пусть единственного course.xml, если курс загружен из репозитория
		public string PathToCourseXml { get; set; }
	}
}