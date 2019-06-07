namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCourseIdToLtiRequests : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.LtiSlideRequests", "IDX_LtiSlideRequest_SlideAndUser");
            AddColumn("dbo.LtiSlideRequests", "CourseId", c => c.String(nullable: false, maxLength: 100));
            CreateIndex("dbo.LtiSlideRequests", new[] { "CourseId", "SlideId", "UserId" }, name: "IDX_LtiSlideRequest_SlideAndUser");
        }
        
        public override void Down()
        {
            DropIndex("dbo.LtiSlideRequests", "IDX_LtiSlideRequest_SlideAndUser");
            DropColumn("dbo.LtiSlideRequests", "CourseId");
            CreateIndex("dbo.LtiSlideRequests", new[] { "SlideId", "UserId" }, name: "IDX_LtiSlideRequest_SlideAndUser");
        }
    }
}
