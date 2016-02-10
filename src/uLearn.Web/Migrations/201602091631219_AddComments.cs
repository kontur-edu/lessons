namespace uLearn.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddComments : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CourseId = c.String(nullable: false, maxLength: 64),
                        SlideId = c.String(nullable: false, maxLength: 64),
                        PublishTime = c.DateTime(nullable: false),
                        Text = c.String(nullable: false),
                        IsVisibleForEveryone = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsPinnedToTop = c.Boolean(nullable: false),
                        ParentCommentId = c.Int(nullable: false),
                        Author_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Author_Id, cascadeDelete: true)
                .Index(t => t.SlideId, name: "CommentBySlide")
                .Index(t => t.Author_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Comments", "Author_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Comments", new[] { "Author_Id" });
            DropIndex("dbo.Comments", "CommentBySlide");
            DropTable("dbo.Comments");
        }
    }
}
