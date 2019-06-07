namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCourseGitBranchField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CourseGitRepos", "Branch", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CourseGitRepos", "Branch");
        }
    }
}
