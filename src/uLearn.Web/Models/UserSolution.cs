using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uLearn.Web.Models
{
	public class UserSolution : ISlideAction
	{
		[Required]
		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(64)]
		public string CourseId { get; set; }

		[Required]
		[StringLength(64)]
		public string SlideId { get; set; }

		public virtual ApplicationUser User { get; set; }

		[StringLength(64)]
		public string UserId { get; set; }

		public virtual TextBlob SolutionCode { get; set; }

		[StringLength(40)]
		[Required]
		public string SolutionCodeHash { get; set; }

		[Required]
		public int CodeHash { get; set; }

		public virtual IList<Like> Likes { get; set; }

		[Required]
		public DateTime Timestamp { get; set; }

		[Required]
		[Index]
		public bool IsRightAnswer { get; set; }

		[Required]
		public bool IsCompilationError { get; set; }

		public virtual TextBlob CompilationError { get; set; }

		[StringLength(40)]
		public string CompilationErrorHash { get; set; }

		public virtual TextBlob Output { get; set; }

		[StringLength(40)]
		public string OutputHash { get; set; }

		public string GetVerdict()
		{
			if (IsCompilationError) return "CompilationError";
			if (!IsRightAnswer) return "Wrong Answer";
			
			return "Accepted";
		}
	}
}