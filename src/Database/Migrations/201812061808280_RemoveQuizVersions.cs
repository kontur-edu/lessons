namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveQuizVersions : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserQuizs", "QuizVersionId", "dbo.QuizVersions");
            DropIndex("dbo.QuizVersions", "IDX_QuizVersion_QuizVersionBySlide");
            DropIndex("dbo.QuizVersions", "IDX_QuizVersion_QuizVersionBySlideAndTime");
            DropIndex("dbo.UserQuizs", new[] { "QuizVersionId" });
            DropColumn("dbo.UserQuizs", "QuizVersionId");
            DropTable("dbo.QuizVersions");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.QuizVersions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CourseId = c.String(nullable: false, maxLength: 64),
                        SlideId = c.Guid(nullable: false),
                        NormalizedXml = c.String(nullable: false),
                        LoadingTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.UserQuizs", "QuizVersionId", c => c.Int());
            CreateIndex("dbo.UserQuizs", "QuizVersionId");
            CreateIndex("dbo.QuizVersions", new[] { "SlideId", "LoadingTime" }, name: "IDX_QuizVersion_QuizVersionBySlideAndTime");
            CreateIndex("dbo.QuizVersions", "SlideId", name: "IDX_QuizVersion_QuizVersionBySlide");
            AddForeignKey("dbo.UserQuizs", "QuizVersionId", "dbo.QuizVersions", "Id");
        }
    }
}
