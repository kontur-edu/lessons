namespace uLearn.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddComments : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CommentLikes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        CommentId = c.Int(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Comments", t => t.CommentId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.UserId, t.CommentId }, unique: true, name: "IDX_CommentLike_ByUserAndComment")
                .Index(t => t.CommentId, name: "IDX_CommentLike_ByComment");
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CourseId = c.String(nullable: false, maxLength: 64),
                        SlideId = c.String(nullable: false, maxLength: 64),
                        AuthorId = c.String(nullable: false, maxLength: 128),
                        PublishTime = c.DateTime(nullable: false),
                        Text = c.String(nullable: false),
                        IsApproved = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        IsCorrectAnswer = c.Boolean(nullable: false),
                        IsPinnedToTop = c.Boolean(nullable: false),
                        ParentCommentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.AuthorId, cascadeDelete: true)
                .Index(t => t.SlideId, name: "IDX_Comment_CommentBySlide")
                .Index(t => new { t.AuthorId, t.PublishTime }, name: "IDX_Comment_ByAuthorAndPublishTime");
            
            CreateTable(
                "dbo.CommentsPolicies",
                c => new
                    {
                        CourseId = c.String(nullable: false, maxLength: 64),
                        IsCommentsEnabled = c.Boolean(nullable: false),
                        ModerationPolicy = c.Int(nullable: false),
                        OnlyInstructorsCanReply = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.CourseId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CommentLikes", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CommentLikes", "CommentId", "dbo.Comments");
            DropForeignKey("dbo.Comments", "AuthorId", "dbo.AspNetUsers");
            DropIndex("dbo.Comments", "IDX_Comment_ByAuthorAndPublishTime");
            DropIndex("dbo.Comments", "IDX_Comment_CommentBySlide");
            DropIndex("dbo.CommentLikes", "IDX_CommentLike_ByComment");
            DropIndex("dbo.CommentLikes", "IDX_CommentLike_ByUserAndComment");
            DropTable("dbo.CommentsPolicies");
            DropTable("dbo.Comments");
            DropTable("dbo.CommentLikes");
        }
    }
}
