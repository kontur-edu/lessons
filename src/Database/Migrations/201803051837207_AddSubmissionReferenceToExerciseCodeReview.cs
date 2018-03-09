namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSubmissionReferenceToExerciseCodeReview : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ExerciseCodeReviews", "ExerciseCheckingId", "dbo.ManualExerciseCheckings");
            DropIndex("dbo.ExerciseCodeReviews", "IDX_ExerciseCodeReview_ByManualExerciseChecking");
            AddColumn("dbo.ExerciseCodeReviews", "SubmissionId", c => c.Int());
            AlterColumn("dbo.ExerciseCodeReviews", "ExerciseCheckingId", c => c.Int());
            CreateIndex("dbo.ExerciseCodeReviews", "ExerciseCheckingId", name: "IDX_ExerciseCodeReview_ByManualExerciseChecking");
            CreateIndex("dbo.ExerciseCodeReviews", "SubmissionId", name: "IDX_ExerciseCodeReview_BySubmission");
            AddForeignKey("dbo.ExerciseCodeReviews", "SubmissionId", "dbo.UserExerciseSubmissions", "Id");
            AddForeignKey("dbo.ExerciseCodeReviews", "ExerciseCheckingId", "dbo.ManualExerciseCheckings", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExerciseCodeReviews", "ExerciseCheckingId", "dbo.ManualExerciseCheckings");
            DropForeignKey("dbo.ExerciseCodeReviews", "SubmissionId", "dbo.UserExerciseSubmissions");
            DropIndex("dbo.ExerciseCodeReviews", "IDX_ExerciseCodeReview_BySubmission");
            DropIndex("dbo.ExerciseCodeReviews", "IDX_ExerciseCodeReview_ByManualExerciseChecking");
            AlterColumn("dbo.ExerciseCodeReviews", "ExerciseCheckingId", c => c.Int(nullable: false));
            DropColumn("dbo.ExerciseCodeReviews", "SubmissionId");
            CreateIndex("dbo.ExerciseCodeReviews", "ExerciseCheckingId", name: "IDX_ExerciseCodeReview_ByManualExerciseChecking");
            AddForeignKey("dbo.ExerciseCodeReviews", "ExerciseCheckingId", "dbo.ManualExerciseCheckings", "Id", cascadeDelete: true);
        }
    }
}
