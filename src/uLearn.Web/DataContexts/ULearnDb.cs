using System;
using System.Data.Entity;
using System.Linq.Expressions;
using EntityFramework.Functions;
using Microsoft.AspNet.Identity.EntityFramework;
using uLearn.Web.Migrations;
using uLearn.Web.Models;

namespace uLearn.Web.DataContexts
{
	public class ULearnDb : IdentityDbContext<ApplicationUser>
	{
		public ULearnDb()
			: base("DefaultConnection", throwIfV1Schema: false)
		{
			Database.SetInitializer(new MigrateDatabaseToLatestVersion<ULearnDb, Configuration>());
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

			CancelCascaseDeleting<ExerciseCodeReview, ApplicationUser, string>(modelBuilder, c => c.Author, c => c.AuthorId);

			modelBuilder.Entity<Like>()
				.HasRequired(l => l.Submission)
				.WithMany(s => s.Likes)
				.HasForeignKey(l => l.SubmissionId)
				.WillCascadeOnDelete(false);

			CancelCascaseDeleting<UserExerciseSubmission, ApplicationUser, string>(modelBuilder, c => c.User, c => c.UserId);
			CancelCascaseDeleting<ManualExerciseChecking, ApplicationUser, string>(modelBuilder, c => c.User, c => c.UserId);

			CancelCascaseDeleting<Certificate, ApplicationUser, string>(modelBuilder, c => c.User, c => c.UserId);
			CancelCascaseDeleting<Certificate, ApplicationUser, string>(modelBuilder, c => c.Instructor, c => c.InstructorId);

			CancelCascaseDeleting<AdditionalScore, ApplicationUser, string>(modelBuilder, c => c.User, c => c.UserId);
			CancelCascaseDeleting<AdditionalScore, ApplicationUser, string>(modelBuilder, c => c.Instructor, c => c.InstructorId);

			CancelCascaseDeleting<GraderClient, ApplicationUser, string>(modelBuilder, c => c.User, c => c.UserId);
		}

		private static void CancelCascaseDeleting<T1, T2, T3>(DbModelBuilder modelBuilder, Expression<Func<T1, T2>> oneWay, Expression<Func<T1, T3>> secondWay)
			where T1 : class
			where T2 : class
		{
			modelBuilder.Entity<T1>()
				.HasRequired(oneWay)
				.WithMany()
				.HasForeignKey(secondWay)
				.WillCascadeOnDelete(false);
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

		public DbSet<Group> Groups { get; set; }
		public DbSet<GroupMember> GroupMembers { get; set; }

		public DbSet<CertificateTemplate> CertificateTemplates { get; set; }
		public DbSet<Certificate> Certificates { get; set; }

		public DbSet<AdditionalScore> AdditionalScores { get; set; }
		public DbSet<EnabledAdditionalScoringGroup> EnabledAdditionalScoringGroups { get; set; }

		public DbSet<GraderClient> GraderClients { get; set; }
		public DbSet<ExerciseSolutionByGrader> ExerciseSolutionsByGrader { get; set; }
	}
}
