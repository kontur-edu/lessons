using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
	public class CourseVersion
	{
		private const string Course_PublishTime = "Course_PublishTime";
		private const string Course_LoadingTime = "Course_LoadingTime";

		[Key]
		public Guid Id { get; set; }

		[Required]
		[StringLength(100)]
		[Index(Course_PublishTime, 1)]
		[Index(Course_LoadingTime, 1)]
		public string CourseId { get; set; }

		[Required]
		[Index(Course_LoadingTime, 2)]
		public DateTime LoadingTime { get; set; }

		[Index(Course_PublishTime, 2)]
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