namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNotUploadedPackageNotification : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "NotUploadedPackageNotification_CommitHash", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "NotUploadedPackageNotification_CommitHash");
        }
    }
}
