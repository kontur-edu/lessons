using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;
using Database.DataContexts;
using Microsoft.AspNet.Identity;

namespace Database.Models
{
	public class AbstractSlideChecking : ITimedSlideAction
	{
		public virtual int Id { get; set; }

		[Required]
		[StringLength(64)]
		[Index("IDX_AbstractSlideChecking_AbstractSlideCheckingBySlide", 1)]
		[Index("IDX_AbstractSlideChecking_AbstractSlideCheckingBySlideAndTime", 1)]
		[Index("IDX_AbstractSlideChecking_AbstractSlideCheckingBySlideAndUser", 1)]
		[Index("IDX_AbstractSlideChecking_AbstractSlideCheckingByCourseAndUser", 1)]
		public string CourseId { get; set; }

		[Required]
		[Index("IDX_AbstractSlideChecking_AbstractSlideCheckingBySlide", 2)]
		[Index("IDX_AbstractSlideChecking_AbstractSlideCheckingBySlideAndTime", 2)]
		[Index("IDX_AbstractSlideChecking_AbstractSlideCheckingBySlideAndUser", 2)]
		public Guid SlideId { get; set; }

		[Required]
		[Index("IDX_AbstractSlideChecking_AbstractSlideCheckingBySlideAndTime", 3)]
		public DateTime Timestamp { get; set; }

		[Required]
		[StringLength(64)]
		[Index("IDX_AbstractSlideChecking_AbstractSlideCheckingBySlideAndUser", 3)]
		[Index("IDX_AbstractSlideChecking_AbstractSlideCheckingByCourseAndUser", 2)]
		public string UserId { get; set; }

		public virtual ApplicationUser User { get; set; }

		public int Score { get; set; }

		public virtual void PreRemove(ULearnDb db)
		{
		}
	}

	public class AbstractManualSlideChecking : AbstractSlideChecking
	{
		public DateTime? LockedUntil { get; set; }

		[StringLength(64)]
		public string LockedById { get; set; }

		public virtual ApplicationUser LockedBy { get; set; }

		public bool IsChecked { get; set; }

		public bool IsLocked => LockedUntil.HasValue && LockedUntil.Value > DateTime.Now;

		public bool IsLockedBy(IIdentity identity)
		{
			return IsLocked && LockedById == identity.GetUserId();
		}

		public bool HasLastLockedBy(IIdentity identity)
		{
			return LockedById == identity.GetUserId();
		}
	}

	public class AbstractAutomaticSlideChecking : AbstractSlideChecking
	{
	}

	public enum AutomaticExerciseCheckingStatus
	{
		Done = 0,
		Waiting = 1,
		NotFound = 2,
		AccessDeny = 3,
		Error = 4,
		Running = 5,
		RequestTimeLimit = 6
	}

	public class AutomaticExerciseChecking : AbstractAutomaticSlideChecking
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public override int Id { get; set; }
		
		public AutomaticExerciseCheckingStatus Status { get; set; }

		public TimeSpan? Elapsed { get; set; }

		public string DisplayName { get; set; }

		[Required]
		[Index("IDX_AutomaticExerciseChecking_ByIsRightanswer")]
		public bool IsRightAnswer { get; set; }

		[Required]
		public bool IsCompilationError { get; set; }

		public virtual TextBlob CompilationError { get; set; }

		[StringLength(40)]
		public string CompilationErrorHash { get; set; }

		public virtual TextBlob Output { get; set; }

		[StringLength(40)]
		public string OutputHash { get; set; }

		[StringLength(40)]
		public string ExecutionServiceName { get; set; }
		
		[StringLength(256)]
		public string CheckingAgentName { get; set; }

		public string GetVerdict()
		{
			if (IsCompilationError)
				return "CompilationError";
			if (!IsRightAnswer)
				return "Wrong Answer";

			return "Accepted";
		}
	}

	/* Manual Exercise Checking is Code Review */

	public class ManualExerciseChecking : AbstractManualSlideChecking
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public override int Id { get; set; }
		
		[Required]
		public int SubmissionId { get; set; }

		public virtual UserExerciseSubmission Submission { get; set; }
		
		[Required]
		[Index("IDX_AbstractSlideChecking_AbstractSlideCheckingBySlideAndUser", 4)]
		public bool ProhibitFurtherManualCheckings { get; set; }

		public virtual IList<ExerciseCodeReview> Reviews { get; set; }

		[NotMapped]
		public List<ExerciseCodeReview> NotDeletedReviews => Reviews.Where(r => !r.IsDeleted).ToList();

		public override void PreRemove(ULearnDb db)
		{
			db.Set<ExerciseCodeReview>().RemoveRange(Reviews);
		}
	}

	public class AutomaticQuizChecking : AbstractAutomaticSlideChecking
	{
		/* This field is not identity and is not database-generated because EF generates Id as foreign key to UserQuizSubmission.Id */
		[Key]
		public override int Id { get; set; }
		
		public virtual UserQuizSubmission Submission { get; set; }
	}

	public class ManualQuizChecking : AbstractManualSlideChecking
	{
		/* This field is not identity and is not database-generated because EF generates Id as foreign key to UserQuizSubmission.Id */
		[Key]
		public override int Id { get; set; }

		public virtual UserQuizSubmission Submission { get; set; }
	}
}