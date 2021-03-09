namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserFlashcardsUnlocking : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.UserFlashcardsUnlocking",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        CourseId = c.String(nullable: false, maxLength: 100),
                        UnitId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.UserId, t.CourseId, t.UnitId }, name: "User_Course_Unit");
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.UserFlashcardsUnlocking", "UserId", "public.AspNetUsers");
            DropIndex("public.UserFlashcardsUnlocking", "User_Course_Unit");
            DropTable("public.UserFlashcardsUnlocking");
        }
    }
}
