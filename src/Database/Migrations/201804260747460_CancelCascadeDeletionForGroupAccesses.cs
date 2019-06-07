namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CancelCascadeDeletionForGroupAccesses : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.GroupAccesses", "IDX_GroupAccess_ByGroupUserAndIsEnabled");
            DropIndex("dbo.GroupAccesses", "IDX_GroupAccess_ByUser");
            DropIndex("dbo.GroupAccesses", new[] { "GrantedById" });
            AlterColumn("dbo.GroupAccesses", "UserId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.GroupAccesses", "GrantedById", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.GroupAccesses", new[] { "GroupId", "UserId", "IsEnabled" }, name: "IDX_GroupAccess_ByGroupUserAndIsEnabled");
            CreateIndex("dbo.GroupAccesses", "UserId", name: "IDX_GroupAccess_ByUser");
            CreateIndex("dbo.GroupAccesses", "GrantedById");
        }
        
        public override void Down()
        {
            DropIndex("dbo.GroupAccesses", new[] { "GrantedById" });
            DropIndex("dbo.GroupAccesses", "IDX_GroupAccess_ByUser");
            DropIndex("dbo.GroupAccesses", "IDX_GroupAccess_ByGroupUserAndIsEnabled");
            AlterColumn("dbo.GroupAccesses", "GrantedById", c => c.String(maxLength: 128));
            AlterColumn("dbo.GroupAccesses", "UserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.GroupAccesses", "GrantedById");
            CreateIndex("dbo.GroupAccesses", "UserId", name: "IDX_GroupAccess_ByUser");
            CreateIndex("dbo.GroupAccesses", new[] { "GroupId", "UserId", "IsEnabled" }, name: "IDX_GroupAccess_ByGroupUserAndIsEnabled");
        }
    }
}
