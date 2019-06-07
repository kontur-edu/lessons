namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCourseGitRepos : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CourseGitRepos",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CourseId = c.String(maxLength: 64),
                        RepoUrl = c.String(),
                        PublicKey = c.String(),
                        PrivateKey = c.String(),
                        IsWebhookEnabled = c.Boolean(nullable: false),
                        PathToCourseXml = c.String(),
                        CreateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CourseGitRepos");
        }
    }
}
