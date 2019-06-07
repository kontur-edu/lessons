namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserQuizSubmissions : DbMigration
    {
        public override void Up()
        {
            RenameIndex(table: "dbo.UserQuizs", name: "IDX_UserQuiz_ByItem", newName: "IDX_UserQuizAnswer_ByItem");
            CreateTable(
                "dbo.UserQuizSubmissions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        CourseId = c.String(nullable: false, maxLength: 40),
                        SlideId = c.Guid(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        IsDropped = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.CourseId, t.SlideId, t.UserId }, name: "IDX_UserQuizSubmission_BySlideAndUser")
                .Index(t => new { t.CourseId, t.SlideId }, name: "IDX_UserQuizSubmission_ByCourseAndSlide")
                .Index(t => new { t.CourseId, t.SlideId, t.Timestamp }, name: "IDX_UserQuizSubmission_BySlideAndTime");
            
            AddColumn("dbo.UserQuizs", "SubmissionId", c => c.Int(nullable: false));
            CreateIndex("dbo.AutomaticQuizCheckings", "Id");
            CreateIndex("dbo.ManualQuizCheckings", "Id");
            
			InitializeUserQuizSubmissions();
			AddSubmissionIdToUserQuizs();
			ReinitializeAutomaticQuizCheckings();
			ReinitializeManualQuizCheckings();
			
			AddForeignKey("dbo.AutomaticQuizCheckings", "Id", "dbo.UserQuizSubmissions", "Id");
			AddForeignKey("dbo.ManualQuizCheckings", "Id", "dbo.UserQuizSubmissions", "Id");
			AddForeignKey("dbo.UserQuizs", "SubmissionId", "dbo.UserQuizSubmissions", "Id", cascadeDelete: true);
			
			DropForeignKey("dbo.UserQuizs", "UserId", "dbo.AspNetUsers");
			DropIndex("dbo.UserQuizs", "FullIndex");
			DropIndex("dbo.UserQuizs", "IDX_UserQuiz_ByCourseSlideAndQuiz");
			DropIndex("dbo.UserQuizs", "IX_UserQuizs_CourseId_SlideId_QuizId");
			DropIndex("dbo.UserQuizs", "StatisticsIndex");

			RenameColumn("dbo.UserQuizs", "QuizId", "BlockId");
			CreateIndex("dbo.UserQuizs", new[] { "SubmissionId", "BlockId" }, name: "IDX_UserQuizAnswer_BySubmissionAndBlock");
            
			DropColumn("dbo.UserQuizs", "CourseId");
            DropColumn("dbo.UserQuizs", "SlideId");
            DropColumn("dbo.UserQuizs", "UserId");
            DropColumn("dbo.UserQuizs", "Timestamp");
            DropColumn("dbo.UserQuizs", "isDropped");
        }


		private void InitializeUserQuizSubmissions()
		{
			Sql("INSERT INTO UserQuizSubmissions (CourseId, SlideId, UserId, [Timestamp], IsDropped) " +
				"SELECT CourseId, SlideId, UserId, [Timestamp], IsDropped " +
				"FROM UserQuizs GROUP BY CourseId, SlideId, UserId, [Timestamp], IsDropped " +
				"ORDER BY [Timestamp]");
		}
		
		private void AddSubmissionIdToUserQuizs()
		{
			Sql("CREATE TABLE [dbo].[UserQuizsTemp](" +
				"[Id] [int] NOT NULL, " +
				"[CourseId] [nvarchar](64) NOT NULL, " +
				"[SlideId] [uniqueidentifier] NOT NULL, " +
				"[UserId] [nvarchar](128) NOT NULL, " +
				"[QuizId] [nvarchar](64) NULL, " +
				"[ItemId] [nvarchar](64) NULL, " +
				"[Text] [nvarchar](max) NULL, " +
				"[Timestamp] [datetime] NOT NULL, " +
				"[IsRightAnswer] [bit] NOT NULL, " +
				"[isDropped] [bit] NOT NULL, " +
				"[QuizBlockScore] [int] NOT NULL, " +
				"[QuizBlockMaxScore] [int] NOT NULL, " +
				"[SubmissionId] [int] NULL, " +
				"CONSTRAINT [PK_dbo.UserQuizsTemp] PRIMARY KEY CLUSTERED ([Id] ASC)" +
				");");
			
			Sql("INSERT INTO UserQuizsTemp " +
				"SELECT a.Id, a.CourseId, a.SlideId, a.UserId, a.QuizId, a.ItemId, a.Text, a.Timestamp, a.IsRightAnswer, a.isDropped, a.QuizBlockScore, a.QuizBlockMaxScore, s.Id as SubmissionId " +
				"FROM UserQuizs a " +
				"LEFT JOIN UserQuizSubmissions s ON a.CourseId = s.CourseId AND a.SLideId = s.SlideId AND a.UserId = s.UserId AND a.Timestamp = s.Timestamp " +
				"ORDER BY a.Timestamp");
			
			Sql("DELETE FROM UserQuizs");
			
			Sql("SET IDENTITY_INSERT UserQuizs ON");
			
			Sql("INSERT INTO UserQuizs (Id, CourseId, SlideId, UserId, QuizId, ItemId, Text, Timestamp, IsRightAnswer, isDropped, QuizBlockScore, QuizBlockMaxScore, SubmissionId) " +
				"SELECT * FROM UserQuizsTemp");
			
			Sql("SET IDENTITY_INSERT UserQuizs OFF");
			
			/* We have no permissions for DBCC CHECKIDENT or the production database */
			// Sql("DBCC CHECKIDENT('UserQuizs', RESEED)");
			
			Sql("DROP TABLE UserQuizsTemp");
		}

		private void ReinitializeAutomaticQuizCheckings()
		{
			Sql("CREATE TABLE [dbo].[AutomaticQuizCheckingsTemp](" +
				"[Id] [int]  NOT NULL," +
				"[CourseId] [nvarchar](64) NOT NULL," +
				"[SlideId] [uniqueidentifier] NOT NULL," +
				"[Timestamp] [datetime] NOT NULL," +
				"[UserId] [nvarchar](128) NOT NULL," +
				"[Score] [int] NOT NULL," +
				"CONSTRAINT [PK_dbo.AutomaticQuizCheckingsTemp] PRIMARY KEY CLUSTERED ([Id] ASC)" +
				")");
			
			Sql("INSERT INTO AutomaticQuizCheckingsTemp " +
				"SELECT o.Id, u.CourseId, u.SlideId, u.Timestamp, u.UserId, u.Score FROM (" +
				"SELECT CourseId, SlideId, UserId, [Timestamp], IsDropped, SUM(QuizBlockScore) as Score FROM (" +
				"SELECT CourseId, SlideId, UserId, [Timestamp], IsDropped, MAX(QuizBlockScore) as QuizBlockScore " +
				"FROM UserQuizs " +
				"GROUP BY CourseId, SlideId, UserId, [Timestamp], IsDropped, QuizId " +
				") s " +
				"GROUP BY s.CourseId, s.SlideId, s.UserId, s.Timestamp, s.IsDropped " +
				") u " +
				"JOIN UserQuizSubmissions o ON u.CourseId = o.CourseId AND u.SlideId = o.SlideId AND u.UserId = o.UserId AND u.Timestamp = o.Timestamp " +
				/* SlideIds are manual-checked quizzes from the production and staging servers */
				"WHERE u.SlideId NOT IN (\'064935E3-F3AC-4D72-9E5D-FBC5A2190159\', \'0B39ED79-19ED-4F1D-8EEF-D2FA73C6C953\', \'11E8CE2D-E642-4BA0-A5B7-83580540CBB9\', \'240E60B3-6AC4-4410-AE85-42333A8F7B09\', \'240E60B3-6AC4-4410-AE85-48333A8F7B09\', \'2819D940-74EE-43E7-B93A-6A0DF1D07E96\', \'363180BA-CBF1-4B2E-A975-CCC0FDD1505C\', \'41138C54-848F-4F49-856B-B738B7E39D29\', \'438A9EA1-18BE-4754-AAC0-1AF06F37BBEF\', \'438A9EA1-18BE-4754-AAC0-2AF06F37BBEF\', \'4699BB67-0B6A-4408-9CEE-795CB3507565\', \'507D73D1-1AE0-442D-ADB1-465259D4F3F1\', \'5C3D1FEB-38BB-4ACD-9830-BB67D5980668\', \'5DAFB150-5068-49DC-860A-7E40795B0D0F\', \'60A4EE5C-8EF7-4830-81A1-82DBC854E417\', \'61308F6A-277F-4EF3-8553-383386C94DDF\', \'641E81E4-8AB7-497C-BA6A-0F83B453E392\', \'6E4BF02A-7750-4082-9522-3F3134E069C1\', \'76F91D1D-C194-4BB8-9605-4B769D0C5169\', \'86A11184-A112-49AE-AEB8-7103C5BD2383\', \'936FC848-B281-4D00-B9AA-036337C87D4B\', \'95037B75-373E-4FD1-98B6-BDE6DD20D033\', \'A879D997-00E3-4025-A33B-2E9DE8D4B2F1\', \'BAD549C9-AAD5-41F0-98F8-112A9BF02E82\', \'BD3A414D-B5D7-49F5-8AF3-05D598FCC618\', \'C1F4998A-159F-4627-8719-7D72DCF094E0\', \'C4F2563F-4042-47DC-BE8D-1BBDEEA1ECEE\', \'C73E7098-B544-437C-996D-0F777975AE98\', \'C86D1E83-B227-447C-AEE0-CED64B878421\', \'C9BFC83E-BA91-4BEC-85C3-3C4E5A2805A0\', \'D6DB513E-63C2-430C-93FD-BA06E51D2989\', \'D7AF8B48-DE37-428A-A7C9-E4872877B112\', \'EE47F0D4-0975-4169-9024-28D367C72295\', \'F35323D9-5544-4F2A-A57F-32F5C511B06D\') " +
				"ORDER BY Timestamp");
			
			/* Copy AutomaticQuizCheckingsTemp to AutomaticQuizCheckings */
			
			Sql("DELETE FROM AutomaticQuizCheckings");
			
			Sql("SET IDENTITY_INSERT AutomaticQuizCheckings ON");
			
			Sql("INSERT INTO AutomaticQuizCheckings (Id, CourseId, SlideId, Timestamp, UserId, Score) " +
				"SELECT * FROM AutomaticQuizCheckingsTemp");
			
			Sql("SET IDENTITY_INSERT AutomaticQuizCheckings OFF");
			
			// Sql("DBCC CHECKIDENT('AutomaticQuizCheckings', RESEED)");
			
			Sql("DROP TABLE AutomaticQuizCheckingsTemp");
		}

		private void ReinitializeManualQuizCheckings()
		{
			Sql("CREATE TABLE [dbo].[ManualQuizCheckingsTemp](" +
				"[Id] [int] NOT NULL," +
				"[CourseId] [nvarchar](64) NOT NULL," +
				"[SlideId] [uniqueidentifier] NOT NULL," +
				"[Timestamp] [datetime] NOT NULL," +
				"[UserId] [nvarchar](128) NOT NULL," +
				"[LockedUntil] [datetime] NULL," +
				"[LockedById] [nvarchar](128) NULL," +
				"[IsChecked] [bit] NOT NULL," +
				"[Score] [int] NOT NULL," +
				" CONSTRAINT [PK_dbo.ManualQuizCheckingsTemp] PRIMARY KEY CLUSTERED ([Id] ASC)" +
				")");
			
			Sql("INSERT INTO ManualQuizCheckingsTemp " +
				"SELECT ISNULL(MAX(q.SubmissionId), MIN(q2.SubmissionId)) as Id, c.CourseId, c.SlideId, c.Timestamp, c.UserId, c.LockedUntil, c.LockedById, c.IsChecked, c.Score " +
				"FROM ManualQuizCheckings c " +
				"LEFT JOIN UserQuizs q ON c.CourseId = q.CourseId AND c.SlideId = q.SlideId AND c.UserId = q.UserId AND c.Timestamp >= q.Timestamp " +
				"LEFT JOIN UserQuizs q2 ON c.CourseId = q2.CourseId AND c.SlideId = q2.SlideId AND c.UserId = q2.UserId AND c.Timestamp < q2.Timestamp " +
				"GROUP BY c.CourseId, c.SlideId, c.Timestamp, c.UserId, c.LockedUntil, c.LockedById, c.IsChecked, c.Score");
			
			/* Delete notifications (it's too difficult to restore these links) */
			Sql("DELETE FROM NotificationDeliveries WHERE NotificationId IN (SELECT Id FROM Notifications WHERE Discriminator = 'PassedManualQuizCheckingNotification')");
			Sql("DELETE FROM Notifications WHERE Discriminator = 'PassedManualQuizCheckingNotification'");
			
			/* Copy ManualQuizCheckingsTemp to ManualQuizCheckings */
			
			Sql("DELETE FROM ManualQuizCheckings");
			
			Sql("SET IDENTITY_INSERT ManualQuizCheckings ON");
			
			Sql("INSERT INTO ManualQuizCheckings (Id, CourseId, SlideId, Timestamp, UserId, LockedUntil, LockedById, IsChecked, Score) " +
				"SELECT * FROM ManualQuizCheckingsTemp");
			
			Sql("SET IDENTITY_INSERT ManualQuizCheckings OFF");
			
			// Sql("DBCC CHECKIDENT('ManualQuizCheckings', RESEED)");
			
			Sql("DROP TABLE ManualQuizCheckingsTemp");
		}
		
		public override void Down()
        {
            AddColumn("dbo.UserQuizs", "isDropped", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserQuizs", "Timestamp", c => c.DateTime(nullable: false));
            AddColumn("dbo.UserQuizs", "QuizId", c => c.String(maxLength: 64));
            AddColumn("dbo.UserQuizs", "UserId", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.UserQuizs", "SlideId", c => c.Guid(nullable: false));
            AddColumn("dbo.UserQuizs", "CourseId", c => c.String(nullable: false, maxLength: 64));
            DropForeignKey("dbo.UserQuizs", "SubmissionId", "dbo.UserQuizSubmissions");
            DropForeignKey("dbo.UserQuizSubmissions", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ManualQuizCheckings", "Id", "dbo.UserQuizSubmissions");
            DropForeignKey("dbo.AutomaticQuizCheckings", "Id", "dbo.UserQuizSubmissions");
            DropIndex("dbo.UserQuizs", "IDX_UserQuizAnswer_BySubmissionAndBlock");
            DropIndex("dbo.ManualQuizCheckings", new[] { "Id" });
            DropIndex("dbo.UserQuizSubmissions", "IDX_UserQuizSubmission_BySlideAndTime");
            DropIndex("dbo.UserQuizSubmissions", "IDX_UserQuizSubmission_ByCourseAndSlide");
            DropIndex("dbo.UserQuizSubmissions", "IDX_UserQuizSubmission_BySlideAndUser");
            DropIndex("dbo.AutomaticQuizCheckings", new[] { "Id" });
            DropColumn("dbo.UserQuizs", "BlockId");
            DropColumn("dbo.UserQuizs", "SubmissionId");
            DropTable("dbo.UserQuizSubmissions");
            RenameIndex(table: "dbo.UserQuizs", name: "IDX_UserQuizAnswer_ByItem", newName: "IDX_UserQuiz_ByItem");
            CreateIndex("dbo.UserQuizs", new[] { "SlideId", "Timestamp" }, name: "StatisticsIndex");
            CreateIndex("dbo.UserQuizs", new[] { "CourseId", "SlideId", "QuizId" }, name: "IDX_UserQuiz_ByCourseSlideAndQuiz");
            CreateIndex("dbo.UserQuizs", new[] { "CourseId", "SlideId", "UserId", "isDropped", "QuizId", "ItemId" }, name: "FullIndex");
            AddForeignKey("dbo.UserQuizs", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
    }
}
