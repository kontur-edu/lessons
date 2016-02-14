namespace uLearn.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddComments3 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Comments", name: "Author_Id", newName: "AuthorId");
            RenameIndex(table: "dbo.Comments", name: "IX_Author_Id", newName: "IX_AuthorId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Comments", name: "IX_AuthorId", newName: "IX_Author_Id");
            RenameColumn(table: "dbo.Comments", name: "AuthorId", newName: "Author_Id");
        }
    }
}
