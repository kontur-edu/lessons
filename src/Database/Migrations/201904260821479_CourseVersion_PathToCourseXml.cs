namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CourseVersion_PathToCourseXml : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CourseVersions", "PathToCourseXml", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CourseVersions", "PathToCourseXml");
        }
    }
}
