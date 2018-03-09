using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Database.Migrations;
using Database.Models;
using EntityFramework.Functions;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Database.DataContexts
{
	public class ULearnDb : IdentityDbContext<ApplicationUser>
	{
		public ULearnDb()
			: base("DefaultConnection", throwIfV1Schema: false)
		{
			System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<ULearnDb, Configuration>());
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			/* See https://weblogs.asp.net/dixin/entityframework.functions
			 * for detailed description about working with stored functions and procedures */
			modelBuilder.AddFunctions<UsersRepo>();
			modelBuilder.AddFunctions<GradersRepo>();

			modelBuilder.Entity<CommentLike>()
				.HasRequired(x => x.Comment)
				.WithMany(x => x.Likes)
				.HasForeignKey(x => x.CommentId)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<GroupMember>()
				.HasRequired(m => m.Group)
				.WithMany(g => g.Members)
				.HasForeignKey(m => m.GroupId)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<Like>()
				.HasRequired(l => l.Submission)
				.WithMany(s => s.Likes)
				.HasForeignKey(l => l.SubmissionId)
				.WillCascadeOnDelete(false);

			modelBuilder.Entity<NotificationDelivery>()
				.HasRequired(d => d.Notification)
				.WithMany(n => n.Deliveries)
				.HasForeignKey(d => d.NotificationId)
				.WillCascadeOnDelete(false);

			CancelCascaseDeleting<ExerciseCodeReview, ApplicationUser, string>(modelBuilder, c => c.Author, c => c.AuthorId);

			CancelCascaseDeleting<UserExerciseSubmission, ApplicationUser, string>(modelBuilder, c => c.User, c => c.UserId);
			CancelCascaseDeleting<ManualExerciseChecking, ApplicationUser, string>(modelBuilder, c => c.User, c => c.UserId);

			CancelCascaseDeleting<ExerciseCodeReviewComment, ApplicationUser, string>(modelBuilder, c => c.Author, c => c.AuthorId);
			//CancelCascaseDeleting<ExerciseCodeReviewComment, ExerciseCodeReview, int>(modelBuilder, c => c.Review, c => c.ReviewId);

			CancelCascaseDeleting<Certificate, ApplicationUser, string>(modelBuilder, c => c.User, c => c.UserId);
			CancelCascaseDeleting<Certificate, ApplicationUser, string>(modelBuilder, c => c.Instructor, c => c.InstructorId);

			CancelCascaseDeleting<AdditionalScore, ApplicationUser, string>(modelBuilder, c => c.User, c => c.UserId);
			CancelCascaseDeleting<AdditionalScore, ApplicationUser, string>(modelBuilder, c => c.Instructor, c => c.InstructorId);

			CancelCascaseDeleting<GraderClient, ApplicationUser, string>(modelBuilder, c => c.User, c => c.UserId);

			CancelCascaseDeleting<Notification, ApplicationUser, string>(modelBuilder, c => c.InitiatedBy, c => c.InitiatedById);
			CancelCascaseDeleting<AddedInstructorNotification, ApplicationUser, string>(modelBuilder, c => c.AddedUser, c => c.AddedUserId);
			CancelCascaseDeleting<LikedYourCommentNotification, ApplicationUser, string>(modelBuilder, c => c.LikedUser, c => c.LikedUserId);
			CancelCascaseDeleting<JoinedToYourGroupNotification, ApplicationUser, string>(modelBuilder, c => c.JoinedUser, c => c.JoinedUserId);
			CancelCascaseDeleting<JoinedToYourGroupNotification, Group, int>(modelBuilder, c => c.Group, c => c.GroupId);
			CancelCascaseDeleting<GrantedAccessToGroupNotification, GroupAccess, int>(modelBuilder, c => c.Access, c => c.AccessId);
			CancelCascaseDeleting<RevokedAccessToGroupNotification, GroupAccess, int>(modelBuilder, c => c.Access, c => c.AccessId);
			CancelCascaseDeleting<GroupMemberHasBeenRemovedNotification, Group, int>(modelBuilder, c => c.Group, c => c.GroupId);
			CancelCascaseDeleting<GroupMemberHasBeenRemovedNotification, ApplicationUser, string>(modelBuilder, c => c.User, c => c.UserId);
			CancelCascaseDeleting<CreatedGroupNotification, Group, int>(modelBuilder, c => c.Group, c => c.GroupId);
			CancelCascaseDeleting<PassedManualExerciseCheckingNotification, ManualExerciseChecking, int>(modelBuilder, c => c.Checking, c => c.CheckingId);
			CancelCascaseDeleting<PassedManualQuizCheckingNotification, ManualQuizChecking, int>(modelBuilder, c => c.Checking, c => c.CheckingId);
			CancelCascaseDeleting<ReceivedAdditionalScoreNotification, AdditionalScore, int?>(modelBuilder, c => c.Score, c => c.ScoreId, isRequired: false);

			CancelCascaseDeleting<NewCommentNotification, Comment, int>(modelBuilder, c => c.Comment, c => c.CommentId);
			CancelCascaseDeleting<LikedYourCommentNotification, Comment, int>(modelBuilder, c => c.Comment, c => c.CommentId);
			CancelCascaseDeleting<RepliedToYourCommentNotification, Comment, int>(modelBuilder, c => c.Comment, c => c.CommentId);
			CancelCascaseDeleting<RepliedToYourCommentNotification, Comment, int>(modelBuilder, c => c.ParentComment, c => c.ParentCommentId);
			
			CancelCascaseDeleting<UploadedPackageNotification, CourseVersion, Guid>(modelBuilder, c => c.CourseVersion, c => c.CourseVersionId);
			CancelCascaseDeleting<PublishedPackageNotification, CourseVersion, Guid>(modelBuilder, c => c.CourseVersion, c => c.CourseVersionId);

			CancelCascaseDeleting<CourseExportedToStepikNotification, StepikExportProcess, int>(modelBuilder, c => c.Process, c => c.ProcessId);
			
			CancelCascaseDeleting<XQueueWatcher, ApplicationUser, string>(modelBuilder, c => c.User, c => c.UserId);

			CancelCascaseDeleting<StepikExportProcess, ApplicationUser, string>(modelBuilder, c => c.Owner, c => c.OwnerId);

			CancelCascaseDeleting<NotificationTransport, ApplicationUser, string>(modelBuilder, c => c.User, c => c.UserId, isRequired: false);

			CancelCascaseDeleting<LabelOnGroup, Group, int>(modelBuilder, c => c.Group, c => c.GroupId);
			CancelCascaseDeleting<GroupLabel, ApplicationUser, string>(modelBuilder, c => c.Owner, c => c.OwnerId);
			CancelCascaseDeleting<LabelOnGroup, GroupLabel, int>(modelBuilder, c => c.Label, c => c.LabelId);
			
			CancelCascaseDeleting<SystemAccess, ApplicationUser, string>(modelBuilder, c => c.GrantedBy, c => c.GrantedById);
		}

		private static void CancelCascaseDeleting<T1, T2, T3>(DbModelBuilder modelBuilder, Expression<Func<T1, T2>> oneWay, Expression<Func<T1, T3>> secondWay, bool isRequired=true)
			where T1 : class
			where T2 : class
		{
			var entity = modelBuilder.Entity<T1>();
			var a = isRequired ? entity.HasRequired(oneWay).WithMany() : entity.HasOptional(oneWay).WithMany();
			a.HasForeignKey(secondWay).WillCascadeOnDelete(false);
		}

		/* Construct easy understandable message on DbEntityValidationException */
		public override int SaveChanges()
		{
			try
			{
				return base.SaveChanges();
			}
			catch (DbEntityValidationException ex)
			{
				throw GetDbEntityValidationExceptionWithDetails(ex);
			}
		}

		public override Task<int> SaveChangesAsync()
		{
			try
			{
				return base.SaveChangesAsync();
			}
			catch (DbEntityValidationException ex)
			{
				throw GetDbEntityValidationExceptionWithDetails(ex);
			}
		}

		private static DbEntityValidationException GetDbEntityValidationExceptionWithDetails(DbEntityValidationException ex)
		{
			var errorMessages = ex.EntityValidationErrors
				.SelectMany(x => x.ValidationErrors)
				.Select(x => x.ErrorMessage);

			var fullErrorMessage = string.Join("; ", errorMessages);
			var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

			// Returns a new DbEntityValidationException with the improved exception message.
			return new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
		}

		public DbSet<UserQuestion> UserQuestions { get; set; }
		public DbSet<SlideRate> SlideRates { get; set; }
		public DbSet<Visit> Visits { get; set; }
		public DbSet<SlideHint> Hints { get; set; }
		public DbSet<Like> SolutionLikes { get; set; }
		public DbSet<UserQuiz> UserQuizzes { get; set; }
		public DbSet<UnitAppearance> UnitAppearances { get; set; }
		public DbSet<TextBlob> Texts { get; set; }
		public DbSet<LtiConsumer> Consumers { get; set; }
		public DbSet<LtiSlideRequest> LtiRequests { get; set; }
		public DbSet<RestoreRequest> RestoreRequests { get; set; }
		public DbSet<UserRole> UserRoles { get; set; }

		public DbSet<Comment> Comments { get; set; }
		public DbSet<CommentLike> CommentLikes { get; set; }
		public DbSet<CommentsPolicy> CommentsPolicies { get; set; }

		public DbSet<QuizVersion> QuizVersions { get; set; }
		public DbSet<CourseVersion> CourseVersions { get; set; }

		public DbSet<ManualExerciseChecking> ManualExerciseCheckings { get; set; }
		public DbSet<AutomaticExerciseChecking> AutomaticExerciseCheckings { get; set; }
		public DbSet<ManualQuizChecking> ManualQuizCheckings { get; set; }
		public DbSet<AutomaticQuizChecking> AutomaticQuizCheckings { get; set; }
		public DbSet<UserExerciseSubmission> UserExerciseSubmissions { get; set; }
		public DbSet<ExerciseCodeReview> ExerciseCodeReviews { get; set; }
		public DbSet<ExerciseCodeReviewComment> ExerciseCodeReviewComments { get; set; }

		public DbSet<Group> Groups { get; set; }
		public DbSet<GroupMember> GroupMembers { get; set; }
		public DbSet<GroupLabel> GroupLabels { get; set; }
		public DbSet<LabelOnGroup> LabelsOnGroups { get; set; }
		public DbSet<GroupAccess> GroupAccesses { get; set; }

		public DbSet<CertificateTemplate> CertificateTemplates { get; set; }
		public DbSet<Certificate> Certificates { get; set; }

		public DbSet<AdditionalScore> AdditionalScores { get; set; }
		public DbSet<EnabledAdditionalScoringGroup> EnabledAdditionalScoringGroups { get; set; }

		public DbSet<GraderClient> GraderClients { get; set; }
		public DbSet<ExerciseSolutionByGrader> ExerciseSolutionsByGrader { get; set; }

		public DbSet<NotificationTransport> NotificationTransports { get; set; }
		public DbSet<NotificationTransportSettings> NotificationTransportSettings { get; set; }
		public DbSet<NotificationDelivery> NotificationDeliveries { get; set; }
		public DbSet<Notification> Notifications { get; set; }

		public DbSet<XQueueWatcher> XQueueWatchers { get; set; }
		public DbSet<XQueueExerciseSubmission> XQueueExerciseSubmissions { get; set; }

		public DbSet<FeedViewTimestamp> FeedViewTimestamps { get; set; }

		public DbSet<StepikAccessToken> StepikAccessTokens { get; set; }
		public DbSet<StepikExportProcess> StepikExportProcesses { get; set; }
		public DbSet<StepikExportSlideAndStepMap> StepikExportSlideAndStepMaps { get; set; }

		public DbSet<CourseAccess> CourseAccesses { get; set; }
		public DbSet<SystemAccess> SystemAccesses { get; set; }
	}
}
 