using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class CourseVersion
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		[StringLength(64)]
		[Index("IDX_CourseVersion_ByCourseAndPublishTime", 1)]
		[Index("IDX_CourseVersion_ByCourseAndLoadingTime", 1)]
		public string CourseId { get; set; }

		[Required]
		[Index("IDX_CourseVersion_ByCourseAndLoadingTime", 2)]
		public DateTime LoadingTime { get; set; }

		[Index("IDX_CourseVersion_ByCourseAndPublishTime", 2)]
		public DateTime? PublishTime { get; set; }

		[Required]
		[StringLength(64)]
		public string AuthorId { get; set; }

		public virtual ApplicationUser Author { get; set; }
		
		// Репозиторий, откуда взята эта версия, по аналогии с git@github.com:vorkulsky/git_test.git
		public string RepoUrl { get; set; }

		[StringLength(40)]
		public string CommitHash { get; set; }

		public string Description { get; set; }

		// Устанавливается из настройки курса или как пусть единственного course.xml, если курс загружен из репозитория
		public string PathToCourseXml { get; set; } 
	}
}