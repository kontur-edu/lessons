namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameUserQuizs : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.UserQuizs", newName: "UserQuizAnswers");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.UserQuizAnswers", newName: "UserQuizs");
        }
    }
}
