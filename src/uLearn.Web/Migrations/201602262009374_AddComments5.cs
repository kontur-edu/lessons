namespace uLearn.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddComments5 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Comments", new[] { "AuthorId" });
            CreateIndex("dbo.Comments", new[] { "AuthorId", "PublishTime" }, name: "IDX_Comment_ByAuthorAndPublishTime");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Comments", "IDX_Comment_ByAuthorAndPublishTime");
            CreateIndex("dbo.Comments", "AuthorId");
        }
    }
}
