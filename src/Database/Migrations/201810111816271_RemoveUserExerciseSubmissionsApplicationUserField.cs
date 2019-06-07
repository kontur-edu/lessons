namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUserExerciseSubmissionsApplicationUserField : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserExerciseSubmissions", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.UserExerciseSubmissions", new[] { "ApplicationUser_Id" });
            DropColumn("dbo.UserExerciseSubmissions", "ApplicationUser_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserExerciseSubmissions", "ApplicationUser_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.UserExerciseSubmissions", "ApplicationUser_Id");
            AddForeignKey("dbo.UserExerciseSubmissions", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
