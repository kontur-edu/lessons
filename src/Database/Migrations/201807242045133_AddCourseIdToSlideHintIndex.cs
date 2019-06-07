namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCourseIdToSlideHintIndex : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.SlideHints", "FullIndex");
            CreateIndex("dbo.SlideHints", new[] { "CourseId", "SlideId", "UserId", "HintId", "IsHintHelped" }, name: "FullIndex");
        }
        
        public override void Down()
        {
            DropIndex("dbo.SlideHints", "FullIndex");
            CreateIndex("dbo.SlideHints", new[] { "SlideId", "UserId", "HintId", "IsHintHelped" }, name: "FullIndex");
        }
    }
}
