namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameColumnsForEfCoreCompatibility : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Notifications", name: "CourseVersionId1", newName: "UploadedPackageNotification_CourseVersionId");
            RenameColumn(table: "dbo.Notifications", name: "GroupId2", newName: "GroupMemberHasBeenRemovedNotification_GroupId");
            RenameColumn(table: "dbo.Notifications", name: "GroupId3", newName: "JoinedToYourGroupNotification_GroupId");
            RenameColumn(table: "dbo.Notifications", name: "CheckingId1", newName: "PassedManualQuizCheckingNotification_CheckingId");
            RenameColumn(table: "dbo.Notifications", name: "AccessId1", newName: "RevokedAccessToGroupNotification_AccessId");
            RenameIndex(table: "dbo.Notifications", name: "IX_CourseVersionId1", newName: "IX_UploadedPackageNotification_CourseVersionId");
            RenameIndex(table: "dbo.Notifications", name: "IX_GroupId2", newName: "IX_GroupMemberHasBeenRemovedNotification_GroupId");
            RenameIndex(table: "dbo.Notifications", name: "IX_GroupId3", newName: "IX_JoinedToYourGroupNotification_GroupId");
            RenameIndex(table: "dbo.Notifications", name: "IX_CheckingId1", newName: "IX_PassedManualQuizCheckingNotification_CheckingId");
            RenameIndex(table: "dbo.Notifications", name: "IX_AccessId1", newName: "IX_RevokedAccessToGroupNotification_AccessId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Notifications", name: "IX_RevokedAccessToGroupNotification_AccessId", newName: "IX_AccessId1");
            RenameIndex(table: "dbo.Notifications", name: "IX_PassedManualQuizCheckingNotification_CheckingId", newName: "IX_CheckingId1");
            RenameIndex(table: "dbo.Notifications", name: "IX_JoinedToYourGroupNotification_GroupId", newName: "IX_GroupId3");
            RenameIndex(table: "dbo.Notifications", name: "IX_GroupMemberHasBeenRemovedNotification_GroupId", newName: "IX_GroupId2");
            RenameIndex(table: "dbo.Notifications", name: "IX_UploadedPackageNotification_CourseVersionId", newName: "IX_CourseVersionId1");
            RenameColumn(table: "dbo.Notifications", name: "RevokedAccessToGroupNotification_AccessId", newName: "AccessId1");
            RenameColumn(table: "dbo.Notifications", name: "PassedManualQuizCheckingNotification_CheckingId", newName: "CheckingId1");
            RenameColumn(table: "dbo.Notifications", name: "JoinedToYourGroupNotification_GroupId", newName: "GroupId3");
            RenameColumn(table: "dbo.Notifications", name: "GroupMemberHasBeenRemovedNotification_GroupId", newName: "GroupId2");
            RenameColumn(table: "dbo.Notifications", name: "UploadedPackageNotification_CourseVersionId", newName: "CourseVersionId1");
        }
    }
}
