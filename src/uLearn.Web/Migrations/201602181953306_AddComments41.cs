namespace uLearn.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddComments41 : DbMigration
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
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CommentsPolicies");
        }
    }
}
