namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveDeleteRestrictionForUser : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserRoles", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.UserRoles", new[] { "UserId" });
            AlterColumn("dbo.UserRoles", "UserId", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.UserRoles", "UserId");
            AddForeignKey("dbo.UserRoles", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserRoles", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.UserRoles", new[] { "UserId" });
            AlterColumn("dbo.UserRoles", "UserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.UserRoles", "UserId");
            AddForeignKey("dbo.UserRoles", "UserId", "dbo.AspNetUsers", "Id");
        }
    }
}
