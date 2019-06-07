namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveIdentityFromQuizCheckingsIds : DbMigration
    {
        public override void Up()
		{
			RemoveIdentityFromAutomaticQuizCheckings();
			RemoveIdentityFromManualQuizCheckings();

			CreateIndex("dbo.AutomaticQuizCheckings", new[] { "CourseId", "SlideId" }, name: "IDX_AbstractSlideChecking_AbstractSlideCheckingBySlide");
			CreateIndex("dbo.AutomaticQuizCheckings", new[] { "CourseId", "SlideId", "Timestamp" }, name: "IDX_AbstractSlideChecking_AbstractSlideCheckingBySlideAndTime");
			CreateIndex("dbo.AutomaticQuizCheckings", new[] { "CourseId", "SlideId", "UserId" }, name: "IDX_AbstractSlideChecking_AbstractSlideCheckingBySlideAndUser");
			CreateIndex("dbo.ManualQuizCheckings", new[] { "CourseId", "SlideId" }, name: "IDX_AbstractSlideChecking_AbstractSlideCheckingBySlide");
			CreateIndex("dbo.ManualQuizCheckings", new[] { "CourseId", "SlideId", "Timestamp" }, name: "IDX_AbstractSlideChecking_AbstractSlideCheckingBySlideAndTime");
			CreateIndex("dbo.ManualQuizCheckings", new[] { "CourseId", "SlideId", "UserId" }, name: "IDX_AbstractSlideChecking_AbstractSlideCheckingBySlideAndUser");

			AddForeignKey("dbo.AutomaticQuizCheckings", "UserId", "dbo.AspNetUsers", "Id");
			AddForeignKey("dbo.ManualQuizCheckings", "UserId", "dbo.AspNetUsers", "Id");
			AddForeignKey("dbo.ManualQuizCheckings", "LockedById", "dbo.AspNetUsers", "Id");
		}

		private void RemoveIdentityFromAutomaticQuizCheckings()
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

			Sql("INSERT INTO AutomaticQuizCheckingsTemp SELECT * FROM AutomaticQuizCheckings");

			Sql("DROP TABLE AutomaticQuizCheckings");

			RenameTable("AutomaticQuizCheckingsTemp", "AutomaticQuizCheckings");
		}

		private void RemoveIdentityFromManualQuizCheckings()
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

			Sql("INSERT INTO ManualQuizCheckingsTemp SELECT * FROM ManualQuizCheckings");

			DropForeignKey("dbo.Notifications", "CheckingId1", "dbo.ManualQuizCheckings");

			Sql("DROP TABLE ManualQuizCheckings");

			RenameTable("ManualQuizCheckingsTemp", "ManualQuizCheckings");

			AddForeignKey("dbo.Notifications", "PassedManualQuizCheckingNotification_CheckingId", "dbo.ManualQuizCheckings", "Id");
		}

		public override void Down()
        {
            // Nothing here
        }
    }
}
