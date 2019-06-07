namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCourseFiles : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CourseFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CourseId = c.String(nullable: false, maxLength: 64),
                        CourseVersionId = c.Guid(nullable: false),
                        File = c.Binary(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CourseVersions", t => t.CourseVersionId, cascadeDelete: true)
                .Index(t => t.CourseVersionId, name: "IX_CourseFiles_CourseVersionId");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CourseFiles", "CourseVersionId", "dbo.CourseVersions");
            DropIndex("dbo.CourseFiles", "IX_CourseFiles_CourseVersionId");
            DropTable("dbo.CourseFiles");
        }
    }
}
