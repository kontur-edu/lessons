namespace uLearn.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddComments4 : DbMigration
    {
        public override void Up()
        {
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
            
            AddColumn("dbo.Comments", "IsApproved", c => c.Boolean(nullable: false));
            AddColumn("dbo.Comments", "IsCorrectAnswer", c => c.Boolean(nullable: false));
            DropColumn("dbo.Comments", "IsVisibleForEveryone");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Comments", "IsVisibleForEveryone", c => c.Boolean(nullable: false));
            DropColumn("dbo.Comments", "IsCorrectAnswer");
            DropColumn("dbo.Comments", "IsApproved");
            DropTable("dbo.CommentsPolicies");
        }
    }
}
