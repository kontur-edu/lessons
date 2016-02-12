namespace uLearn.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddComments1 : DbMigration
    {
        public override void Up()
        {
            RenameIndex(table: "dbo.Comments", name: "CommentBySlide", newName: "IDX_Comment_CommentBySlide");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Comments", name: "IDX_Comment_CommentBySlide", newName: "CommentBySlide");
        }
    }
}
