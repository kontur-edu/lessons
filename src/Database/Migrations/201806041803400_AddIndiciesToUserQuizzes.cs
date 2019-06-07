namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIndiciesToUserQuizzes : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.UserQuizs", new[] { "CourseId", "SlideId", "QuizId" }, name: "IDX_UserQuiz_ByCourseSlideAndQuiz");
            CreateIndex("dbo.UserQuizs", "ItemId", name: "IDX_UserQuiz_ByItem");
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserQuizs", "IDX_UserQuiz_ByItem");
            DropIndex("dbo.UserQuizs", "IDX_UserQuiz_ByCourseSlideAndQuiz");
        }
    }
}
