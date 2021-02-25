using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace Database.Models
{
	public class CourseAccess
	{
		private const string Course_IsEnabled = "Course_IsEnabled";
		private const string Course_User_IsEnabled = "Course_User_IsEnabled";

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		[Index("Course")]
		[Index(Course_IsEnabled, 1)]
		[Index(Course_User_IsEnabled, 1)]
		public string CourseId { get; set; }

		[StringLength(64)]
		[Index("User")]
		[Index(Course_User_IsEnabled, 2)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[StringLength(64)]
		public string GrantedById { get; set; }

		public virtual ApplicationUser GrantedBy { get; set; }

		[Required]
		public CourseAccessType AccessType { get; set; }

		[Index("GrantTime")]
		public DateTime GrantTime { get; set; }

		[Required]
		[Index(Course_IsEnabled, 2)]
		[Index(Course_User_IsEnabled, 3)]

		public bool IsEnabled { get; set; }

		[CanBeNull]
		public string Comment { get; set; }
	}

	public enum CourseAccessType : short
	{
		/* Редактировать, закреплять, удалять (скрывать) комментарии */
		[Display(Name = "Редактировать и удалять комментарии")]
		EditPinAndRemoveComments = 1,

		/* Смотреть решения всех, а не только студентов своих групп */
		[Display(Name = "Видеть решения всех пользователей")]
		ViewAllStudentsSubmissions = 2,

		[Display(Name = "Назначать преподавателей")]
		AddAndRemoveInstructors = 3,

		[Display(Name = "Видеть, в каких группах состоят все студенты")]
		ViewAllGroupMembers = 4,

		[Display(Name = "Получать в АПИ статистику по код-ревью (/codereveiew/statistics)")]
		ApiViewCodeReviewStatistics = 101,

		/*
		// Antiplagiarism service is enabled for everyone now. But don't use value 1001 for another features to avoid collissions.		
		[Display(Name = "Фича: антиплагиат")]
		FeatureUseAntiPlagiarism = 1001,
		*/
	}
}