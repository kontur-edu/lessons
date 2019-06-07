namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCourseIdToUserQuzzesIndex : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.UserQuizs", "FullIndex");
            CreateIndex("dbo.UserQuizs", new[] { "CourseId", "SlideId", "UserId", "isDropped", "QuizId", "ItemId" }, name: "FullIndex");
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserQuizs", "FullIndex");
            CreateIndex("dbo.UserQuizs", new[] { "UserId", "SlideId", "isDropped", "QuizId", "ItemId" }, name: "FullIndex");
        }
    }
}
