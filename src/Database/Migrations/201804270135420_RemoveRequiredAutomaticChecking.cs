namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveRequiredAutomaticChecking : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserExerciseSubmissions", "AutomaticCheckingId", "dbo.AutomaticExerciseCheckings");
            DropIndex("dbo.UserExerciseSubmissions", new[] { "AutomaticCheckingId" });
            AlterColumn("dbo.UserExerciseSubmissions", "AutomaticCheckingId", c => c.Int());
            CreateIndex("dbo.UserExerciseSubmissions", "AutomaticCheckingId");
            AddForeignKey("dbo.UserExerciseSubmissions", "AutomaticCheckingId", "dbo.AutomaticExerciseCheckings", "Id");
			Sql("UPDATE dbo.UserExerciseSubmissions SET Language = 1 WHERE Language = 0");
        }
        
        public override void Down()
        {
			Sql("UPDATE dbo.UserExerciseSubmissions SET Language = 0 WHERE Language = 1");
            DropForeignKey("dbo.UserExerciseSubmissions", "AutomaticCheckingId", "dbo.AutomaticExerciseCheckings");
            DropIndex("dbo.UserExerciseSubmissions", new[] { "AutomaticCheckingId" });
            AlterColumn("dbo.UserExerciseSubmissions", "AutomaticCheckingId", c => c.Int(nullable: false));
            CreateIndex("dbo.UserExerciseSubmissions", "AutomaticCheckingId");
            AddForeignKey("dbo.UserExerciseSubmissions", "AutomaticCheckingId", "dbo.AutomaticExerciseCheckings", "Id", cascadeDelete: true);
        }
    }
}
