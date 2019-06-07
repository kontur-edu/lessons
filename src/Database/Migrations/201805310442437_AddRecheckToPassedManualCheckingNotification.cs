namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRecheckToPassedManualCheckingNotification : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "IsRecheck", c => c.Boolean(defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "IsRecheck");
        }
    }
}
