namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRepoUrl2NotUploadedPackageNotification : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "NotUploadedPackageNotification_RepoUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "NotUploadedPackageNotification_RepoUrl");
        }
    }
}
