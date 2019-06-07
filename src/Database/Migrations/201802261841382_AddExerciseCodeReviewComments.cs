namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExerciseCodeReviewComments : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Notifications", name: "CommentId", newName: "CommentId1");
            RenameIndex(table: "dbo.Notifications", name: "IX_CommentId", newName: "IX_CommentId1");
            CreateTable(
                "dbo.ExerciseCodeReviewComments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReviewId = c.Int(nullable: false),
                        Text = c.String(nullable: false),
                        AuthorId = c.String(nullable: false, maxLength: 128),
                        IsDeleted = c.Boolean(nullable: false),
                        AddingTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.AuthorId)
                .ForeignKey("dbo.ExerciseCodeReviews", t => t.ReviewId, cascadeDelete: true)
                .Index(t => new { t.ReviewId, t.IsDeleted }, name: "IDX_ExerciseCodeReviewComment_ByReviewAndIsDeleted")
                .Index(t => t.AuthorId)
                .Index(t => t.AddingTime, name: "IDX_ExerciseCodeReview_ByAddingTime");
            
            AddColumn("dbo.ExerciseCodeReviews", "AddingTime", c => c.DateTime(nullable: false));
			AddColumn("dbo.Notifications", "CommentId", c => c.Int());
            CreateIndex("dbo.Notifications", "CommentId");
            AddForeignKey("dbo.Notifications", "CommentId", "dbo.ExerciseCodeReviewComments", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Notifications", "CommentId", "dbo.ExerciseCodeReviewComments");
            DropForeignKey("dbo.ExerciseCodeReviewComments", "ReviewId", "dbo.ExerciseCodeReviews");
            DropForeignKey("dbo.ExerciseCodeReviewComments", "AuthorId", "dbo.AspNetUsers");
            DropIndex("dbo.Notifications", new[] { "CommentId" });
            DropIndex("dbo.ExerciseCodeReviewComments", "IDX_ExerciseCodeReview_ByAddingTime");
            DropIndex("dbo.ExerciseCodeReviewComments", new[] { "AuthorId" });
            DropIndex("dbo.ExerciseCodeReviewComments", "IDX_ExerciseCodeReviewComment_ByReviewAndIsDeleted");
            DropColumn("dbo.ExerciseCodeReviews", "AddingTime");
            DropTable("dbo.ExerciseCodeReviewComments");
            RenameIndex(table: "dbo.Notifications", name: "IX_CommentId1", newName: "IX_CommentId");
            RenameColumn(table: "dbo.Notifications", name: "CommentId1", newName: "CommentId");
        }
    }
}
