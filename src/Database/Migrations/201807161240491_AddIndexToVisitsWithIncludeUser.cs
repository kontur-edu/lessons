namespace Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIndexToVisitsWithIncludeUser : DbMigration
    {
		private string indexName = "IDX_Visits_BySlideAndTime_IncludeUser";

        public override void Up()
		{
			Sql($"CREATE NONCLUSTERED INDEX [{indexName}] ON [dbo].[Visits] ([SlideId], [Timestamp]) INCLUDE ([UserId])");
		}
        
        public override void Down()
        {
			Sql($"DROP INDEX [{indexName}]");
        }
    }
}
