namespace uLearn.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddComments2 : DbMigration
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CommentLikes", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.CommentLikes", "CommentId", "dbo.Comments");
            DropIndex("dbo.CommentLikes", "IDX_CommentLike_ByComment");
            DropIndex("dbo.CommentLikes", "IDX_CommentLike_ByUserAndComment");
            DropTable("dbo.CommentLikes");
        }
    }
}
