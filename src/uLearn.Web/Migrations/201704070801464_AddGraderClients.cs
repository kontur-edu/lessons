namespace uLearn.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGraderClients : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExerciseSolutionByGraders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientId = c.Guid(nullable: false),
                        SubmissionId = c.Int(nullable: false),
                        ClientUserId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GraderClients", t => t.ClientId, cascadeDelete: true)
                .ForeignKey("dbo.UserExerciseSubmissions", t => t.SubmissionId, cascadeDelete: true)
                .Index(t => t.ClientId)
                .Index(t => t.SubmissionId);
            
            CreateTable(
                "dbo.GraderClients",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CourseId = c.String(nullable: false, maxLength: 64),
                        Name = c.String(nullable: false, maxLength: 100),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);

			Sql("CREATE FULLTEXT CATALOG ExerciseSolutionByGradersFullTextCatalog WITH ACCENT_SENSITIVITY = OFF", true);
			Sql("CREATE FULLTEXT INDEX ON dbo.ExerciseSolutionByGraders ([ClientUserId] LANGUAGE [Russian]) KEY INDEX [PK_dbo.ExerciseSolutionByGraders] ON (ExerciseSolutionByGradersFullTextCatalog, FILEGROUP[PRIMARY]) WITH (CHANGE_TRACKING = AUTO, STOPLIST = SYSTEM)", true);
			Sql("CREATE FUNCTION dbo.GetGraderSolutionByClientUserId (@userId NVARCHAR(4000)) RETURNS TABLE RETURN SELECT [Id] FROM dbo.ExerciseSolutionByGraders WHERE CONTAINS([ClientUserId], @userId)");
		}
        
        public override void Down()
        {
			Sql("DROP FUNCTION dbo.GetGraderSolutionByClientUserId");
			Sql("DROP FULLTEXT INDEX ON dbo.ExerciseSolutionByGraders", true);
			Sql("DROP FULLTEXT CATALOG ExerciseSolutionByGradersFullTextCatalog", true);

			DropForeignKey("dbo.ExerciseSolutionByGraders", "SubmissionId", "dbo.UserExerciseSubmissions");
            DropForeignKey("dbo.ExerciseSolutionByGraders", "ClientId", "dbo.GraderClients");
            DropForeignKey("dbo.GraderClients", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.GraderClients", new[] { "UserId" });
            DropIndex("dbo.ExerciseSolutionByGraders", new[] { "SubmissionId" });
            DropIndex("dbo.ExerciseSolutionByGraders", new[] { "ClientId" });
            DropTable("dbo.GraderClients");
            DropTable("dbo.ExerciseSolutionByGraders");
        }
    }
}
