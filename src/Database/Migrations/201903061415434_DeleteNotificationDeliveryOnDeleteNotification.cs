namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteNotificationDeliveryOnDeleteNotification : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.NotificationDeliveries", "NotificationId", "dbo.Notifications");
            AddForeignKey("dbo.NotificationDeliveries", "NotificationId", "dbo.Notifications", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NotificationDeliveries", "NotificationId", "dbo.Notifications");
            AddForeignKey("dbo.NotificationDeliveries", "NotificationId", "dbo.Notifications", "Id");
        }
    }
}
