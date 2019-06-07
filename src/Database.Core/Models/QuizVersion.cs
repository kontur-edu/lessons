using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Ulearn.Core.Courses;
using Ulearn.Core.Courses.Slides;
using Ulearn.Core.Courses.Slides.Quizzes;
using Ulearn.Core.Courses.Units;

namespace Database.Models
{
	/*
	public class QuizVersion
	{
		private static readonly XmlSlideLoader loader = new XmlSlideLoader();
		
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(64)]
		public string CourseId { get; set; }

		[Required]
		public Guid SlideId { get; set; }

		[Required]
		public string NormalizedXml { get; set; }

		[Required]
		public DateTime LoadingTime { get; set; }

		public QuizSlide GetRestoredQuiz(Course course, Unit unit)
		{
			var xmlBytes = Encoding.UTF8.GetBytes(NormalizedXml);
			return (QuizSlide) loader.Load(xmlBytes, 0, unit, course.Id, course.Settings);
		}
	}
	*/
}