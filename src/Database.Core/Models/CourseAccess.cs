using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Database.Models
{
	public class CourseAccess
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		
		[Required]
		[StringLength(64)]
		public string CourseId { get; set; }

		[StringLength(64)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[StringLength(64)]
		public string GrantedById { get; set; }

		public virtual ApplicationUser GrantedBy { get; set; }

		[Required]
		public CourseAccessType AccessType { get; set; }

		public DateTime GrantTime { get; set; }

		[Required]
		public bool IsEnabled { get; set; }
	}

	[JsonConverter(typeof(StringEnumConverter), true)]
	public enum CourseAccessType : short
	{
		/* Редактировать, закреплять, удалять (скрывать) комментарии, видеть неопубликованные комментарии */
		[Display(Name = "Редактировать и удалять комментарии")]
		EditPinAndRemoveComments = 1,

		/* Смотреть решения всех, а не только студентов своих групп */
		[Display(Name = "Видеть решения всех пользователей")]
		ViewAllStudentsSubmissions = 2,

		/* Назначать людей преподавателями */
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

	public static class CourseAccessTypeExtensions
	{
		public static string GetAuthorizationPolicyName(this CourseAccessType accessType)
		{
			return "CourseAccess." + Enum.GetName(typeof(CourseAccessType), accessType);
		}
	}
}
