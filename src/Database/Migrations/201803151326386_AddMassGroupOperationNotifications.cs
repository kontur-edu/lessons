namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMassGroupOperationNotifications : DbMigration
    {
        public override void Up()
        {
			RenameColumn(table: "dbo.Notifications", name: "GroupId2", newName: "GroupId3");
			RenameColumn(table: "dbo.Notifications", name: "GroupId1", newName: "GroupId2");
            RenameColumn(table: "dbo.Notifications", name: "GroupId", newName: "GroupId1");
			RenameIndex(table: "dbo.Notifications", name: "IX_GroupId2", newName: "IX_GroupId3");
			RenameIndex(table: "dbo.Notifications", name: "IX_GroupId1", newName: "IX_GroupId2");
            RenameIndex(table: "dbo.Notifications", name: "IX_GroupId", newName: "IX_GroupId1");
			
			AddColumn("dbo.Notifications", "GroupId", c => c.Int());
            AddColumn("dbo.Notifications", "UserIds", c => c.String());
            AddColumn("dbo.Notifications", "UserDescriptions", c => c.String());
            CreateIndex("dbo.Notifications", "GroupId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Notifications", new[] { "GroupId" });
            DropColumn("dbo.Notifications", "UserDescriptions");
            DropColumn("dbo.Notifications", "UserIds");
			DropColumn("dbo.Notifications", "GroupId");
            RenameIndex(table: "dbo.Notifications", name: "IX_GroupId1", newName: "IX_GroupId");
			RenameIndex(table: "dbo.Notifications", name: "IX_GroupId2", newName: "IX_GroupId1");
			RenameIndex(table: "dbo.Notifications", name: "IX_GroupId3", newName: "IX_GroupId2");
            RenameColumn(table: "dbo.Notifications", name: "GroupId1", newName: "GroupId");
			RenameColumn(table: "dbo.Notifications", name: "GroupId2", newName: "GroupId1");
			RenameColumn(table: "dbo.Notifications", name: "GroupId3", newName: "GroupId2");
        }
    }
}
