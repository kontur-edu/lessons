namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSubmissionsLanguageAndAgentName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserExerciseSubmissions", "Language", c => c.Short(nullable: false));
            AddColumn("dbo.AutomaticExerciseCheckings", "CheckingAgentName", c => c.String(maxLength: 256));
            CreateIndex("dbo.UserExerciseSubmissions", "Language", name: "IDX_UserExerciseSubmissions_ByLanguage");
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserExerciseSubmissions", "IDX_UserExerciseSubmissions_ByLanguage");
            DropColumn("dbo.AutomaticExerciseCheckings", "CheckingAgentName");
            DropColumn("dbo.UserExerciseSubmissions", "Language");
        }
    }
}
