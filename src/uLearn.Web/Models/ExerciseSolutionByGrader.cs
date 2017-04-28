using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace uLearn.Web.Models
{
	public class ExerciseSolutionByGrader
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public Guid ClientId { get; set; }
		public virtual GraderClient Client { get; set; }

		[Required]
		public int SubmissionId { get; set; }
		public virtual UserExerciseSubmission Submission { get; set; }

		[Required(AllowEmptyStrings = true)]
		/* Some additional info about user from grader client */
		public string ClientUserId { get; set; }
	}
}