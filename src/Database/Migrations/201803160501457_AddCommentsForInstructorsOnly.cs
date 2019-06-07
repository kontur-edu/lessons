namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCommentsForInstructorsOnly : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Comments", "IsForInstructorsOnly", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Comments", "IsForInstructorsOnly");
        }
    }
}
