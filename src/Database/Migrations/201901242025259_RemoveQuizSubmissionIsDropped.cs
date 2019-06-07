namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveQuizSubmissionIsDropped : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.UserQuizSubmissions", "IsDropped");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserQuizSubmissions", "IsDropped", c => c.Boolean(nullable: false));
        }
    }
}
