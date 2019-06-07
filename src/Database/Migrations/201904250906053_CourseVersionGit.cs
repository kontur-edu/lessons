namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CourseVersionGit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CourseVersions", "RepoUrl", c => c.String());
            AddColumn("dbo.CourseVersions", "CommitHash", c => c.String(maxLength: 40));
            AddColumn("dbo.CourseVersions", "Description", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CourseVersions", "Description");
            DropColumn("dbo.CourseVersions", "CommitHash");
            DropColumn("dbo.CourseVersions", "RepoUrl");
        }
    }
}
