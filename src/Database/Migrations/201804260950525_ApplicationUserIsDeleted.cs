namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ApplicationUserIsDeleted : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IsDeleted", c => c.Boolean(nullable: false));
            CreateIndex("dbo.AspNetUsers", "IsDeleted", name: "IDX_ApplicationUser_ByIsDeleted");
        }
        
        public override void Down()
        {
            DropIndex("dbo.AspNetUsers", "IDX_ApplicationUser_ByIsDeleted");
            DropColumn("dbo.AspNetUsers", "IsDeleted");
        }
    }
}
